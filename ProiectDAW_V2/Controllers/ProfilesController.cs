﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // TODO: Aparent nu se afiseaza erorile de validare (?)
    [Authorize(Roles = "User,Admin")]
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
        profile.UserId = _userManager.GetUserId(User)!;

        if (string.IsNullOrEmpty(profile.FirstName) || profile.FirstName.Length < 2 ||
            profile.FirstName.Length > 100)
            ModelState.AddModelError(string.Empty, "Invalid first name");

        if (string.IsNullOrEmpty(profile.LastName) || profile.LastName.Length < 2 ||
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
        ApplicationUser? user = null;
        if (id != null)
        {
            user = _userManager.FindByIdAsync(id).Result;
            if (user == null)
                return NotFound();
        }
        else
        {
            if (!User.Identity.IsAuthenticated)
                return NotFound();
        }

        var loggedInUserId = _userManager.GetUserId(User)!;
        var profile = (id == null)
            ? db.Profiles.Include(p => p.User)
                .Include(p => p.User.Followers)
                .Include(p => p.User.Following)
                .FirstOrDefault(x => x.UserId == _userManager.GetUserId(User)!)
            : db.Profiles.Include(p => p.User)
                .Include(p => p.User.Followers)
                .Include(p => p.User.Following)
                .FirstOrDefault(x => x.UserId == id);

        var isLoggedInUser = id == null || id == loggedInUserId;

        ViewBag.Profile = profile!;
        ViewBag.CanEdit = isLoggedInUser;
        ViewBag.CanDelete = false;

        if (!isLoggedInUser)
        {
            var follower = db.Followers.FirstOrDefault(f => f.FollowerId == loggedInUserId && f.FollowedId == id);
            ViewBag.FollowRequest = db.FollowRequests.Find(loggedInUserId, id) != null;
            ViewBag.Followed = follower != null;
            if (User.IsInRole("Admin"))
            {
                ViewBag.CanDelete = true;
            }
        }

        ViewBag.UserId = loggedInUserId;

        if (ViewBag.CanEdit || ViewBag.Followed || ViewBag.Profile.Visibility == Profile.VisibilityType.Public)
        {
            var userProfileId = (id == null) ? _userManager.GetUserId(User)! : id;
            ViewBag.Posts = db.Posts
                .Include(p => p.Comments)
                .Where(p => p.UserId == userProfileId && p.GroupId == null)
                .OrderByDescending(p => p.Date);
        }

        return View(profile);
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult Edit()
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = db.Profiles.FirstOrDefault(x => x.UserId == userId);

        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Profile requestProfile, IFormFile? profilePicture)
    {
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

        Console.WriteLine(requestProfile.DeleteProfilePicture);

        if (requestProfile.DeleteProfilePicture)
        {
            if (!string.IsNullOrEmpty(profile.ProfilePicture))
            {
                var oldFilePath = Path.Combine(_env.WebRootPath, profile.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            profile.ProfilePicture = null;
        }
        else if (profilePicture != null && profilePicture.Length > 0)
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
            profile.Visibility = requestProfile.Visibility;
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

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public IActionResult Delete(string id)
    {
        var userToBeDeleted = _userManager.FindByIdAsync(id).Result;
        if (userToBeDeleted == null)
            return NotFound();

        // TODO: Logica pentru grupuri!
        db.Followers.RemoveRange(db.Followers.Where(f => f.FollowedId == id || f.FollowerId == id));
        db.FollowRequests.RemoveRange(db.FollowRequests.Where(fr => fr.SenderId == id || fr.ReceiverId == id));

        var posts = db.Posts.Where(p => p.UserId == userToBeDeleted.Id).ToList();
        foreach (var post in posts)
        {
            var comments = db.Comments.Where(c => c.PostId == post.Id);
            db.Comments.RemoveRange(comments);
            db.SaveChanges();
        }
        
        db.Posts.RemoveRange(posts);
        db.SaveChanges();

        var groups = db.Groups.Where(g => g.ModeratorId == id).ToList();
        foreach (var group in groups)
        {
            var groupPosts = db.Posts.Where(p => p.GroupId != null && p.GroupId == group.Id).ToList();
            foreach (var post in groupPosts)
            {
                var comments = db.Comments.Where(c => c.PostId == post.Id);
                db.Comments.RemoveRange(comments);
                db.SaveChanges();
            }
            
            db.Posts.RemoveRange(groupPosts);
            db.SaveChanges();
            
            db.GroupJoinRequests.RemoveRange(db.GroupJoinRequests.Where(gr => gr.GroupId == group.Id));
            db.SaveChanges();

            db.UserGroups.RemoveRange(db.UserGroups.Where(gr => gr.GroupId == group.Id));
            db.SaveChanges();
        }
        db.Groups.RemoveRange(groups);
        db.SaveChanges();
        
        db.Profiles.RemoveRange(db.Profiles.Where(p => p.UserId == id));
        _userManager.DeleteAsync(userToBeDeleted).GetAwaiter().GetResult();
        db.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Search()
    {
        var search = "";

        if (!string.IsNullOrEmpty(HttpContext.Request.Query["search"]))
        {
            search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

            var profiles =
                db.Profiles.Where(
                    at => (at.FirstName + " " + at.LastName).Contains(search)).ToList();
            ViewBag.Profiles = profiles;
        }
        else
        {
            // Return all profiles for empty search
            ViewBag.Profiles = db.Profiles.ToList();
        }

        ViewBag.Search = search;

        return View();
    }
}