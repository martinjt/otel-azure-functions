using System.Diagnostics;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Trace;

public class ActivityTrackingMiddleware : IFunctionsWorkerMiddleware
{
    public static ActivitySource Source = new ActivitySource("AzureFunctionsWorker");
    private readonly IConfiguration _configuration;

    public ActivityTrackingMiddleware(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        Activity? activity;
        if (context.FunctionDefinition.InputBindings.TryGetValue("req", out BindingMetadata? metadata) &&
            metadata.Type == "httpTrigger" &&
            await context.GetHttpRequestDataAsync() is { } requestData)
        {
            var route = GetRoute(context, requestData);
            activity = Source.StartActivity($"{requestData.Method.ToUpper()} {context.FunctionDefinition.Name}");
            if (activity != null)
            {
                activity.SetTag(TraceSemanticConventions.AttributeHttpRoute, route);
                activity.SetTag(TraceSemanticConventions.AttributeHttpMethod, requestData.Method);
                activity.SetTag(TraceSemanticConventions.AttributeHttpTarget, requestData.Url);
                activity.SetTag(TraceSemanticConventions.AttributeNetHostName, requestData.Url.Host);
                activity.SetTag(TraceSemanticConventions.AttributeNetHostPort, requestData.Url.Port);
                activity.SetTag(TraceSemanticConventions.AttributeHttpScheme, requestData.Url.Scheme);
                activity.SetTag(TraceSemanticConventions.AttributeHttpRequestContentLength, requestData.Body.Length);
            }
        }
        else
            activity = Source.StartActivity("Function Executed", ActivityKind.Server);

        if (activity != null)
        {
            activity.SetTag(TraceSemanticConventions.AttributeFaasInvokedName, context.FunctionDefinition.Name);
            activity.SetTag(TraceSemanticConventions.AttributeFaasExecution, context.InvocationId);
            activity.SetTag(FunctionActivityConstants.Entrypoint, context.FunctionDefinition.EntryPoint);
            activity.SetTag(FunctionActivityConstants.Id, context.FunctionDefinition.Id);
        }

        try
        {
            await next.Invoke(context);
        }
        finally
        {
            if (context.GetHttpResponseData() is { } responseData)
            {
                activity?.SetTag(TraceSemanticConventions.AttributeHttpStatusCode, responseData.StatusCode);
                activity?.SetTag(TraceSemanticConventions.AttributeHttpResponseContentLength, responseData.Body.Length);
            }
            if (activity != null)
                activity.Dispose();
        }
    }

    private static string GetRoute(FunctionContext context, HttpRequestData requestData)
    {
        var entrypointArray = context.FunctionDefinition.EntryPoint.Split(".").ToList();
        var entryPointClass = string.Join(".", entrypointArray
            .ToList()
            .GetRange(0, entrypointArray.Count() - 1));
        var entryPointMethod = entrypointArray.Last();
        var entrypoint =
            Assembly.LoadFrom(context.FunctionDefinition.PathToAssembly)?
                    .GetType(entryPointClass)?
                    .GetMethod(entryPointMethod);

        if (entrypoint == null)
            return "";

        var parameters = entrypoint.GetParameters();
        var httpTriggerAttribute = parameters[0]
            .GetCustomAttribute<HttpTriggerAttribute>();

        if (string.IsNullOrEmpty(httpTriggerAttribute?.Route))
            return requestData.Url.AbsolutePath;

        return $"/{httpTriggerAttribute?.Route!}";
    }
}

internal static class FunctionActivityConstants
{
    public const string Entrypoint = "azure.function.entrypoint";
    public const string Id = "azure.function.id";
}