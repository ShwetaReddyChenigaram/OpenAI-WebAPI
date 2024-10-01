using System.Collections.Generic;

namespace OpenAI_WebAPI.Config
{
    public class OpenAIResponse
    {
        public string Id { get; set; }

        public string Object { get; set; }

        public string Created { get; set; }

        public string Model { get; set; }

        public List<OpenAIChoice> Choices { get; set; }

        public OpenAIUsage Usage { get; set; }

        public string System_fingerprint { get; set; }
    }

    public class OpenAIChoice
    {
        public int index { get; set; }

        public OpenAIResponseMessage Message {get; set;}

        public string Finish_reason { get; set; }
    }

    public class OpenAIUsage
    {
        public string Prompt_tokens { get; set; }

        public string Completion_tokens { get; set; }

        public string Total_tokens { get; set; }
    }
}
