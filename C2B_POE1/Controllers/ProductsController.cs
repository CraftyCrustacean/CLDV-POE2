using C2B_POE1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;

namespace C2B_POE1.Controllers
{
	public class ProductsController : Controller
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ProductsController> _logger;

		public ProductsController(HttpClient httpClient, ILogger<ProductsController> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		// GET: Products
		public async Task<IActionResult> Index()
		{
			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");
			var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
			ViewBag.CategoryLookup = categories?.ToDictionary(c => c.RowKey, c => c.CatName) ?? new Dictionary<string, string>();
			return View(products ?? new List<Product>());
		}

		public async Task<IActionResult> Search(string query)
		{
			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");

			if (!string.IsNullOrWhiteSpace(query))
			{
				products = products?
					.Where(p => p.ProductName != null && p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
			ViewBag.CategoryLookup = categories?.ToDictionary(c => c.RowKey, c => c.CatName) ?? new Dictionary<string, string>();

			ViewBag.SearchQuery = query;
			return View("Index", products ?? new List<Product>());
		}


		// GET: Products/Detail
		public async Task<IActionResult> Details(string rowKey)
		{
			if (string.IsNullOrEmpty(rowKey)) return NotFound();

			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");
			var product = products?.FirstOrDefault(p => p.RowKey == rowKey);
			if (product == null) return NotFound();

			return View(product);
		}

		// GET: Products/Create
		public async Task<IActionResult> Create()
		{
			var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
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
				var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
				ViewData["CategoryId"] = new SelectList(categories, "RowKey", "CatName", product.CategoryId);
				return View(product);
			}

			product.RowKey = Guid.NewGuid().ToString();
			product.PartitionKey = product.CategoryId;

			if (imageFile != null && imageFile.Length > 0)
			{
				var form = new MultipartFormDataContent();
				form.Add(new StreamContent(imageFile.OpenReadStream()), "file", imageFile.FileName);

				var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/UploadBlob?code=ckGv79JzNbHAhTZ__5LeHl1s3sfBRrimv9aFjXPmVYdJAzFuQ2tMaw==");
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

			await _httpClient.PostAsJsonAsync("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product", product);
			return RedirectToAction(nameof(Index));
		}

		// GET: Products/Edit/5
		public async Task<IActionResult> Edit(string rowKey)
		{
			if (string.IsNullOrEmpty(rowKey)) return NotFound();

			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");
			var product = products?.FirstOrDefault(p => p.RowKey == rowKey);
			if (product == null) return NotFound();

			var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
			ViewData["CategoryId"] = new SelectList(categories, "RowKey", "CatName", product.CategoryId);

			return View(product);
		}

		// POST: Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string rowKey, [Bind("RowKey,ProductName,ProductDescription,ProductPrice,CategoryId,ProductImageURL,PartitionKey")] Product product, IFormFile? imageFile)
		{
			if (rowKey != product.RowKey) return NotFound();
			if (!ModelState.IsValid) return View(product);

			// Get the old product to check if category changed
			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");
			var oldProduct = products?.FirstOrDefault(p => p.RowKey == rowKey && p.PartitionKey == product.PartitionKey);

			if (oldProduct == null)
			{
				TempData["Error"] = "Product not found.";
				return RedirectToAction(nameof(Index));
			}

			// Check if category changed
			bool categoryChanged = oldProduct.PartitionKey != product.CategoryId;

			// Handle image upload
			if (imageFile != null && imageFile.Length > 0)
			{
				var form = new MultipartFormDataContent();
				form.Add(new StreamContent(imageFile.OpenReadStream()), "file", imageFile.FileName);

				var request = new HttpRequestMessage(HttpMethod.Post, "https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/UploadBlob?code=ckGv79JzNbHAhTZ__5LeHl1s3sfBRrimv9aFjXPmVYdJAzFuQ2tMaw==");
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
			else
			{
				product.ProductImageURL = oldProduct.ProductImageURL;
			}

			if (categoryChanged)
			{
				var deleteUrl = $"https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product/{oldProduct.PartitionKey}/{rowKey}";
				_logger.LogInformation($"Category changed. Deleting old entity: {deleteUrl}");
				await _httpClient.DeleteAsync(deleteUrl);

				product.PartitionKey = product.CategoryId;
				await _httpClient.PostAsJsonAsync("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product", product);
			}
			else
			{
				product.PartitionKey = product.CategoryId;
				await _httpClient.PostAsJsonAsync("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product", product);
			}

			TempData["Success"] = "Product updated successfully.";
			return RedirectToAction(nameof(Index));
		}

		// GET: Products/Delete/5
		public async Task<IActionResult> Delete(string rowKey, string partitionKey)
		{
			if (string.IsNullOrEmpty(rowKey)) return NotFound();

			var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");

			var product = !string.IsNullOrEmpty(partitionKey)
				? products?.FirstOrDefault(p => p.RowKey == rowKey && p.PartitionKey == partitionKey)
				: products?.FirstOrDefault(p => p.RowKey == rowKey);

			if (product == null) return NotFound();

			var categories = await _httpClient.GetFromJsonAsync<List<Category>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Category");
			ViewBag.CategoryLookup = categories?.ToDictionary(c => c.RowKey, c => c.CatName) ?? new Dictionary<string, string>();

			return View(product);
		}

		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string rowKey, string partitionKey)
		{
			_logger.LogInformation($"DeleteConfirmed called with rowKey: {rowKey}, partitionKey: {partitionKey}");

			if (string.IsNullOrEmpty(rowKey))
			{
				TempData["Error"] = "Invalid product ID.";
				return RedirectToAction(nameof(Index));
			}

			try
			{
				if (string.IsNullOrEmpty(partitionKey))
				{
					_logger.LogWarning("PartitionKey not provided, looking it up...");
					var products = await _httpClient.GetFromJsonAsync<List<Product>>("https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product");
					var product = products?.FirstOrDefault(p => p.RowKey == rowKey);

					if (product == null)
					{
						TempData["Error"] = "Product not found.";
						return RedirectToAction(nameof(Index));
					}

					partitionKey = product.PartitionKey;
				}

				var deleteUrl = $"https://st10435382funcpoe-fqfyeceahsfedacs.southafricanorth-01.azurewebsites.net/api/table/Product/{partitionKey}/{rowKey}";
				_logger.LogInformation($"Sending DELETE to: {deleteUrl}");

				var request = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);
				var response = await _httpClient.SendAsync(request);

				var responseContent = await response.Content.ReadAsStringAsync();
				_logger.LogInformation($"DELETE response status: {response.StatusCode}, content: {responseContent}");

				if (!response.IsSuccessStatusCode)
				{
					TempData["Error"] = $"Failed to delete product: {response.StatusCode} - {responseContent}";
				}
				else
				{
					TempData["Success"] = "Product deleted successfully!";
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Exception during delete");
				TempData["Error"] = $"Error deleting product: {ex.Message}";
			}

			return RedirectToAction(nameof(Index));
		}


	}
}