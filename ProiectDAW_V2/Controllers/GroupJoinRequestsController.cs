﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Data;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Controllers;

public class GroupJoinRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public GroupJoinRequestsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult New(int groupId)
    {
        var group = _db.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;

        if (_db.GroupJoinRequests.Any(g => g.UserId == userId && g.GroupId == groupId))
            return BadRequest("Request already sent");

        var groupJoinRequest = new GroupJoinRequest();
        groupJoinRequest.GroupId = group.Id;
        groupJoinRequest.UserId = userId;
        _db.GroupJoinRequests.Add(groupJoinRequest);
        _db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok();
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult Delete(int groupId)
    {
        var group = _db.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;

        if (!_db.GroupJoinRequests.Any(g => g.UserId == userId && g.GroupId == groupId))
            return BadRequest("Request does not exist");

        _db.GroupJoinRequests.Remove(_db.GroupJoinRequests.Find(userId, groupId));
        _db.SaveChanges();

        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return Ok();
    }

    [Authorize(Roles = "User,Admin")]
    public IActionResult Show(int groupId)
    {
        var group = _db.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return NotFound();

        var userId = _userManager.GetUserId(User)!;
        if (group.ModeratorId != userId)
            return Unauthorized();

        ViewBag.Group = group;

        ViewBag.GroupJoinRequests = _db.GroupJoinRequests
            .Include(g => g.User)
            .Include(g => g.User.Profile)
            .Where(g => g.GroupId == groupId);

        return View();
    }

    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult AcceptRequest(string userId, int groupId)
    {
        var group = _db.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return NotFound();
        
        var userGroup = new UserGroup();
        userGroup.GroupId = groupId;
        userGroup.UserId = userId;
        _db.UserGroups.Add(userGroup);
        
        var joinRequst = _db.GroupJoinRequests.Find(userId, groupId);
        _db.GroupJoinRequests.Remove(joinRequst);
        _db.SaveChanges();
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        
        return Ok();
    }
    
    [Authorize(Roles = "User,Admin")]
    [HttpPost]
    public IActionResult DenyRequest(string userId, int groupId)
    {
        var group = _db.Groups.FirstOrDefault(g => g.Id == groupId);
        if (group == null)
            return NotFound();
        
        var joinRequst = _db.GroupJoinRequests.Find(userId, groupId);
        _db.GroupJoinRequests.Remove(joinRequst);
        _db.SaveChanges();
        
        var referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }
        
        return Ok();
    }
}