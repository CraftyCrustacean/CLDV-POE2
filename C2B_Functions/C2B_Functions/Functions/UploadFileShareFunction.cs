using System.Net;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace C2B_Functions.Functions
{
    public class UploadFileShareFunction
    {
        private readonly ShareClient _shareClient;

        public UploadFileShareFunction()
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;
            _shareClient = new ShareClient(connectionString, "dummycontract");
            _shareClient.CreateIfNotExists();
        }

        [Function("UploadFileShare")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            var response = req.CreateResponse();

            if (!req.Headers.TryGetValues("x-filename", out var filenames))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Missing 'x-filename' header");
                return response;
            }

            var fileName = filenames.First();
            var rootDir = _shareClient.GetRootDirectoryClient();
            var fileClient = rootDir.GetFileClient($"{Guid.NewGuid()}{Path.GetExtension(fileName)}");

            if (!req.Headers.TryGetValues("Content-Length", out var lengthValues) || !long.TryParse(lengthValues.FirstOrDefault(), out var contentLength))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Missing or invalid 'Content-Length' header");
                return response;
            }

            await fileClient.CreateAsync(contentLength);
            await fileClient.UploadAsync(req.Body);

            response.StatusCode = HttpStatusCode.OK;
            await response.WriteStringAsync($"File uploaded to File Share successfully: {fileClient.Uri}");

            return response;
        }

    }
}
