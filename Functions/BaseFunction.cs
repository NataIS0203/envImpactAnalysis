using Durable.Models;
using Durable.Utilities;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Durable.Api.Functions
{
    public class BaseFunction
    {
        protected async Task<HttpResponseData> HandleSuccessResponse(HttpRequestData req, object data, HttpStatusCode status = HttpStatusCode.OK)
        {
            return await GetResponse(req, data, status);
        }

        protected async Task<HttpResponseData> HandleCreatedResponse(HttpRequestData req, object data, HttpStatusCode status = HttpStatusCode.Created)
        {
            return await GetResponse(req, data, status, null);
        }

        protected async Task<HttpResponseData> HandleCreatedResponse(HttpRequestData req, object data, string location, HttpStatusCode status = HttpStatusCode.Created)
        {
            return await GetResponse(req, data, status, location);
        }

        protected async Task<HttpResponseData> HandleNotFoundResponse(HttpRequestData req, HttpStatusCode status = HttpStatusCode.NotFound)
        {
            return await GetResponse(req, null, status);
        }

        protected async Task<HttpResponseData> HandleNotFoundResponse(HttpRequestData req, object data, HttpStatusCode status = HttpStatusCode.NotFound)
        {
            return await GetResponse(req, data, status);
        }

        protected async Task<HttpResponseData> HandleValidationResponse(HttpRequestData req, ValidationResult validationResult, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            List<KeyValuePair<string, string>> propertyMessages = validationResult.Errors.Select(e => new KeyValuePair<string, string>(e.PropertyName, e.ErrorMessage)).ToList();

            return await GetErrorResponse(req, propertyMessages, statusCode);
        }

        protected async Task<HttpResponseData> HandleDerivedValidationResponse(HttpRequestData req, ValidationResult validationResult)
        {
            IEnumerable<IGrouping<string, ValidationFailure>> validationErrorsByCode = validationResult.Errors.GroupBy(e => e.ErrorCode);

            IEnumerable<ValidationFailure>? badRequestErrors = validationErrorsByCode.FirstOrDefault(x => x.Key == ((int)HttpStatusCode.BadRequest).ToString())?.Select(x => x);

            if (!badRequestErrors.IsNullOrEmpty())
            {
                return await HandleValidationResponse(req, new ValidationResult(badRequestErrors), HttpStatusCode.BadRequest);
            }

            IEnumerable<ValidationFailure>? notFoundErrors = validationErrorsByCode.FirstOrDefault(x => x.Key == ((int)HttpStatusCode.NotFound).ToString())?.Select(x => x);

            if (!notFoundErrors.IsNullOrEmpty())
            {
                return await HandleValidationResponse(req, new ValidationResult(notFoundErrors), HttpStatusCode.NotFound);
            }

            IEnumerable<ValidationFailure>? conflictErrors = validationErrorsByCode.FirstOrDefault(x => x.Key == ((int)HttpStatusCode.Conflict).ToString())?.Select(x => x);

            return !conflictErrors.IsNullOrEmpty()
                ? await HandleValidationResponse(req, new ValidationResult(conflictErrors), HttpStatusCode.Conflict)
                : await HandleValidationResponse(req, validationResult);
        }

        protected string GetValidationError(ValidationResult validationResult)
        {
            IEnumerable<string> propertyMessages = validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}");

            return string.Join(Environment.NewLine, propertyMessages);
        }

        protected async Task<HttpResponseData> HandleValidationResponse(HttpRequestData req, string propertyName, string errorMessage, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            ValidationResult validationResult = new ValidationResult();
            validationResult.Errors.Add(new ValidationFailure(propertyName, errorMessage));

            return await HandleValidationResponse(req, validationResult, statusCode);
        }

        protected async Task<HttpResponseData> HandleErrorResponse(HttpRequestData req, Exception ex, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            if (ex is JsonPatchException patchException)
            {
                string patchPath = patchException.FailedOperation.path;
                object patchValue = patchException.FailedOperation.value;

                return await GetErrorResponse(req, $"value", $"Patch path [{patchPath}] has invalid value [{patchValue}]", HttpStatusCode.BadRequest);
            }

            if (ex is System.Text.Json.JsonException jsonException)
            {
                object? property = jsonException.GetType()?.GetProperty("Path")?.GetValue(jsonException, null);

                return await GetErrorResponse(req, $"{property}", $"Property [{property}] has invalid value", HttpStatusCode.BadRequest);
            }

            return ex is UnauthorizedException
                ? await GetErrorResponse(req, null, ex.Message, HttpStatusCode.Unauthorized)
                : await GetErrorResponse(req, null, ex.Message, statusCode);
        }

        protected async Task<HttpResponseData> HandleErrorResponse(HttpRequestData req, Response response, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
        {
            return await GetResponse(req, response, statusCode);
        }

        private async Task<HttpResponseData> GetResponse(HttpRequestData req, object data, HttpStatusCode status, string location = null)
        {
            HttpResponseData result = req.CreateResponse();

            if (!location.IsNullOrEmpty())
            {
                result.Headers.Add("Location", location);
            }

            return result;
        }
        private async Task<HttpResponseData> GetErrorResponse(HttpRequestData req, List<KeyValuePair<string, string>> errorMessages, HttpStatusCode status)
        {
            Response response = new Response()
            {
                Errors = errorMessages.Select(s => new ResponseError
                {
                    ErrorCode = (int)status,
                    Property = s.Key,
                    Message = s.Value,
                }).ToList(),
            };

            return await GetResponse(req, response, status);
        }

        private async Task<HttpResponseData> GetErrorResponse(HttpRequestData req, string property, string message, HttpStatusCode status)
        {
            List<KeyValuePair<string, string>> errorMessages = new List<KeyValuePair<string, string>>()
            {
                new (property, message),
            };

            return await GetErrorResponse(req, errorMessages, status);
        }
    }
}
