using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProiectDAW_V2.Models;

public class Follower
{
    [Key, Column(Order = 0)] public string FollowerId { get; set; }
    public virtual ApplicationUser? FollowerUser { get; set; }
    
    [Key, Column(Order = 1)] public string FollowedId { get; set; }
    public virtual ApplicationUser? FollowedUser { get; set; }
}