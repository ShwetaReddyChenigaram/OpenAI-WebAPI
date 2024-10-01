using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI_WebAPI.Config;
using OpenAI_WebAPI.Models;
using OpenAI_WebAPI.Services;
using OpenAI_WebAPI.Utilities;
using RestSharp;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace OpenAI_WebAPI.Operations
{
    public interface IImagesOperations
    {
        Task<GetMissingAssetsFromImagesResponse> FindMissingAssets(GetMissingAssetsFromImagesRequest request);
        string ReplaceBlackSlash(string identifiedAsset);
    }

    public class ImagesOperations : IImagesOperations
    {
        private OpenAIConfig _config;
        private IDirectoryWrapper _directory;
        private IBlobStorageService _blobStorageService;

        public ImagesOperations(IOptions<OpenAIConfig> config,
            IDirectoryWrapper directory,
            IBlobStorageService blobStorageService)
        {
            _config = config.Value;
            _directory = directory;
            _blobStorageService = blobStorageService;
        }

        public async Task<GetMissingAssetsFromImagesResponse> FindMissingAssets(GetMissingAssetsFromImagesRequest request)
        {
            var missingAssetsResponse = new GetMissingAssetsFromImagesResponse();
            var identifiedAssets = request.IdentifiedAssets == null || !request.IdentifiedAssets.Any() ? string.Empty : string.Join(", ", request.IdentifiedAssets);
            var allInventory = Constants.AllInventory == null || !Constants.AllInventory.Any() ? string.Empty : string.Join(", ", Constants.AllInventory.Select(s => s.AssetName));
            string question = $"Identify all the assets from the images which are also included in the inventory assets list ({allInventory}), exclude already identified assets ({identifiedAssets}), and format the response as (**Asset Name** - first image name where it was identified); ensure no duplicates, and display ##No Asset Found## only if no assets are found.";
            try
            {
                OpenAIRequest requestBody = null;
                if (request.RequestSourceType == RequestSourceType.Folder)
                {
                    var imagePaths = _directory.GetFiles(Constants.LocalImagePath).Where(f => f.ToLower().Contains(".jpg")).ToList();
                    requestBody = await GetRequestForOpenAI(imagePaths, question, true);
                }
                else if(request.RequestSourceType == RequestSourceType.AzureBlobStorage)
                {
                    var imagePaths = await _blobStorageService.GetBlobStorage();
                    requestBody = await GetRequestForOpenAI(imagePaths, question, false);
                }
                else if (request.RequestSourceType == RequestSourceType.PublicUrl)
                {
                    var imagePaths = _directory.GetFiles(Constants.LocalImagePath).Where(f => f.ToLower().Contains(".jpg")).ToList();
                    var formattedImages = new List<string>();
                    foreach(var image in imagePaths)
                    {
                        var fileName = GetFileName(image);
                        var formattedImage = $"{Constants.PublicUrlImageBasePath}/{fileName}";
                        formattedImages.Add(formattedImage);
                    }

                    requestBody = await GetRequestForOpenAI(formattedImages, question, false);
                }

                string response = await GetImageInsightsFromOpenAIAsync(requestBody);
                if (response != null)
                {
                    var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(response);
                    missingAssetsResponse.OpenAIRequest = requestBody;
                    missingAssetsResponse.OpenAIResponse = openAIResponse;
                    if (openAIResponse != null && openAIResponse.Choices != null && openAIResponse.Choices.Any() && (openAIResponse.Choices?.FirstOrDefault().Message) != null)
                    {
                        var identifiedAssetsFromImages = openAIResponse.Choices.FirstOrDefault().Message.content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                        if (identifiedAssetsFromImages != null && identifiedAssetsFromImages.Any())
                        {
                            var finalMissingAssetList = ProcessAssets(identifiedAssetsFromImages.ToList(), request.IdentifiedAssets, Constants.AllInventory);

                            if (finalMissingAssetList != null && finalMissingAssetList.Any())
                            {
                                ////var missingAssets = BuildMissingAssetsResponse(finalMissingAssetList);
                                missingAssetsResponse.MissingAssets = finalMissingAssetList;
                                return missingAssetsResponse;
                            }
                        }
                    }
                }

                return missingAssetsResponse;
            }
            catch (Exception ex)
            {
                return new GetMissingAssetsFromImagesResponse { Exception = ex };
            }
        }

        private async Task<OpenAIRequest> GetRequestForOpenAI(List<string> images, string question, bool convertByte)
        {
            var openAIContents = new List<OpenAIContent>()
            {
                new OpenAIContent()
                            {
                                type = "text",
                                text = question
                }
            };

            var i = 0;
            foreach (var image in images)
            {
                i++;
                OpenAIContent openAIContent = null;
                var fileName = GetImageName(image);

                if (convertByte)
                {
                    byte[] imageBytes = await File.ReadAllBytesAsync(image);
                    var base64String = Convert.ToBase64String(imageBytes);

                    openAIContent = new OpenAIContent()
                    {
                        type = "image_url",
                        image_url = new OpenAIImageUrl()
                        {
                            ////url = image,
                            url = $"data:image/jpeg;base64,{base64String}",
                            detail = "high"
                        }
                    };
                }
                else
                {
                    openAIContent = new OpenAIContent()
                    {
                        type = "image_url",
                        image_url = new OpenAIImageUrl()
                        {
                            url = image,
                            detail = "high"
                        }
                    };
                }

                openAIContents.Add(openAIContent);

                var openAIImageName = new OpenAIContent()
                {
                    type = "text",
                    text = $"Image {i} is named: {fileName}"
                };

                openAIContents.Add(openAIImageName);
            }

            // Create a JSON request body with images and the question
            var requestBody = new OpenAIRequest()
            {
                model = _config.OpenAIModel,
                messages = new List<OpenAIMessage>()
                {
                    new OpenAIMessage()
                    {
                        role = "user",
                        content = openAIContents
                    }
                },
                max_tokens = 1000
            };

            return requestBody;
        }

        private IEnumerable<string> ExtractMatchedAssets(string identifiedAsset)
        {
            var matches = Regex.Matches(identifiedAsset, @"\*\*(.*?)\*\*");
            return matches.Select(match => match.Groups[1].Value);
        }
        private bool IsAssetMatched(string matchedAsset, List<string> identifiedAssetsSitenotes)
        {
            return identifiedAssetsSitenotes != null && identifiedAssetsSitenotes.Any(w =>
                w.Contains(matchedAsset) || matchedAsset.Contains(w));
        }

        private string BuildMissingAssetsResponse(string finalMissingAssetList)
        {
            return $"Hey, we got these from site notes photos, but in the photos there appears to be:\n" +
                             $"{finalMissingAssetList} \n Please check if assets are missing.";
        }

        private bool IsDivision40(string matchedAsset, List<Inventory> assets)
        {
            return assets.Any(a => (a.AssetName.Contains(matchedAsset) || matchedAsset.Contains(a.AssetName)) && a.IsDivision40);
        }

        private List<string> ProcessAssets(List<string> identifiedAssetsFromImages, List<string> identifiedAssetsSitenotes, List<Inventory> assets)
        {
            List<string> finalMissingAssetList = new List<string>();
            int index = 0;

            foreach (var identifiedAsset in identifiedAssetsFromImages)
            {
                var matchedAssets = ExtractMatchedAssets(identifiedAsset);
                foreach (var matchedAsset in matchedAssets)
                {
                    if (IsAssetMatched(matchedAsset, identifiedAssetsSitenotes)) continue;

                    index++;
                    string formattedAsset = IsDivision40(matchedAsset, assets)
                        ? ReplaceBlackSlash($"<b>{index}.{identifiedAsset}</b>")
                        : ReplaceBlackSlash($"{index}.{identifiedAsset}");

                    finalMissingAssetList.Add(formattedAsset);
                }
            }

            return finalMissingAssetList;
        }

        private async Task<string> GetImageInsightsFromOpenAIAsync(OpenAIRequest requestBody)
        {
            var uri = $"{_config.OpenAIUrl}v1/chat/completions";

            var client = new RestClient(uri);

            var request = new RestRequest()
            {
                Method = RestSharp.Method.Post,
                Timeout = TimeSpan.FromMinutes(5)
            };
            request.AddHeader("Authorization", $"{_config.OpenAIToken}");
            request.AddHeader("Content-Type", "application/json");


            var jsonString = JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            request.AddBody(jsonString);
            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                return response.Content ?? string.Empty;
            }
            else
            {
                throw new Exception($"Error: {response.ErrorMessage}");
            }
        }

        private string GetFileName(string imagePath)
        {
            var nameComponent = imagePath.Split('\\');
            string lastWord = nameComponent[^1]; // Using the ^1 index to get the last element
            int queryIndex = lastWord.IndexOf('?');
            if (queryIndex != -1)
            {
                // Get the filename part before the '?'
                return lastWord.Substring(0, queryIndex);
            }

            // If there's no query parameter, return the last component directly
            return lastWord;
        }

        private string GetImageName(string imagePath)
        {
            var nameComponent = imagePath.Split('/');
            string lastWord = nameComponent[^1]; // Using the ^1 index to get the last element
            int queryIndex = lastWord.IndexOf('?');
            if (queryIndex != -1)
            {
                // Get the filename part before the '?'
                return lastWord.Substring(0, queryIndex);
            }

            // If there's no query parameter, return the last component directly
            return lastWord;
        }

        public string ReplaceBlackSlash(string identifiedAsset)
        {
            string identifiedAsset1 = "**Light shades** - D:\\\\Shweta\\\\images\\\\680003\\\\Filtred\\680003_Sheet1_9-12-2019_11_49_07_AM.jpg  "; // Example input with double backslashes

            // Replace double backslashes with a single backslash
            var replace =  $"{identifiedAsset.Replace(@"\\", @"\")}";
            var replace1 = $"{identifiedAsset1.Replace(@"\\", @"\")}";
            return replace;
        }
    }
}
