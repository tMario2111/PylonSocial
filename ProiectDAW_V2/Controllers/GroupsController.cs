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
        
        

        return View();
    }
}