using BloggingAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BloggingAPI.Data;
public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Post> BlogPosts { get; set; }
    public DbSet<Comment> PostComments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Comment>()
                   .HasOne(c => c.Post)
                   .WithMany(c => c.Comment)
                   .HasForeignKey(c => c.PostId)
                   .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Post>()
                    .HasMany(p => p.Comment)
                    .WithOne(p => p.Post)
                    .HasForeignKey(p => p.PostId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>()
            .Property(p => p.Title)
            .IsRequired();

        modelBuilder.Entity<Post>()
            .Property(p => p.PublishedOn).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Post>()
            .Property(p => p.DateModified).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Comment>()
            .Property(c => c.PublishedOn).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Comment>()
            .Property(c => c.DateModified).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<Post>()
            .Property(p => p.ImagePublicId)
            .IsRequired(false);
        modelBuilder.Entity<Post>()
            .Property(p=>p.ImageFormat)
            .IsRequired(false);


        modelBuilder.Entity<User>()
            .Property(u => u.DateCreated).HasDefaultValueSql("GETDATE()");
        modelBuilder.Entity<User>()
            .Property(u => u.DateModified).HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<CommentVote>()
            .HasOne(cv => cv.Comment)
            .WithMany(c => c.Votes)
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

        modelBuilder.Entity<CommentVote>()
            .HasOne(cv => cv.User)
            .WithMany()
            .HasForeignKey(cv => cv.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IdentityRole>().HasData(
         new IdentityRole
         {
             Name = "User",
             ConcurrencyStamp ="1",
             NormalizedName = "USER"
         },
         new IdentityRole
         {
             Name = "Administrator",
             ConcurrencyStamp ="2",
             NormalizedName = "ADMINISTRATOR"
         }
            );
    }
}

