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

        modelBuilder.Entity<FollowRequest>().HasKey(f => new { f.SenderId, f.ReceiverId });

        modelBuilder.Entity<FollowRequest>().HasOne(fr => fr.Sender).WithMany(u => u.RequestsSent)
            .HasForeignKey(fr => fr.SenderId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<FollowRequest>().HasOne(fr => fr.Receiver).WithMany(u => u.RequestsReceived)
            .HasForeignKey(fr => fr.ReceiverId);
        
        modelBuilder.Entity<UserGroup>().HasKey(ug => new { ug.UserId, ug.GroupId });
        modelBuilder.Entity<UserGroup>().HasOne(ug => ug.User).WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);
        modelBuilder.Entity<UserGroup>().HasOne(ug => ug.Group).WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);

        modelBuilder.Entity<GroupJoinRequest>().HasKey(x => new { x.UserId, x.GroupId });
        modelBuilder.Entity<GroupJoinRequest>().HasOne(x => x.User)
            .WithMany(x => x.GroupJoinRequests).HasForeignKey(x => x.UserId);
        modelBuilder.Entity<GroupJoinRequest>().HasOne(x => x.Group)
            .WithMany(x => x.GroupJoinRequests).HasForeignKey(x => x.GroupId);
        
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Comment>().HasOne(c => c.Post).WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.NoAction);
    }
}