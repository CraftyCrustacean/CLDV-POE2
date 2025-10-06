using C2B_POE1.Data;
using Microsoft.AspNetCore.Mvc;

public class ContractsController : Controller
{
    private readonly AzureFileService _fileService;

    public ContractsController(AzureFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile contractFile)
    {
        if (contractFile != null && contractFile.Length > 0)
        {
            var path = await _fileService.UploadFileAsync(contractFile);
            ViewBag.Message = $"File uploaded to {path}";
            return View();
        }

        ModelState.AddModelError("", "No file selected");
        return View();
    }
}
