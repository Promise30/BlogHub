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
            builder.HasKey(cv => cv.Id);
            builder.HasOne(cv => cv.User)
                   .WithMany()
                   .HasForeignKey(cv => cv.ApplicationUserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
