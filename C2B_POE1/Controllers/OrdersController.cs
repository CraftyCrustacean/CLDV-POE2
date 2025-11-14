using C2B_POE1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class OrdersController : Controller
{
	private readonly HttpClient _httpClient;

	public OrdersController(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	// GET: Orders
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> Index()
	{
		var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Orders");
		return View(orders ?? new List<Order>());
	}

	// GET: Orders/MyOrders
	[Authorize(Roles = "Customer")]
	public async Task<IActionResult> MyOrders()
	{
		var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
		var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Orders");
		var myOrders = orders?.Where(o => o.PartitionKey == userEmail)
							  .OrderByDescending(o => o.Timestamp)
							  .ToList() ?? new List<Order>();

		return View(myOrders);
	}

    // GET: Orders/Details/5
    public async Task<IActionResult> Details(string rowKey)
    {
        if (string.IsNullOrEmpty(rowKey)) return NotFound();

        var orderLines = await _httpClient.GetFromJsonAsync<List<OrderLine>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/OrderLines");
        var linesForOrder = orderLines?.Where(l => l.PartitionKey == rowKey).ToList() ?? new List<OrderLine>();

        var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Orders");
        var order = orders?.FirstOrDefault(o => o.RowKey == rowKey);
        if (order == null) return NotFound();

        if (User.IsInRole("Customer") && order.PartitionKey != User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value)
        {
            return Forbid();
        }

        var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");

        var productLookup = products?.ToDictionary(p => p.RowKey, p => p) ?? new Dictionary<string, Product>();

        ViewBag.OrderLines = linesForOrder;
        ViewBag.ProductLookup = productLookup;
        return View(order);
    }

    // POST: Orders/MarkFulfilled
    [HttpPost]
	[Authorize(Roles = "Admin")]
	public async Task<IActionResult> MarkFulfilled(string rowKey)
	{
		if (string.IsNullOrEmpty(rowKey)) return BadRequest();

		var orders = await _httpClient.GetFromJsonAsync<List<Order>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Orders");
		var order = orders?.FirstOrDefault(o => o.RowKey == rowKey);
		if (order == null) return NotFound();

		order.Fufilled = true;
		var response = await _httpClient.PostAsJsonAsync("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Orders", order);

		if (!response.IsSuccessStatusCode)
		{
			TempData["Error"] = "Failed to update order via function.";
		}
		else
		{
			TempData["Success"] = "Order marked as fulfilled";
		}

		return RedirectToAction("Index");
	}

}