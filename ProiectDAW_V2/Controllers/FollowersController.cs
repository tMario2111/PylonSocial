using Microsoft.AspNetCore.Mvc;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class FollowersController : Controller
{
    private readonly ApplicationDbContext db;

    public FollowersController(ApplicationDbContext context)
    {
        db = context;
    }

    [HttpPost]
    public IActionResult New(string sender, string receiver)
    {
        if (db.Followers.FirstOrDefault(f => f.FollowerId == sender && f.FollowedId == receiver) != null)
        {
            return BadRequest("User is already followed");
        }

        Console.WriteLine("Got here!");

        var follower = new Follower();
        follower.FollowerId = sender;
        follower.FollowedId = receiver;
        db.Followers.Add(follower);
        db.SaveChanges();

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
}