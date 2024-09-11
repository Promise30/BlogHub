using BloggingAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloggingAPI.Persistence;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Post> BlogPosts { get; set; }
    public DbSet<Comment> PostComments { get; set; }
    public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<IdentityRole>().HasData(
         new IdentityRole
         {
             Name = "User",
             ConcurrencyStamp = "1",
             NormalizedName = "USER"
         },
         new IdentityRole
         {
             Name = "Administrator",
             ConcurrencyStamp = "2",
             NormalizedName = "ADMINISTRATOR"
         });
    }
}

