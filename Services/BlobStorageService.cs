namespace OpenAI_WebAPI.Services
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Sas;
    using OpenAI_WebAPI.Utilities;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public interface IBlobStorageService
    {
        Task<string> UploadBlobAsync(string containerName, string blobName, string filePath);
        Task DownloadBlobAsync(string containerName, string blobName, string downloadFilePath);

        Task<List<string>> GetBlobStorage();

    }
    public class BlobStorageService: IBlobStorageService
    {
        
        public async Task<string> UploadBlobAsync(string containerName, string blobName, string filePath)
        {
            // Create a BlobServiceClient object to manage storage account
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.AzureConnectionString);

            // Get the container client to interact with blob container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Upload the file
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Open file stream and upload the blob
            using FileStream uploadFileStream = File.OpenRead(filePath);
            var response = await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            Console.WriteLine($"Uploaded {blobName} to {containerName}");

            // Generate SAS URL for the uploaded blob
            return GenerateSasUrl(blobClient);
        }

        private string GenerateSasUrl(BlobClient blobClient)
        {
            // Create a SAS token that's valid for 1 hour
            if (blobClient.CanGenerateSasUri)
            {
                var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read, expiresOn: DateTimeOffset.UtcNow.AddHours(1));
                return sasUri == null? string.Empty: sasUri.AbsoluteUri;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task DownloadBlobAsync(string containerName, string blobName, string downloadFilePath)
        {
            // Create a BlobServiceClient object
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.AzureConnectionString);

            // Get the container client
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Download the blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                await download.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }

            Console.WriteLine($"Downloaded {blobName} to {downloadFilePath}");
        }

        public async Task<List<string>> GetBlobStorage()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Constants.AzureConnectionString);
            var imageList = new List<string>();
            // Get a reference to the container
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(Constants.Container);

            // List all blobs in the container
            Console.WriteLine($"Listing blobs in container '{Constants.Container}':");

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync(prefix: Constants.BlobName))
            {
                BlobClient blobClient = containerClient.GetBlobClient(blobItem.Name);
                var sasUrl = GenerateSasUrl(blobClient);
                imageList.Add(sasUrl);
            }

            return imageList;
        }
    }
}
