using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace C2B_POE1.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly HttpClient _httpClient;

        public CategoriesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            return View(categories ?? new List<Category>());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            var category = categories?.FirstOrDefault(c => c.RowKey == rowKey);
            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create() => View();

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CatName")] Category category)
        {
            if (!ModelState.IsValid) return View(category);

            category.RowKey = Guid.NewGuid().ToString();
            category.PartitionKey = "Category";

            var response = await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category", category);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create category via function.");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            var category = categories?.FirstOrDefault(c => c.RowKey == rowKey);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, [Bind("RowKey,CatName")] Category category)
        {
            if (rowKey != category.RowKey) return NotFound();
            if (!ModelState.IsValid) return View(category);

            category.PartitionKey = "Category";

            var response = await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category", category);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to update category via function.");
                return View(category);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category");
            var category = categories?.FirstOrDefault(c => c.RowKey == rowKey);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Category/{rowKey}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Failed to delete category via function.";

            return RedirectToAction(nameof(Index));
        }
    }
}
