namespace OpenAI_WebAPI.Models
{
    public class FileDetailsResponse
    {
        public IEnumerable<FileDetails> FileDetails { get; set; }
    }

    public class FileDetails
    {
        public string FileName { get; set; }

        public string Extension { get; set; }

        public DateTime ModifiedTime { get; set; }

        public double FileSizeInByte { get; set; }

        public string FullPath { get; set; }

        public string SubFolder { get; set; }
    }
}
