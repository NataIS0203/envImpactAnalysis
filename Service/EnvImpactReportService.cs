using AutoMapper;
using Durable.Service.Models;
using Durable.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.ClientModel;

namespace Durable.Services
{
    public class EnvImpactReportService : IEnvImpactReportService
    {
        private readonly ILogger<EnvImpactReportService> _logger;
        private readonly IMapper _mapper;
        public EnvImpactReportService(ILogger<EnvImpactReportService> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<string> GetReportAsync(
            string name,
            string? region,
            int percentage,
            string reportName)
        {
            List<PromptModel> prompts = await GetPromptsAsync($"{reportName}PromptsFileName");

            List<string> openAIResponses = await GetOpenAIResponse(prompts);

            // Set a variable to the Documents path.
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string fileName = Path.Combine(docPath, name, region ?? "", $"With{percentage.ToString()}Percentage.txt");

            // Append text to an existing file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(fileName, true))
            {
                outputFile.WriteLine("Fourth Line");
            }
            return fileName;
        }

        private async Task<List<PromptModel>> GetPromptsAsync(string promptsFileName)
        {
            string location = Environment.GetEnvironmentVariable(promptsFileName);
            List<PromptModel>? prompts = new List<PromptModel>();
            using (StreamReader r = new StreamReader(location))
            {
                string json = r.ReadToEnd();
                prompts = JsonConvert.DeserializeObject<List<PromptModel>>(json);
            }
            return prompts;
        }

        private async Task<List<string>> GetOpenAIResponse(List<PromptModel> prompts)
        {
            List<string> result = new List<string>();
            try
            {
                ChatClient client = new(model: "gpt-4o", apiKey: Environment.GetEnvironmentVariable("OpenAPIKey"));
                IEnumerable<UserChatMessage> promprChatMessage = _mapper.Map<IEnumerable<UserChatMessage>>(prompts);

                AsyncCollectionResult<StreamingChatCompletionUpdate> completionUpdates = client.CompleteChatStreamingAsync(promprChatMessage);
                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        result.Add(completionUpdate.ContentUpdate[0].Text);
                    }
                }

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
