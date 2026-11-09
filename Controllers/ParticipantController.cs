using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly IRaceDayApiClient _api;
    public ParticipantController(IRaceDayApiClient api) => _api = api;

    private string Token => HttpContext.Session.GetString("JWToken") ?? "";

    public IActionResult Index()
    {
        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        return View();
    }
}