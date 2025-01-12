using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
        if (!ViewBag.IsAuthenticated)
            return View();

        var userId = _userManager.GetUserId(User)!;

        var followedIds = _db.Followers
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowedId)
            .ToList();

        var joinedGroups = _db.UserGroups
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToList();

        foreach (var group in joinedGroups)
            Console.WriteLine(group);

        ViewBag.Posts = _db.Posts
            .Include(p => p.Comments)
            .Include(p => p.User)
            .ThenInclude(u => u.Profile)
            .Include(p => p.Group)
            .Where(p => (p.GroupId == null && followedIds.Any(id => id == p.UserId))
                        || (p.GroupId != null && joinedGroups.Any(id => id == p.GroupId)
                                              && p.UserId != userId))
            .OrderByDescending(p => p.Date)
            .ToList();

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