using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

public class ActivityTrackingMiddleware : IFunctionsWorkerMiddleware
{
    public static ActivitySource Source = new ActivitySource("AzureFunctionsWorker");
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        using var activity = Source.StartActivity("Function Executed");
        activity?.SetTag(FunctionActivityConstants.FaasName, context.FunctionDefinition.Name);
        activity?.SetTag(FunctionActivityConstants.Entrypoint, context.FunctionDefinition.EntryPoint);
        activity?.SetTag(FunctionActivityConstants.Id, context.FunctionDefinition.Id);
        activity?.SetTag(FunctionActivityConstants.InvocationId, context.InvocationId);

        await next.Invoke(context);
    }
}

internal static class FunctionActivityConstants
{
   public const string FaasName = "faas.name";
   public const string Entrypoint = "azure.function.entrypoint";
   public const string Id = "azure.function.id";
   public const string InvocationId = "azure.function.invocation_id";
   
}