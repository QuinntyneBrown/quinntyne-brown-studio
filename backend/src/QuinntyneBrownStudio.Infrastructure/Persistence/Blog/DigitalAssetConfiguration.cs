using QuinntyneBrownStudio.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using QuinntyneBrownStudio.Domain.Entities.Blog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuinntyneBrownStudio.Infrastructure.Persistence.Blog;

public class DigitalAssetConfiguration : IEntityTypeConfiguration<DigitalAsset>
{
    public void Configure(EntityTypeBuilder<DigitalAsset> builder)
    {
        builder.HasKey(d => d.DigitalAssetId);
        builder.Property(d => d.DigitalAssetId).ValueGeneratedOnAdd();
        builder.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(256);
        builder.Property(d => d.StoredFileName).IsRequired().HasMaxLength(256);
        builder.Property(d => d.ContentType).IsRequired().HasMaxLength(128);
        builder.Property(d => d.FileSizeBytes).IsRequired();
        builder.Property(d => d.CreatedBy).IsRequired();

        builder.HasOne<Microsoft.AspNetCore.Identity.IdentityUser<Guid>>()
            .WithMany()
            .HasForeignKey(d => d.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.StoredFileName).IsUnique().HasDatabaseName("IX_DigitalAssets_StoredFileName");
        builder.HasIndex(d => d.CreatedBy).HasDatabaseName("IX_DigitalAssets_CreatedBy");
        builder.HasIndex(d => d.ContentType).HasDatabaseName("IX_DigitalAssets_ContentType");
    }
}
