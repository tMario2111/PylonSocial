using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class FollowRequestsController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public FollowRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Show()
    {
        // AI RAMAS AICI!
        ViewBag.FollowRequests =
            db.FollowRequests.Include(fr => fr.Sender).Include(fr => fr.Receiver)
                .Include(fr => fr.Sender.Profile)
                .Where(fr => fr.ReceiverId == _userManager.GetUserId(User)).ToList();


        return View();
    }

    [HttpPost]
    public IActionResult AcceptRequest(string senderId)
    {
        var userId = _userManager.GetUserId(User);
        var requestToDelete = db.FollowRequests.Find(senderId, userId);
        if (requestToDelete == null)
        {
            return NotFound();
        }
        db.FollowRequests.Remove(requestToDelete);

        var follow = new Follower();
        follow.FollowerId = senderId;
        follow.FollowedId = userId;
        db.Followers.Add(follow);
        db.SaveChanges();
        
        return RedirectToAction("Show", "FollowRequests");
    }

    [HttpPost]
    public IActionResult DenyRequest(string senderId)
    {
        var userId = _userManager.GetUserId(User);
        var requestToDelete = db.FollowRequests.Find(senderId, userId);
        
        if (requestToDelete == null)
        {
            return NotFound();
        }
        db.FollowRequests.Remove(requestToDelete);
        db.SaveChanges();

        return RedirectToAction("Show", "FollowRequests");
 
    }
}