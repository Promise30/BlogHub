using BloggingAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BloggingAPI.Persistence.Configurations
{
    internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.HasKey(t=> t.TagId);
            builder.Property(t=> t.Name)
                   .IsRequired();
        }
    }
}
