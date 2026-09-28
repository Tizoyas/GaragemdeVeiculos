using System.Diagnostics;
using GaragemVeiculos.Models;
using Microsoft.AspNetCore.Mvc;

namespace GaragemVeiculos.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Reserva");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
