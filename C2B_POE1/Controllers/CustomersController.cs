using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace C2B_POE1.Controllers
{
    public class CustomersController : Controller
    {
        private readonly HttpClient _httpClient;

        public CustomersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetFromJsonAsync<List<Customer>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer");
            return View(response ?? new List<Customer>());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer");
            var customer = customers?.FirstOrDefault(c => c.RowKey == rowKey);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create() => View();

        // POST: Customers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerFirstName,CustomerLastName,CustomerEmail,CustomerCell,CustomerDoB")] Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            customer.RowKey = Guid.NewGuid().ToString();
            customer.PartitionKey = "Customer";

            var response = await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer", customer);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to create customer via function.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer");
            var customer = customers?.FirstOrDefault(c => c.RowKey == rowKey);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string rowKey, [Bind("RowKey,CustomerFirstName,CustomerLastName,CustomerEmail,CustomerCell,CustomerDoB")] Customer customer)
        {
            if (rowKey != customer.RowKey) return NotFound();
            if (!ModelState.IsValid) return View(customer);

            customer.PartitionKey = "Customer";

            var response = await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer", customer);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to update customer via function.");
                return View(customer);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var customers = await _httpClient.GetFromJsonAsync<List<Customer>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer");
            var customer = customers?.FirstOrDefault(c => c.RowKey == rowKey);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string rowKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Customer/{rowKey}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                TempData["Error"] = "Failed to delete customer via function.";

            return RedirectToAction(nameof(Index));
        }
    }
}
