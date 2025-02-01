using Durable.Records;
using Durable.Service.Models;
using Durable.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using MongoDB.Driver;

using System.IO;
using System.Linq;
using MongoDB.Bson;
using Durable.Utilities;
using System.Net.Http.Headers;
using System.Text;

namespace Durable.Services
{
    public class EnvImpactReportService(ILogger<EnvImpactReportService> logger, IMongoDbRepository mongoDbRepository, IHttpClientFactory httpClientFactory) : IEnvImpactReportService
    {
        private readonly ILogger<EnvImpactReportService> _logger = logger;
        private readonly IMongoDbRepository _mongoDbRepository = mongoDbRepository;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<string> GetReportAsync(ReportBaseModel model)
        {
            try
            {
                var prompts = await GetPromptsAsync($"{model.ReportName}");

                prompts.Questions = model.ReportName.Equals(ReportTypeReport.Images.Name)
                    ? prompts.Questions
                .Select(x => string.Format(x, model.ImageType, model.Name, model.Additional))
                .ToList()
                    : prompts.Questions
                .Select(x => string.Format(x, model.Name, model.Region, model.Percentage))
                .ToList();

                List<string> openAIResponses = model.ReportName.Equals(ReportTypeReport.Images.Name)
                    ? await GetImageOpenAIResponse(prompts)
                    : await GetOpenAIResponse(prompts);

                if (openAIResponses.Count > 0)
                {
                    // Set a variable to the Documents path.
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    string fileName = model.ReportName.Equals(ReportTypeReport.Images.Name)
                        ? Path.Combine(docPath, $"{model.ReportName}-{model.ImageType}-{model.Name}with{model.Additional}.txt")
                        : Path.Combine(docPath, $"{model.ReportName}-{model.Name}{model.Region ?? ""}With{model.Percentage}Percentage.txt");

                    // Append text to an existing file named "WriteLines.txt".
                    using (StreamWriter outputFile = (File.Exists(fileName))
                        ? File.AppendText(fileName)
                        : File.CreateText(fileName))
                    {
                        foreach (var line in openAIResponses)
                        {
                            outputFile.WriteLine(line);
                        }
                    }
                    return fileName;

                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return string.Empty;
        }


        public async Task<HttpResponseMessage> Execute(string url, object body = null, string authToken = null)
        {
            HttpResponseMessage? response = null;

            using (HttpClient httpClient = _httpClientFactory.CreateClient("default"))
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (authToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                }

                if (body != null)
                {
                    if (!(body is string))
                    {
                        body = JsonConvert.SerializeObject(body);
                    }

                    StringContent content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(url, content);
                }
                else
                {
                    response = await httpClient.GetAsync(url);
                }

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{nameof(Execute)}: Successfully hit URL: '{url}'");
                }
                else
                {
                    _logger.LogError($"{nameof(Execute)}: Failed to hit URL: '{url}'. Response: {(int)response.StatusCode + " : " + response.ReasonPhrase}");
                }
            }

            return response;
        }
        private async Task<PromptModel> GetPromptsAsync(string promptsName)
        {
            var promptDocuments = await _mongoDbRepository.GetQueryByFilter("types", promptsName);
            var list = new List<string>();
            var prompt = new PromptModel { Questions = list };
            promptDocuments.ForEach(z =>
            {
                var promptTemp = JsonConvert.DeserializeObject<PromptModel>(z.ToString());
                list.AddRange(promptTemp?.Questions);
            });

            return prompt;
        }

        private async Task<List<string>> GetOpenAIResponse(PromptModel prompts)
        {
            var result = new List<string>();
            try
            {
                ChatClient client = new(model: Environment.GetEnvironmentVariable("ChatGPTModel"), apiKey: Environment.GetEnvironmentVariable("OpenAPIKey"));

                var promprAssitChatMessage = prompts.Questions.Select(z => new AssistantChatMessage(Environment.GetEnvironmentVariable("AssistantPrompt")));
                var promprChatMessage = prompts.Questions.Select(z => new UserChatMessage(z));
                var messages = new List<ChatMessage>();
                messages.AddRange(promprAssitChatMessage);
                messages.AddRange(promprChatMessage);
                AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(messages);

                string currentBlock = string.Empty;
                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        var currLine = completionUpdate.ContentUpdate[0].Text;
                        currentBlock = string.Concat(currentBlock, currLine);
                        if (currLine.Equals("\n"))
                        {
                            result.Add(currentBlock);
                            currentBlock = string.Empty;
                        }
                    }
                }
                result.Add(currentBlock);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return result;
        }


        private async Task<List<string>> GetImageOpenAIResponse(PromptModel prompts)
        {
            var result = new List<string>();
            try
            {
                ImageClient client = new (model: Environment.GetEnvironmentVariable("ImageDallEModel"), apiKey: Environment.GetEnvironmentVariable("OpenAPIKey"));

                var promprAssitChatMessage = prompts.Questions.Select(z => new AssistantChatMessage(Environment.GetEnvironmentVariable("AssistantPrompt")));
                var promprMessage = prompts.Questions.FirstOrDefault() ?? "random";
                var imageOptions = new ImageGenerationOptions();
                // Initialize cancellation token
                var cancellationToken = new CancellationToken();
                var completionUpdates = await client.GenerateImageAsync(promprMessage, imageOptions, cancellationToken);

                var generations = completionUpdates.Value;

                result.Add(completionUpdates.Value.ImageUri.ToString());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return result;
        }
    }
}
