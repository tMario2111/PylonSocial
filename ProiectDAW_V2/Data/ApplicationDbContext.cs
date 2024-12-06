using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProiectDAW_V2.Models;

namespace ProiectDAW_V2.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Follower> Followers { get; set; }
    public DbSet<FollowRequest> FollowRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Follower>().HasKey(f => new { f.FollowerId, f.FollowedId });
        
        modelBuilder.Entity<Follower>().HasOne(f => f.FollowerUser).WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId).OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Follower>().HasOne(f => f.FollowedUser).WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowedId).OnDelete(DeleteBehavior.Restrict);
    }
}