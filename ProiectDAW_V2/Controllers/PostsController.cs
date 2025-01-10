using System.Drawing.Printing;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;


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
    
    public ActionResult New()
    {
        var post = new Post();
        return View(post);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult New(Post post)
    { 
        TempData["SelectedPostType"] = post.Type;
        return RedirectToAction("SubmitPost");
    }
    
    
    public ActionResult SubmitPost()
    {
        if (TempData["SelectedPostType"] == null)
            return RedirectToAction("New");

        var postType = (Post.PostType)TempData["SelectedPostType"];
        TempData.Keep("SelectedPostType");

        var post = new Post { Type = postType };
        ViewBag.SelectedPostType = postType;

        return View(post);
    }
    
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitPost(Post post, IFormFile? content)
    {
        post.UserId = _userManager.GetUserId(User);
        post.Date = DateTime.Now;
        ViewBag.SelectedPostType = post.Type;
        
        if (content != null && content.Length > 0)
        {
            var allowedExtensions = new [] {"dad"};
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
        
        //if(ModelState.IsValid)
        {
            db.Posts.Add(post);
            db.SaveChanges();

            return RedirectToAction("Index"); 
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
        var post = db.Posts.Include(p => p.User).FirstOrDefault(p => p.Id == id);
        var userId = _userManager.GetUserId(User)!;
        var profile = db.Profiles.Include(p => p.User)
                .Include(p => p.User.Followers)
                .Include(p => p.User.Following)
                .FirstOrDefault(x => x.UserId == userId);

        ViewBag.Profile = profile!;
        return View(post);
    }

}