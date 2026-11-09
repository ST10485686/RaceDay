using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    private readonly IRaceDayApiClient _api;
    public OrganiserController(IRaceDayApiClient api) => _api = api;

    private string Token => HttpContext.Session.GetString("JWToken") ?? "";

    public IActionResult Dashboard()
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        return View();
    }
}