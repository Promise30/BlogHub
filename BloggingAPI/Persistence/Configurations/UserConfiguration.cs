using BloggingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace BloggingAPI.Persistence.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName)
                   .IsRequired();

            builder.Property(u => u.LastName)
                   .IsRequired();

            builder.Property(u => u.Email)
                   .IsRequired();

            builder.Property(u=> u.UserName)
                   .IsRequired();

            builder.Property(u => u.DateCreated)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.DateModified)
                   .HasDefaultValueSql("GETDATE()");
            builder.HasMany(u => u.Posts)
                    .WithOne()
                    .HasForeignKey(p => p.ApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
