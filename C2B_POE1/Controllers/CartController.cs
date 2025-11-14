using C2B_POE1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace C2B_POE1.Controllers
{
    [Authorize(Roles = "Customer")]
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
        public IActionResult AddToCart(string rowKey, int quantity = 1, string returnUrl = null)
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

            var totalItems = cart.Sum(x => x.Value);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, quantity = cart[rowKey], totalItems = totalItems });
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Details", "Products", new { rowKey = rowKey });
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            var cartDict = string.IsNullOrEmpty(cartJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;

            var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product")
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

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "guest@example.com";
            var userName = $"{User.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value} {User.FindFirst(System.Security.Claims.ClaimTypes.Surname)?.Value}";

            var orderRowKey = Guid.NewGuid().ToString();

            var queueOrderMessage = new QueueOrderMessage
            {
                Order = new QueueOrder
                {
                    PartitionKey = userEmail,
                    RowKey = orderRowKey,
                    Fufilled = false
                },
                OrderLines = cart.Select(kvp => new QueueOrderLine
                {
                    PartitionKey = orderRowKey,
                    RowKey = Guid.NewGuid().ToString(),
                    ProductRowKey = kvp.Key,
                    Quantity = kvp.Value
                }).ToList()
            };

            var json = JsonSerializer.Serialize(queueOrderMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/EnqueueOrder?code=5VxMtOqrXapt26DrAcHAAAF-F18Yn5DkdDq4OGDtDYBmAzFuYzEpSA==", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to queue order: {errorContent}";
                return RedirectToAction("Index");
            }

            HttpContext.Session.Remove(SessionKey);
            TempData["Success"] = $"Order {orderRowKey} placed successfully!";
            return RedirectToAction("MyOrders", "Orders");
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(string productRowKey, int quantity)
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return RedirectToAction("Index");

            var cart = JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;

            if (quantity <= 0)
            {
                cart.Remove(productRowKey);
            }
            else
            {
                cart[productRowKey] = quantity;
            }

            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
            return RedirectToAction("Index");
        }

        // POST: Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(string productRowKey)
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return RedirectToAction("Index");

            var cart = JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;
            cart.Remove(productRowKey);

            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
            TempData["Success"] = "Item removed from cart";
            return RedirectToAction("Index");
        }

        // GET: Cart/GetCartCount
        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cartJson = HttpContext.Session.GetString(SessionKey);
            var cart = string.IsNullOrEmpty(cartJson)
                ? new Dictionary<string, int>()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(cartJson)!;

            var totalItems = cart.Sum(x => x.Value);
            return Json(new { count = totalItems });
        }

    }
}
