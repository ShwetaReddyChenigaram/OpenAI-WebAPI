using OpenAI_WebAPI.Config;

namespace OpenAI_WebAPI.Models
{
    public class GetMissingAssetsFromImagesResponse
    { 
        public List<string> MissingAssets { get; set; }

        public OpenAIResponse? OpenAIResponse { get; set; }

        public OpenAIRequest? OpenAIRequest { get; set; }

        public Exception? Exception { get; set; }
    }
}
