namespace OpenAI_WebAPI.Models
{
    public class GetMissingAssetsFromImagesRequest
    {
        public RequestSourceType RequestSourceType { get; set; }
        public string FolderPath { get; set; }

        public List<string> IdentifiedAssets { get; set; }
    }

    public enum RequestSourceType
    {
        Folder = 1,
        AzureBlobStorage = 2,
        PublicUrl = 3
    }
}
