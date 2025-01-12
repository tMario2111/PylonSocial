using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;


namespace ProiectDAW_V2.Controllers;

public class PostsController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;

    public PostsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    public ActionResult New(int? groupId = null)
    {
        var post = new Post();
        ViewBag.GroupId = groupId;
        if (TempData["GroupId"] != null)
            TempData.Remove("GroupId");
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public ActionResult New(Post post, int? groupId = null)
    {
        TempData["SelectedPostType"] = post.Type;
        if (groupId != null)
            TempData["GroupId"] = groupId;
        return RedirectToAction("SubmitPost");
    }

    public ActionResult SubmitPost()
    {
        if (TempData["SelectedPostType"] == null)
            return RedirectToAction("New");

        var postType = (Post.PostType)TempData["SelectedPostType"];
        TempData.Keep("SelectedPostType");

        if (TempData["GroupId"] != null)
            TempData.Keep("GroupId");

        var post = new Post { Type = postType };
        ViewBag.SelectedPostType = postType;

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> SubmitPost(Post post, IFormFile? content)
    {
        if (TempData["GroupId"] != null)
        {
            if (!db.UserGroups.Any(g => g.UserId == _userManager.GetUserId(User)
                                        && g.GroupId == (int)TempData["GroupId"]!))
            {
                return Unauthorized();
            }
        }

        post.UserId = _userManager.GetUserId(User);
        post.Date = DateTime.Now;
        ViewBag.SelectedPostType = post.Type;
        Console.WriteLine(post.UserId);

        if (content != null && content.Length > 0)
        {
            var allowedExtensions = new[] { "" };
            if (post.Type == Post.PostType.Image)
            {
                allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            }
            else
            {
                allowedExtensions = new[] { ".mp4" };
            }

            var fileExtension = Path.GetExtension(content.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file extension");
                return View(post);
            }

            string uid = new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyz0123456789", 32)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());

            var storagePath = Path.Combine(_env.WebRootPath, "posts",
                uid + Path.GetExtension(content.FileName));
            var databaseFileName = "/posts/" + uid + Path.GetExtension(content.FileName);

            using (var fileStream = new FileStream(storagePath, FileMode.Create))
            {
                await content.CopyToAsync(fileStream);
            }

            ModelState.Remove(nameof(post.Content));
            post.Content = databaseFileName;
        }

        if (ModelState.IsValid)
        {
            if (TempData["GroupId"] != null)
            {
                post.GroupId = (int)TempData["GroupId"]!;
                TempData.Remove("GroupId");
            }

            db.Posts.Add(post);
            db.SaveChanges();
            return RedirectToAction("Show", "Posts", new { id = post.Id });
        }

        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }

        return View(post);
    }

    public ActionResult Show(int id)
    {
        if (db.Posts.Find(id) == null)
            return NotFound();

        var post = db.Posts
            .Include(p => p.User)
            .Include(p => p.Comments.OrderByDescending(c => c.Date))
            .ThenInclude(c => c.Author)
            .ThenInclude(a => a.Profile)
            .Include(p => p.Group)
            .FirstOrDefault(p => p.Id == id);
        var userId = post.UserId;
        var profile = db.Profiles.Include(p => p.User)
            .Include(p => p.User.Followers)
            .Include(p => p.User.Following)
            .FirstOrDefault(x => x.UserId == userId);
        
        if (post.GroupId != null)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();
            if (!db.UserGroups.Any(g => g.UserId == _userManager.GetUserId(User) && g.GroupId == post.GroupId))
                return Unauthorized();
            ViewBag.IsModerator = post.Group.ModeratorId == _userManager.GetUserId(User);
        }
        else if (profile.Visibility == Profile.VisibilityType.Private)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();
            if (userId != _userManager.GetUserId(User) && !db.Followers.Any(f =>
                    f.FollowerId == _userManager.GetUserId(User)
                    && f.FollowedId == userId))
                return Unauthorized();
        }

        ViewBag.IsLoggedIn = User.Identity.IsAuthenticated;
        if (ViewBag.IsLoggedIn)
            ViewBag.LoggedInUserId = _userManager.GetUserId(User)!;
        ViewBag.IsAdmin = User.IsInRole("Admin");

        ViewBag.Profile = profile!;
        ViewBag.Comments = post.Comments;
        
        return View(post);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var post = db.Posts
            .Include(p => p.Comments)
            .Include(p => p.Group)
            .FirstOrDefault(p => p.Id == id);
        if (post == null)
            return NotFound();

        if (post.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin")
            && !(post.GroupId != null && post.Group.ModeratorId == _userManager.GetUserId(User)))
            return Unauthorized();

        foreach (var comment in post.Comments)
            db.Comments.Remove(comment);
        db.SaveChanges();

        db.Posts.Remove(post);
        db.SaveChanges();

        return RedirectToAction("Index", "Home");
    }
}