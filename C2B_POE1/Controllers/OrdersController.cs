using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace C2B_POE1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrdersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Order");
            return View(orders ?? new List<Order>());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var orderLines = await _httpClient.GetFromJsonAsync<List<OrderLine>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/OrderLine");
            var linesForOrder = orderLines?.Where(l => l.PartitionKey == $"Order_{rowKey}").ToList() ?? new List<OrderLine>();

            var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Order");
            var order = orders?.FirstOrDefault(o => o.RowKey == rowKey);
            if (order == null) return NotFound();

            ViewBag.OrderLines = linesForOrder;
            return View(order);
        }

        // POST: Orders/MarkFulfilled
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<IActionResult> MarkFulfilled(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return BadRequest();

            var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Order");
            var order = orders?.FirstOrDefault(o => o.RowKey == rowKey);
            if (order == null) return NotFound();

            order.Fufilled = true;
            var response = await _httpClient.PostAsJsonAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Order", order);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to update order via function.";
            }

            return RedirectToAction("Index");
        }
    }
}
