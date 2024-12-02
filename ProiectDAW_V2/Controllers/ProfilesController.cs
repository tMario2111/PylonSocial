using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class ProfilesController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ProfilesController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult New()
    {
        var profile = new Profile();

        // Trebuie sa fie logat
        if (_userManager.GetUserId(User) == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user_id = _userManager.GetUserId(User)!;
        // Are deja profil
        if (db.Profiles.Any(x => x.UserId == user_id))
        {
            return RedirectToAction("Index", "Home");
        }

        return View(profile);
    }

    [HttpPost]
    public IActionResult New(Profile profile)
    {
        profile.ProfilePicture = "";
        profile.UserId = _userManager.GetUserId(User)!;
        
        if (string.IsNullOrEmpty(profile.FirstName) || profile.FirstName.Length < 3 ||
            profile.FirstName.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid first name");

        if (string.IsNullOrEmpty(profile.LastName) || profile.LastName.Length < 3 ||
            profile.LastName.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid last name");

        if (string.IsNullOrEmpty(profile.Description) || profile.Description.Length < 3 ||
            profile.Description.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid description");
        
        if (ModelState.IsValid)
        {
            db.Profiles.Add(profile);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }

        return View(profile);
    }

    public IActionResult Show()
    {
        var user_id = _userManager.GetUserId(User)!;
        var profile = db.Profiles.FirstOrDefault(x => x.UserId == user_id);
        
        return View(profile);
    }
}