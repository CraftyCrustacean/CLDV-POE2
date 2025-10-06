using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace C2B_POE1.Controllers
{
    public class CartController : Controller
    {
        private const string SessionKey = "Cart";
        private readonly HttpClient _httpClient;

        public CartController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // POST: Add product to cart
        [HttpPost]
        public IActionResult AddToCart(string rowKey, int quantity = 1)
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            var cart = string.IsNullOrEmpty(cartJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;

            if (cart.ContainsKey(rowKey))
                cart[rowKey] += quantity;
            else
                cart[rowKey] = quantity;

            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
            return Json(new { success = true, quantity = cart[rowKey] });
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            var cartDict = string.IsNullOrEmpty(cartJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;

            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/table/Product")
                           ?? new List<Product>();

            var cartItems = cartDict.Select(kvp =>
            {
                var product = products.FirstOrDefault(p => p.RowKey == kvp.Key);
                return new CartItem
                {
                    Product = product ?? new Product { ProductName = "Unknown", ProductPrice = 0 },
                    Quantity = kvp.Value
                };
            }).ToList();

            return View(cartItems);
        }

        // POST: Clear cart
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(SessionKey);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return RedirectToAction("Index");

            var cart = JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;
            if (cart.Count == 0)
                return RedirectToAction("Index");

            var orderRowKey = Guid.NewGuid().ToString();

            var queueOrderMessage = new QueueOrderMessage
            {
                Order = new QueueOrder
                {
                    PartitionKey = "Orders",
                    RowKey = orderRowKey,
                    Fufilled = false
                },
                OrderLines = cart.Select(kvp => new QueueOrderLine
                {
                    PartitionKey = "Orders",
                    RowKey = Guid.NewGuid().ToString(),
                    ProductRowKey = kvp.Key,
                    Quantity = kvp.Value
                }).ToList()
            };

            var json = JsonSerializer.Serialize(queueOrderMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://st10435382func-e0drdcavcae5chen.uksouth-01.azurewebsites.net/api/EnqueueOrder", content);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to queue order. Please try again.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.Remove(SessionKey);
            TempData["Success"] = $"Order {orderRowKey} queued successfully!";
            return RedirectToAction("Index");
        }

    }
}
