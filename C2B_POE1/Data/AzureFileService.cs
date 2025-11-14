using System.Net.Http.Headers;

namespace C2B_POE1.Data
{
    public class AzureFileService
    {
        private readonly HttpClient _httpClient;

        public AzureFileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string directory = "dummycontract")
        {
            if (file == null || file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/UploadFileShare?code=Iqqo-Qknlg7OJL9RXdgiUG3p4b8PTjSQSzAjw0ObADEwAzFux63uIA==")
            {
                Content = new StreamContent(stream)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            request.Headers.Add("x-filename", file.FileName);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }
}
