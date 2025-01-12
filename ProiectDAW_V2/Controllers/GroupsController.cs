using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public IActionResult Index()
    {
        ViewBag.Groups = _db.Groups.ToList();

        return View();
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

            var userGroup = new UserGroup();
            userGroup.GroupId = group.Id;
            userGroup.UserId = group.ModeratorId;
            _db.UserGroups.Add(userGroup);
            _db.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        return View(group);
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult Show(int id)
    {
        var group = _db.Groups.Find(id);
        if (group == null)
            return NotFound();

        ViewBag.Group = group;

        ViewBag.MemberCount = _db.UserGroups.Count(ug => ug.GroupId == group.Id).ToString();

        var userId = _userManager.GetUserId(User)!;

        ViewBag.IsMember = _db.UserGroups.Any(ug => ug.GroupId == group.Id && ug.UserId == userId);
        ViewBag.PendingRequest = _db.GroupJoinRequests.Any
            (gr => gr.GroupId == group.Id && gr.UserId == userId);

        ViewBag.IsModerator = _db.Groups.Any(g => g.ModeratorId == userId && g.Id == group.Id);
        if (ViewBag.IsModerator)
        {
            ViewBag.JoinRequestCount = _db.GroupJoinRequests.Count(gr => gr.GroupId == group.Id)
                .ToString();
        }

        ViewBag.IsAdmin = User.IsInRole("Admin");

        ViewBag.Posts = _db.Posts
            .Include(p => p.Comments)
            .Include(p => p.User)
            .ThenInclude(u => u.Profile)
            .Where(p => p.GroupId == group.Id)
            .OrderByDescending(p => p.Date)
            .ToList();

        return View();
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult Leave(int groupId)
    {
        var group = _db.Groups.Find(groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var userGroup = _db.UserGroups.Find(userId, groupId);
        if (userGroup == null)
            return BadRequest();

        _db.UserGroups.Remove(userGroup);
        _db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok();
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult ManageMembers(int groupId)
    {
        var group = _db.Groups.Find(groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (group.ModeratorId != userId)
            return Unauthorized();

        ViewBag.Group = group;

        ViewBag.Members = _db.UserGroups
            .Include(gr => gr.User)
            .Include(gr => gr.User.Profile)
            .Where(gr => gr.GroupId == group.Id &&
                         gr.UserId != userId);

        return View();
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Kick(int groupId, string memberId)
    {
        var group = _db.Groups.Find(groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (group.ModeratorId != userId)
            return Unauthorized();

        var userGroup = _db.UserGroups.Find(memberId, groupId);
        if (userGroup == null)
            return NotFound();

        _db.UserGroups.Remove(userGroup);
        _db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok();
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public IActionResult Delete(int groupId)
    {
        var group = _db.Groups.Find(groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (group.ModeratorId != userId && !User.IsInRole("Admin"))
            return Unauthorized();

        _db.GroupJoinRequests.RemoveRange(_db.GroupJoinRequests.Where(gr => gr.GroupId == groupId));
        _db.SaveChanges();

        _db.UserGroups.RemoveRange(_db.UserGroups.Where(gr => gr.GroupId == groupId));
        _db.SaveChanges();

        var posts = _db.Posts.Where(p => p.GroupId != null && p.GroupId == group.Id).ToList();
        foreach (var post in posts)
        {
            var comments = _db.Comments.Where(c => c.PostId == post.Id);
            _db.Comments.RemoveRange(comments);
            _db.SaveChanges();
        }
        
        _db.Posts.RemoveRange(posts);
        _db.SaveChanges();

        _db.Groups.Remove(group);
        _db.SaveChanges();

        return RedirectToAction("Index", "Groups");
    }
}