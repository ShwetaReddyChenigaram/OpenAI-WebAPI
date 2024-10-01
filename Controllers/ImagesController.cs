using Microsoft.AspNetCore.Mvc;
using OpenAI_WebAPI.Models;
using OpenAI_WebAPI.Operations;
using OpenAI_WebAPI.Services;

namespace OpenAI_WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImagesOperations _imagesOperations;
        private readonly IBlobStorageService _blobStorageService;

        public ImagesController(IImagesOperations imagesOperations, IBlobStorageService blobStorageService)
        {
            _imagesOperations = imagesOperations;
            _blobStorageService = blobStorageService;
        }

        [HttpPost]
        [Route("missingassets")]
        public async Task<IActionResult> GetMissingAssetsFromImages([FromBody] GetMissingAssetsFromImagesRequest request)
        {
            var response = await _imagesOperations.FindMissingAssets(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("upload/blobstorage")]
        public async Task<IActionResult> UploadImageToBlobStorage([FromBody] UploadImageToBlobStorageRequest request)
        {
            await _blobStorageService.UploadBlobAsync(request.ContainerName, request.BlobName, request.FilePath);
            return Ok();
        }

        [HttpGet]
        [Route("blobStorage")]
        public async Task<IActionResult> GetBlobStorage()
        {
            var response = await _blobStorageService.GetBlobStorage();
            return Ok(response);
        }
    }
}
