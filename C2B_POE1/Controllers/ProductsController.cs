using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace C2B_POE1.Controllers
{
    public class ProductsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductsController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product");
            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            ViewBag.CategoryLookup = categories?.ToDictionary(c => c.RowKey, c => c.CatName) ?? new Dictionary<string, string>();
            return View(products ?? new List<Product>());
        }

        public async Task<IActionResult> Search(string query)
        {
            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product");

            if (!string.IsNullOrWhiteSpace(query))
            {
                products = products?
                    .Where(p => p.ProductName != null && p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            ViewBag.CategoryLookup = categories?.ToDictionary(c => c.RowKey, c => c.CatName) ?? new Dictionary<string, string>();

            ViewBag.SearchQuery = query;
            return View("Index", products ?? new List<Product>());
        }


        // GET: Products/Details/{rowKey}
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product");
            var product = products?.FirstOrDefault(p => p.RowKey == rowKey);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            ViewData["CategoryId"] = new SelectList(categories, "RowKey", "CatName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,ProductDescription,ProductPrice,CategoryId")] Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
                ViewData["CategoryId"] = new SelectList(categories, "RowKey", "CatName", product.CategoryId);
                return View(product);
            }

            product.RowKey = Guid.NewGuid().ToString();
            product.PartitionKey = product.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(imageFile.OpenReadStream()), "file", imageFile.FileName);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/UploadBlob");
                request.Content = form;
                request.Headers.Add("x-filename", imageFile.FileName);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    product.ProductImageURL = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TempData["Error"] = "Image upload failed. Proceeding without image.";
                }
            }

            await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product", product);
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product");
            var product = products?.FirstOrDefault(p => p.RowKey == rowKey);
            if (product == null) return NotFound();

            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            ViewData["CategoryId"] = new SelectList(categories, "RowKey", "CatName", product.CategoryId);

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, [Bind("RowKey,ProductName,ProductDescription,ProductPrice,CategoryId,ProductImageURL")] Product product, IFormFile? imageFile)
        {
            if (rowKey != product.RowKey) return NotFound();
            if (!ModelState.IsValid) return View(product);

            product.PartitionKey = product.CategoryId;

            if (imageFile != null && imageFile.Length > 0)
            {
                var form = new MultipartFormDataContent();
                form.Add(new StreamContent(imageFile.OpenReadStream()), "file", imageFile.FileName);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/UploadBlob");
                request.Content = form;
                request.Headers.Add("x-filename", imageFile.FileName);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    product.ProductImageURL = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    TempData["Error"] = "Image upload failed. Keeping existing image.";
                }
            }

            await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product", product);
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product");
            var product = products?.FirstOrDefault(p => p.RowKey == rowKey);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product/{rowKey}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Failed to delete product via function.";

            return RedirectToAction(nameof(Index));
        }


    }
}
