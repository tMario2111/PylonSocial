using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class FollowersController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public FollowersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        db = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost]
    public IActionResult New(string sender, string receiver)
    {
        if (db.Followers.FirstOrDefault(f => f.FollowerId == sender && f.FollowedId == receiver) != null)
        {
            return BadRequest("User is already followed");
        }

        var receiverProfile = db.Profiles.First(p => p.UserId == receiver);

        if (receiverProfile.Visibility == Profile.VisibilityType.Private)
        {
            var followRequest = new FollowRequest();
            followRequest.SenderId = sender;
            followRequest.ReceiverId = receiver;
            followRequest.Date = DateTime.Now;
            db.FollowRequests.Add(followRequest);
            db.SaveChanges();
        }
        else
        {
            var follower = new Follower();
            follower.FollowerId = sender;
            follower.FollowedId = receiver;
            db.Followers.Add(follower);
            db.SaveChanges();
        }

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok("Followed successfully");
    }

    [HttpPost]
    public IActionResult Delete(string sender, string receiver)
    {
        if (db.Followers.FirstOrDefault(f => f.FollowerId == sender && f.FollowedId == receiver) == null)
        {
            return BadRequest("User is not followed");
        }

        db.Followers.Remove(db.Followers.First(f => f.FollowerId == sender && f.FollowedId == receiver));
        db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok("Unfollowed successfully");
    }

    // TODO: Nu stiu daca actiunea asta ar trebui sa fie in FollowRequestsController
    // Dupa mine nu prea are relevanta
    [HttpPost]
    public IActionResult DeleteFollowRequest(string sender, string receiver)
    {
        var request = db.FollowRequests.Find(sender, receiver);
        if (request == null)
        {
            return BadRequest("Follow request not found");
        }

        db.FollowRequests.Remove(request);
        db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok("Follow request deleted successfully");
    }

    public enum ShowType
    {
        Followers,
        Following,
    }

    // Actiune de show care poate afisa utilizatorii urmaritori, dar si pe cei urmariti
    public IActionResult Show(string userId, ShowType showType)
    {
        ViewBag.ShowType = showType;
        
        var userProfile = db.Profiles.First(p => p.UserId == userId);
        ViewBag.Name = userProfile.FirstName + ' ' + userProfile.LastName;
        
        if (showType == ShowType.Followers)
        {
            ViewBag.Profiles = db.Followers
                .Include(f => f.FollowerUser)
                .Include(f => f.FollowerUser.Profile)
                .Where(f => f.FollowedId == userId)
                .Select(f => f.FollowerUser.Profile)
                .ToList();
        }
        else // ShowType.Following
        {
            ViewBag.Profiles = db.Followers
                .Include(f => f.FollowedUser)
                .Include(f => f.FollowedUser.Profile)
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowedUser.Profile)
                .ToList();
        }

        return View();
    }
}