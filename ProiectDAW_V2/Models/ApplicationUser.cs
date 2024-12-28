using Microsoft.AspNetCore.Identity;

namespace ProiectDAW_V2.Models;

public class ApplicationUser : IdentityUser
{
    public virtual Profile? Profile { get; set; }
    
    public virtual ICollection<Follower>? Following { get; set; }
    public virtual ICollection<Follower>? Followers { get; set; }
    
    public virtual ICollection<FollowRequest>? RequestsSent { get; set; }
    public virtual ICollection<FollowRequest>? RequestsReceived { get; set; }
    
    public virtual ICollection<UserGroup>? UserGroups { get; set; }
}