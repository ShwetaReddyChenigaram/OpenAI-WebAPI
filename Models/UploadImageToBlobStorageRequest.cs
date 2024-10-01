namespace OpenAI_WebAPI.Models
{
    public class UploadImageToBlobStorageRequest
    {
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public string FilePath { get; set; }
    }
}
