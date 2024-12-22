using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Company.Function
{
    public static class DurableFunctionsOrchestrationCSharp1
    {
        [Function(nameof(DurableFunctionsOrchestrationCSharp1))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context,
             IDictionary<string, string> query)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(DurableFunctionsOrchestrationCSharp1));
            logger.LogInformation("Saying hello.");
            List<string> outputs = new List<string>();
            string? organizationId = "test";
            query?.TryGetValue("organization", out organizationId);
            // Replace name and input with values relevant for your Durable Functions Activity

            outputs.Add("Some OrgId");
            outputs.Add($"{organizationId}");
            //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), organizationId ?? "test"));
            //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "Seattle"));
            //outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        //[Function(nameof(SayHello))]
        //public static string SayHello([ActivityTrigger] string name, FunctionContext executionContext, IDictionary<string, string> query)
        //{
        //    ILogger logger = executionContext.GetLogger("SayHello");
        //    logger.LogInformation("Saying hello to {name}.", name);
        //    return $"Hello {name}!";
        //}

        [Function("DurableFunctionsOrchestrationCSharp1_HttpStart")]
        [OpenApiOperation(operationId: nameof(DurableFunctionsOrchestrationCSharp1_HttpStart), tags: new[] { nameof(DurableFunctionsOrchestrationCSharp1_HttpStart) })]
        [OpenApiParameter(name: "organization", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The Organization comma delimited **id** parameter to filter on")]
        public static async Task<HttpResponseData> DurableFunctionsOrchestrationCSharp1_HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "export/new")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            IDictionary<string, string> query)
        {
            ILogger logger = executionContext.GetLogger("DurableFunctionsOrchestrationCSharp1_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(DurableFunctionsOrchestrationCSharp1), query);

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId, HttpStatusCode.Accepted);
        }
    }
}
