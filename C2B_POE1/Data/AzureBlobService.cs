using System.Net.Http.Headers;

namespace C2B_POE1.Data
{
    public class AzureBlobService
    {
        private readonly HttpClient _httpClient;

        public AzureBlobService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return string.Empty;

            using var form = new MultipartFormDataContent();
            form.Add(new StreamContent(file.OpenReadStream()), "file", file.FileName);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/UploadBlob")
            {
                Content = form
            };
            request.Headers.Add("x-filename", file.FileName);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
