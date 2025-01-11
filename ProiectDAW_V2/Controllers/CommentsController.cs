using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class CommentsController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;
    
    public CommentsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    [Authorize(Roles = "User,Admin")]
    public ActionResult New(int postId)
    {
        var comment = new Comment();
        comment.PostId = postId;
        return View(comment);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public ActionResult New(Comment comment)
    {
        comment.AuthorId = _userManager.GetUserId(User);
        comment.Date = DateTime.Now;
        Console.WriteLine(comment.AuthorId);
        if (ModelState.IsValid)
        {
            db.Add(comment);
            db.SaveChanges();
            
            return RedirectToAction("Show", "Posts", new { id = comment.PostId });
        }
        
        foreach (var state in ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
            }
        }
        return View(comment);
    }
}