using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class GroupsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GroupsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult New()
    {
        var group = new Group();

        return View(group);
    }

    [HttpPost]
    public IActionResult New(Group group)
    {
        if (string.IsNullOrEmpty(group.Name) || group.Name.Length < 2 || group.Name.Length > 50)
            ModelState.AddModelError(string.Empty, "Name must be between 2 and 50 characters");

        if (string.IsNullOrEmpty(group.Description) || group.Description.Length < 3 || group.Description.Length > 100)
            ModelState.AddModelError(string.Empty, "Description must be between 3 and 100 characters");

        group.ModeratorId = _userManager.GetUserId(User)!;
        if (ModelState.IsValid)
        {
            _db.Groups.Add(group);
            _db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }

        return View(group);
    }
}