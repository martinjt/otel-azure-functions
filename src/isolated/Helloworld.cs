using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace isolated
{
    public class Helloworld
    {
        private readonly ILogger _logger;

        public Helloworld(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Helloworld>();
        }

        [Function("hello-world")]
        public HttpResponseData HelloWorld([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hello-world")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
