using AutoMapper;
using Durable.Api.Functions;
using Durable.Functions.Models;
using Durable.Functions.Validators;
using Durable.Records;
using Durable.Service.Models;
using Durable.Services.Interfaces;
using Durable.Utilities;
using FluentValidation.Results;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;

namespace Durable.Functions
{
    public class EnvImpactDurableFunction(
        ILogger<EnvImpactDurableFunction> logger,
        IMapper mapper,
        IEnvImpactReportService envImpactReportService,
        IServiceProviderValidatorFactory validatorFactory,
        IMemoryCache memoryCache) : BaseFunction
    {
        private readonly ILogger<EnvImpactDurableFunction> _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IEnvImpactReportService _envImpactReportService = envImpactReportService;
        private readonly IServiceProviderValidatorFactory _validatorFactory = validatorFactory;
        private readonly IMemoryCache _memoryCache = memoryCache;

        [Function(nameof(EnvImpactDurableFunction))]
        public async Task<string> RunOrchestrator(
             [OrchestrationTrigger] TaskOrchestrationContext context,
             GetBaseRequest request)
        {
            _logger.LogInformation($"Running orcestration for {request.ReportName} report.");
            string outputs = await context.CallActivityAsync<string>(nameof(GetReportsRun), request);
            return outputs;
        }

        [Function(nameof(GetReportsRun))]
        public async Task<string> GetReportsRun(
            [ActivityTrigger] GetBaseRequest request)
        {
            _logger.LogInformation($"GetReportsRun for report {request.ReportName}.");
            string cacheKey = string.Concat("filename", request.ReportName, request.Name, request.Region, request.Percentage, request.Additional, request.ImageType);
            string filename = (string)ServiceUtilities.GetCachedObject(_memoryCache, cacheKey);
            if (!filename.IsNullOrEmpty())
            {
                return filename;
            }

            var outputs = await _envImpactReportService.GetReportAsync(_mapper.Map<ReportBaseModel>(request));

            ServiceUtilities.SetCachedObject(_memoryCache, cacheKey, outputs);

            return outputs;
        }

        [Function("GetSpeciesEnvImpact")]
        [OpenApiOperation(operationId: nameof(GetSpeciesEnvImpact), tags: new[] { nameof(GetSpeciesEnvImpact) })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The Specie parameter to filter on")]
        [OpenApiParameter(name: "region", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The Region parameter to filter on")]
        [OpenApiParameter(name: "percentage", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The Percentage parameter to filter on")]
        public async Task<HttpResponseData> GetSpeciesEnvImpact(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "species")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            IDictionary<string, string> query)
        {
            _logger.LogInformation($"GetSpeciesEnvImpact with query: {query?.Keys}");

            (GetBaseRequest reportRequest, ValidationResult validationResult) = await GetBaseRequestAsync(query, ReportTypeReport.Species.Name);

            if (!validationResult.IsValid)
            {
                return await HandleValidationResponse(req, validationResult);
            }


            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(EnvImpactDurableFunction), reportRequest);

            _logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId, HttpStatusCode.Accepted);
        }

        [Function("GetResourcesEnvImpact")]
        [OpenApiOperation(operationId: nameof(GetResourcesEnvImpact), tags: new[] { nameof(GetResourcesEnvImpact) })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The Resource parameter to filter on")]
        [OpenApiParameter(name: "region", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The Region parameter to filter on")]
        [OpenApiParameter(name: "percentage", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The Percentage parameter to filter on")]
        public async Task<HttpResponseData> GetResourcesEnvImpact(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resources")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            IDictionary<string, string> query)
        {
            _logger.LogInformation($"GetResourcesEnvImpact with query: {query?.Keys}");
            (GetBaseRequest reportRequest, ValidationResult validationResult) = await GetBaseRequestAsync(query, ReportTypeReport.Resources.Name);

            if (!validationResult.IsValid)
            {
                return await HandleValidationResponse(req, validationResult);
            }

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(EnvImpactDurableFunction), reportRequest);

            _logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId, HttpStatusCode.Accepted);
        }

        [Function("GetImageEnvImpact")]
        [OpenApiOperation(operationId: nameof(GetImageEnvImpact), tags: new[] { nameof(GetImageEnvImpact) })]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The Resource parameter to filter on")]
        [OpenApiParameter(name: "additional", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The Additional parameter to filter on")]
        [OpenApiParameter(name: "type", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The Type parameter to filter on")]
        public async Task<HttpResponseData> GetImageEnvImpact(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "images")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext,
            IDictionary<string, string> query)
        {
            _logger.LogInformation($"GetImageEnvImpact with query: {query?.Keys}");
            (GetBaseRequest reportRequest, ValidationResult validationResult) = await GetBaseRequestAsync(query, ReportTypeReport.Images.Name);

            if (!validationResult.IsValid)
            {
                return await HandleValidationResponse(req, validationResult);
            }

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(EnvImpactDurableFunction), reportRequest);

            _logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId, HttpStatusCode.Accepted);
        }

        private static async Task<(GetBaseRequest reportRequest, ValidationResult validationResult)> GetBaseRequestAsync(IDictionary<string, string> query, string reportName)
        {
            query.TryGetValue("name", out string name);
            query.TryGetValue("region", out string? region);
            query.TryGetValue("percentage", out string? percentage);
            query.TryGetValue("additional", out string? additional);
            query.TryGetValue("Type", out string? type);
            var imageType = string.Empty;
            switch (type)
            {
                case "1":
                    imageType = ImageTypeRecord.Photography.Name;
                    break;
                case "2":
                    imageType = ImageTypeRecord.Drawing.Name;
                    break;
                case "3":
                    imageType = ImageTypeRecord.Painting.Name;
                    break;
            }
            GetBaseRequest result = new()
            {
                Name = name,
                Region = region,
                Percentage = percentage,
                Additional = additional,
                ImageType = imageType,
                ReportName = reportName,
            };

            GetBaseRequestValidator validator = new();
            ValidationResult validationResult = validator.Validate(result);

            return (result, validationResult);
        }
    }
}
