using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [Authorize(Roles = "User,Admin")]
    public IActionResult Edit(int id)
    {
        var comment = db.Comments.Find(id);
        if (comment == null)
            return NotFound();
        if (comment.AuthorId != _userManager.GetUserId(User))
            return Unauthorized();

        return View(comment);
    }

    [HttpPost]
    public IActionResult Edit(int id, Comment requestComment)
    {
        var comment = db.Comments.Find(id);

        if (ModelState.IsValid)
        {
            comment.Date = DateTime.Now;
            comment.Content = requestComment.Content;
            db.SaveChanges();

            return RedirectToAction("Show", "Posts", new { id = requestComment.PostId });
        }

        return View(requestComment);
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var comment = db.Comments
            .Include(c => c.Post)
            .ThenInclude(p => p.Group)
            .FirstOrDefault(c => c.Id == id);
        if (comment == null)
            return NotFound();

        if (comment.AuthorId != _userManager.GetUserId(User)
            && !User.IsInRole("Admin")
            && !(comment.Post.GroupId != null && comment.Post.Group.ModeratorId == _userManager.GetUserId(User)))
            return Unauthorized();

        db.Remove(comment);
        db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok();
    }
}