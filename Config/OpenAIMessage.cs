using System.Collections.Generic;

namespace OpenAI_WebAPI.Config
{
    public class OpenAIMessage
    {
        public string role { get; set; }

        public List<OpenAIContent> content { get; set; }
    }
}
