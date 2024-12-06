using Microsoft.AspNetCore.Identity;

namespace ProiectDAW_V2.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Follower>? Following { get; set; }
    public virtual ICollection<Follower>? Followers { get; set; }
}