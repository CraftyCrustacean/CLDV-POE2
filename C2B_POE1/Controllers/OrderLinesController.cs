using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace C2B_POE1.Controllers
{
    public class OrderLinesController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrderLinesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: OrderLines
        public async Task<IActionResult> Index()
        {
            var orderLines = await _httpClient.GetFromJsonAsync<List<OrderLine>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/OrderLine");
            return View(orderLines ?? new List<OrderLine>());
        }

        // GET: OrderLines/Details/5
        public async Task<IActionResult> Details(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey)) return NotFound();

            var orderLines = await _httpClient.GetFromJsonAsync<List<OrderLine>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/OrderLine");
            var line = orderLines?.FirstOrDefault(l => l.RowKey == rowKey);
            if (line == null) return NotFound();

            return View(line);
        }
    }
}
