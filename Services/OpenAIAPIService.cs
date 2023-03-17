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
        private readonly IOpenAIService? _openAIService;
        private readonly ILocalSettings settingsService;
        private readonly ILoggingService Logger;
        private string _apikey;
        private bool _disposed;

        public OpenAIAPIService(ILocalSettings settingsService, ILoggingService logger)
        {
            this.settingsService = settingsService;
            this.Logger = logger;
            _apikey = settingsService.Load<string>("ApiKey");

            if (String.IsNullOrEmpty(_apikey))
            {
                _apikey = "Api Key Is Null or Empty";
                Logger.LogError("_apikey");
            }

            _openAIService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey =  _apikey
            });
        }

        public async Task<bool> IsApiKeyValid()
        {
            if (String.IsNullOrEmpty(_apikey) || !_apikey.StartsWith("sk-") || _apikey.Length != 51)
                return false;
            return true;
        }

        public async Task<string> GetResponse(string prompt)
        {
            if (!await IsApiKeyValid())
            {
                Logger.LogError("OpenAI API Key is Invalid.");
                return "Invalid API Key.  Please check your settings.";
            }

            Logger.LogDebug("Sending prompt to OpenAI API");
            var completionResult = await _openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(settingsService.Load<string>("System_Preprompt")),
                    ChatMessage.FromUser(prompt),
                },
                Model = Models.ChatGpt3_5Turbo,
                MaxTokens = 2048,
                //Model = Models.CodeDavinciV2,
                //MaxTokens = 8001
            });

            if (completionResult.Successful)
            {
                Logger.LogDebug("OpenAI API Response Received");


                var maid = completionResult.Choices.First().Message.Content;
                var lines = maid.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var dbgstr = "Raw Prompt :\r\n";
                foreach (var line in lines)
                    dbgstr += ">>> " + line;
                Logger.LogDebug(dbgstr);

                maid = PromptCleaners.CleanBlockIdentifiers(maid);
                if (settingsService.Load<bool>("Trim_Whitespaces"))
                    maid = PromptCleaners.TrimWhitespaces(maid);
                if (settingsService.Load<bool>("Trim_Newlines"))
                    maid = PromptCleaners.TrimNewlines(maid);
                return maid;
            }
            else
            {
                var result = completionResult.Error?.Message;

                if (result != null && result.Contains("Incorrect API key provided"))
                {
                    Logger.LogError("You aren't using a valid OpenAI API Key.");
                }
                else
                {
                    Logger.LogError("OpenAI API Failed, Reason :");
                    if (result == null)
                        Logger.LogError("Result was null.");
                    else
                        Logger.LogError(result);
                }

                return String.Empty;
            }
        }

        public async Task<byte[]> ReadFileBytes(StorageFile file)
        {
            if (file == null)
            {
                Logger.LogException("File is null");
                throw new ArgumentNullException(nameof(file));
            }

            BasicProperties fileProperties;

            try
            {
                fileProperties = await file.GetBasicPropertiesAsync();
            }
            catch (TaskCanceledException e)
            {
                Logger.LogException($"Task cancelled exception: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to get basic properties: {e.Message}");
                throw;
            }

            var fileSize = fileProperties.Size;
            byte[] fileBytes = new byte[fileSize];

            Logger.LogDebug("Reading audio file to byte array");
            try
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await fileStream.ReadAsync(fileBytes.AsBuffer(), (uint)fileSize, InputStreamOptions.None);
                }
            }
            catch (Exception e)
            {
                Logger.LogException($"Failed to read file bytes: {e.Message}");
                throw;
            }

            return fileBytes;
        }


        public async Task<string> GetWhisperResponse(StorageFile inmp3)
        {

            var fileBytes = await ReadFileBytes(inmp3);

            Logger.LogDebug("Sending audio to Whisper API");
            var completionResult = await _openAIService.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.Text,
                File = fileBytes,
                FileName = inmp3.Name,
            });

            if (completionResult.Successful && !completionResult.Text.Contains("\"type\": \"invalid_request_error\","))
            {
                //Logger.LogInfo($"Whisper API Response: " + completionResult.Text);  // sent elsewhere for now
                return completionResult.Text;
            }
            else
            {
                var result = completionResult.Error?.Message;

                if (result != null && result.Contains("Incorrect API key provided"))
                {
                    Logger.LogError("You aren't using a valid OpenAI API Key.");
                }
                else
                {
                    Logger.LogError("Whisper API Failed, Reason :");
                    if (result == null)
                        Logger.LogError("Result was null.");
                    else
                        Logger.LogError(result);
                }

                return String.Empty;
            }
        }

    }
}