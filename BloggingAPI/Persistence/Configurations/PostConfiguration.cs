using BloggingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BloggingAPI.Persistence.Configurations
{
    internal sealed class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasKey(p => p.PostId); 
                   
            builder.HasMany(p => p.Comment)
                    .WithOne(p => p.Post)
                    .HasForeignKey(p => p.PostId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

            builder.Property(p => p.Title)
                    .IsRequired();

            builder.Property(p=> p.Content)
                    .IsRequired();

            builder.Property(p => p.ImagePublicId)
                    .IsRequired(false);

            builder.Property(p => p.ImageFormat)
                    .IsRequired(false);

            builder.Property(p => p.PublishedOn)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.DateModified)
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}
