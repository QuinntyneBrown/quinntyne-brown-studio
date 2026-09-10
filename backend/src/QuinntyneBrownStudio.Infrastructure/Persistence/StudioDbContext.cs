using QuinntyneBrownStudio.Domain.Entities.Blog;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuinntyneBrownStudio.Infrastructure.Persistence;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<DigitalAsset> DigitalAssets => Set<DigitalAsset>();

    public DbSet<StoredRecord> Records => Set<StoredRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfiguration(new Blog.ArticleConfiguration());
        b.ApplyConfiguration(new Blog.DigitalAssetConfiguration());
        b.Entity<StoredRecord>().HasKey(x => new { x.Kind, x.Id });
        b.Entity<StoredRecord>().Property(x => x.Kind).HasMaxLength(100);
        b.Entity<StoredRecord>().Property(x => x.Version).IsConcurrencyToken();
        b.Entity<StoredRecord>().Property(x => x.UniqueKey).HasMaxLength(300);
        b.Entity<StoredRecord>()
            .HasIndex(x => new { x.Kind, x.UniqueKey })
            .IsUnique()
            .HasFilter("[UniqueKey] IS NOT NULL");
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") != null &&
                    entry.Property("CreatedAt").CurrentValue is DateTime created && created == default)
                    entry.Property("CreatedAt").CurrentValue = now;
                if (entry.Metadata.FindProperty("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Metadata.FindProperty("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = now;
                if (entry.Entity is Article article)
                    article.Version++;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
