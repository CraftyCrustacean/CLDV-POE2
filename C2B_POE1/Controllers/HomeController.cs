using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using C2B_POE1.Models;
using C2B_POE1.Data;

namespace C2B_POE1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AzureTableService<Product> _productService;

    public HomeController(ILogger<HomeController> logger, AzureTableService<Product> productService)
    {
        _logger = logger;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
