using BloggingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BloggingAPI.Persistence.Configurations
{
    internal sealed class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
    {
        public void Configure(EntityTypeBuilder<CommentVote> builder)
        {
            builder.HasOne(cv => cv.Comment)
                   .WithMany(c => c.Votes)
                   .HasForeignKey(cv => cv.CommentId)
                   .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(cv => cv.User)
                   .WithMany()
                   .HasForeignKey(cv => cv.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
