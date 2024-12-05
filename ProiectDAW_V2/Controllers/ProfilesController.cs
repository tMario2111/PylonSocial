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
    private readonly IWebHostEnvironment _env;

    public ProfilesController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
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
    public async Task<IActionResult> New(Profile profile, IFormFile? profilePicture)
    {
        // TODO: Aparent profile picture e required
        // Probabil mai trebuie o migratie si se rezolva

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

        if (profilePicture != null && profilePicture.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file extension");
                return View(profile);
            }

            // Folosim un identificator unic pentru poze
            string uid = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 32)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var storagePath = Path.Combine(_env.WebRootPath, "profile_pictures",
                uid + Path.GetExtension(profilePicture.FileName));
            var databaseFileName = "/profile_pictures/" + uid + Path.GetExtension(profilePicture.FileName);

            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            ModelState.Remove(nameof(profile.ProfilePicture));
            profile.ProfilePicture = databaseFileName;
        }

        if (ModelState.IsValid)
        {
            db.Profiles.Add(profile);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        // TODO: Sterge asta
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }

        return View(profile);
    }

    public IActionResult Show(string? id = null)
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = (id == null) ? db.Profiles.FirstOrDefault(x => x.UserId == userId) :
                db.Profiles.FirstOrDefault(x => x.UserId == id);
        
        ViewBag.Profile = profile!;
        ViewBag.CanEdit = id == null || id == userId;
        
        return View(profile);
    }
    
    public IActionResult Edit()
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = db.Profiles.FirstOrDefault(x => x.UserId == userId);

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Profile requestProfile, IFormFile? profilePicture)
    {
        // TODO: Nu prea inteleg cum dau valoarea checkbox-ului in edit

        Profile profile = db.Profiles.FirstOrDefault(x => x.UserId == _userManager.GetUserId(User)!);

        if (string.IsNullOrEmpty(requestProfile.FirstName) || requestProfile.FirstName.Length < 3 ||
            requestProfile.FirstName.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid first name");

        if (string.IsNullOrEmpty(requestProfile.LastName) || requestProfile.LastName.Length < 3 ||
            requestProfile.LastName.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid last name");

        if (string.IsNullOrEmpty(requestProfile.Description) || requestProfile.Description.Length < 3 ||
            requestProfile.Description.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid description");

        foreach (var key in Request.Form.Keys)
        {
            string value = Request.Form[key];
            Console.WriteLine($"Key: {key}, Value: {value}");
        }

        // TODO: Rezolva problema cu checkbox-ul vietii
        // if (deleteProfilePicture)
        // {
        //     if (!string.IsNullOrEmpty(profile.ProfilePicture))
        //     {
        //         var oldFilePath = Path.Combine(_env.WebRootPath, profile.ProfilePicture.TrimStart('/'));
        //         if (System.IO.File.Exists(oldFilePath))
        //         {
        //             System.IO.File.Delete(oldFilePath);
        //         }
        //     }
        //
        //     profile.ProfilePicture = null;
        // }
        if (profilePicture != null && profilePicture.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(profilePicture.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file extension");
                return View(profile);
            }

            if (!string.IsNullOrEmpty(profile.ProfilePicture))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, profile.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Folosim un identificator unic pentru poze
            string uid = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 32)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var storagePath = Path.Combine(_env.WebRootPath, "profile_pictures",
                uid + Path.GetExtension(profilePicture.FileName));
            var databaseFileName = "/profile_pictures/" + uid + Path.GetExtension(profilePicture.FileName);

            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            ModelState.Remove(nameof(profile.ProfilePicture));
            profile.ProfilePicture = databaseFileName;
        }

        if (ModelState.IsValid)
        {
            profile.FirstName = requestProfile.FirstName;
            profile.LastName = requestProfile.LastName;
            profile.Description = requestProfile.Description;
            db.SaveChanges();
            return RedirectToAction("Show", "Profiles");
        }

        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }

        return View(requestProfile);
    }

    public IActionResult Search()
    {
        // TODO: Fix empty search page crash
        var search = "";

        if (!string.IsNullOrEmpty(HttpContext.Request.Query["search"]))
        {
            search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

            var profiles = 
                db.Profiles.Where(at => (at.FirstName + " " + at.LastName).Contains(search)).ToList();
            ViewBag.Profiles = profiles;
        }

        ViewBag.Search = search;

        return View();
    }
}