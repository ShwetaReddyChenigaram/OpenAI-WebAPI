using System.Collections.Generic;

namespace OpenAI_WebAPI.Config
{
    public class OpenAIRequest
    {
        public string model { get; set; }

        public List<OpenAIMessage> messages { get; set; }

        public int max_tokens { get; set; }
    }

    public class OpenAIContent
    {
        public string type { get; set; }

        public string text { get; set; }

        public OpenAIImageUrl image_url { get; set; }
    }

    public class OpenAIImageUrl
    {
        public string url { get; set; }

        public string detail { get; set; }
    }
}
