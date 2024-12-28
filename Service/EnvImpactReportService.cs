using AutoMapper;
using Durable.Records;
using Durable.Service.Models;
using Durable.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI.Chat;
using OpenAI.Images;
using System.ClientModel;
using System.IO;
using System.Linq;

namespace Durable.Services
{
    public class EnvImpactReportService : IEnvImpactReportService
    {
        private readonly ILogger<EnvImpactReportService> _logger;
        public EnvImpactReportService(ILogger<EnvImpactReportService> logger)
        {
            _logger = logger;
        }

        public async Task<string> GetReportAsync(ReportBaseModel model)
        {
            try
            {
                var prompts = await GetPromptsAsync($"{model.ReportName}PromptsFileName");

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

                if (openAIResponses.Any())
                {
                    // Set a variable to the Documents path.
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    string fileName = model.ReportName.Equals(ReportTypeReport.Images.Name)
                        ? Path.Combine(docPath, $"{model.ReportName}-{model.ImageType}-{model.Name}with{model.Additional}.txt")
                        : Path.Combine(docPath, $"{model.ReportName}-{model.Name}{model.Region ?? ""}With{model.Percentage.ToString()}Percentage.txt");

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

        private async Task<PromptModel> GetPromptsAsync(string promptsFileName)
        {
            string location = Environment.GetEnvironmentVariable(promptsFileName);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            PromptModel prompts;
            using (StreamReader r = new StreamReader(location))
            {
                string json = r.ReadToEnd();
                prompts = JsonConvert.DeserializeObject<PromptModel>(json);
            }

            return prompts;
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
                ImageClient client = new (model: Environment.GetEnvironmentVariable("ImageChatGPTModel"), apiKey: Environment.GetEnvironmentVariable("OpenAPIKey"));

                var promprAssitChatMessage = prompts.Questions.Select(z => new AssistantChatMessage(Environment.GetEnvironmentVariable("AssistantPrompt")));
                var promprMessage = prompts.Questions.FirstOrDefault()??"random";
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
