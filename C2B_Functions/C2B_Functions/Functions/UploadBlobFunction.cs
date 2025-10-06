using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace C2B_Functions.Functions
{
    public class UploadBlobFunction
    {
        private readonly BlobServiceClient _blobServiceClient;

        public UploadBlobFunction(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        [Function("UploadBlob")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            if (!req.Headers.TryGetValues("Content-Type", out var contentTypes))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Missing Content-Type header");
                return response;
            }

            var contentType = contentTypes.First();
            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value;
            if (string.IsNullOrEmpty(boundary))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Missing boundary");
                return response;
            }

            var reader = new MultipartReader(boundary, req.Body);

            MultipartSection section;
            Stream fileStream = null;
            string fileName = null;

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader && contentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                {
                    fileName = contentDisposition.FileName.Value.Trim('"');
                    fileStream = section.Body;
                    break;
                }
            }

            if (fileStream == null || string.IsNullOrEmpty(fileName))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("No file data found");
                return response;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient("prod-img");
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(fileName)}");

            await blobClient.UploadAsync(fileStream, overwrite: true);

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync(blobClient.Uri.ToString());

            return response;
        }
    }
}
