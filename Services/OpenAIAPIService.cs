using OpenAI.GPT3;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using wingman.Helpers;
using wingman.Interfaces;

namespace wingman.Services
{
    public class OpenAIAPIService : IOpenAIAPIService
    {
        private readonly IOpenAIService _openAIService;
        private readonly ILocalSettings settingsService;
        private string _apikey;

        public OpenAIAPIService(ILocalSettings settingsService)
        {
            this.settingsService = settingsService;
            _apikey = settingsService.Load<string>("ApiKey");

            if (String.IsNullOrEmpty(_apikey))
            {
                _apikey = "Api Key Is Null or Empty";
                //throw new Exception($"Settings \"ApiKey\" is null.");
            }

            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey =  _apikey
            });
        }

        private async Task<bool> IsApiKeyValid()
        {
            if (String.IsNullOrEmpty(_apikey) || !_apikey.StartsWith("sk-") || _apikey.Length != 51)
                return false;
            return true;
        }

        public async Task<string> GetResponse(string prompt)
        {
            if (!await IsApiKeyValid())
                return "Invalid API Key.  Please check your settings.";

            // Future support for CodeDavinci ... it is in beta, and seems to be disabled right now
            /*
            var completionResult = await _openAIService.Completions.CreateCompletion(new CompletionCreateRequest
            {
                Prompt = prompt,
                Model = Models.CodeDavinciV2,
                MaxTokens = 8001
            });
            */
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(settingsService.Load<string>("System_Preprompt")),
                    //ChatMessage.FromSystem("You are a programming assistant.  You are only allowed to respond with the raw code.  Do not generate explanations.  Do not preface.  Do not follow-up after the code."),
                    ChatMessage.FromUser(prompt),
                },
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 2048,
                //Model = Models.CodeDavinciV2,
                //MaxTokens = 8001
            });


            if (completionResult.Successful)
            {
                // for davinci model :
                // var maid = completionResult.Choices.First().Text;

                var maid = completionResult.Choices.First().Message.Content;

                maid = PromptCleaners.CleanBlockIdentifiers(maid);
                if (settingsService.Load<bool>("Trim_Whitespaces"))
                    maid = PromptCleaners.TrimWhitespaces(maid);
                if (settingsService.Load<bool>("Trim_Newlines"))
                    maid = PromptCleaners.TrimNewlines(maid);

                return maid;
            }
            else
            {
                throw new Exception($"Failed to generate response: {completionResult.Error?.Message}");
            }
        }

        public async Task<byte[]> ReadFileBytes(StorageFile file)
        {
            BasicProperties fileProperties;

            try
            {
                fileProperties = await file.GetBasicPropertiesAsync();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to get basic properties: {e.Message}");
            }
            var fileSize = fileProperties.Size;

            byte[] fileBytes = new byte[fileSize];

            using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
            {
                await fileStream.ReadAsync(fileBytes.AsBuffer(), (uint)fileSize, InputStreamOptions.None);
            }

            return fileBytes;
        }

        public async Task<string> GetWhisperResponse(StorageFile inmp3)
        {

            var fileBytes = await ReadFileBytes(inmp3);

            var completionResult = await _openAIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.Text,
                File = fileBytes,
                FileName = inmp3.Name,
            });

            if (completionResult.Successful)
            {
                return completionResult.Text;
            }
            else
            {
                throw new Exception($"Failed to generate response: {completionResult.Error?.Message}");
            }
        }

    }
}