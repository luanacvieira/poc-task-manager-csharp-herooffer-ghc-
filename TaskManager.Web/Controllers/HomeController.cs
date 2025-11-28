using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Models;

namespace TaskManager.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        _logger.LogInformation("HomeController initialized");
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Index page accessed");
        return View();
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
