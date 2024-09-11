using BloggingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BloggingAPI.Persistence.Configurations
{
    internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c=> c.Content)
                   .IsRequired();

            builder.HasOne(c => c.Post)
                   .WithMany(c => c.Comment)
                   .HasForeignKey(c => c.PostId)
                   .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(c => c.Votes)
                    .WithOne(cv => cv.Comment)
                    .HasForeignKey(cv => cv.CommentId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.PublishedOn)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.DateModified)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
