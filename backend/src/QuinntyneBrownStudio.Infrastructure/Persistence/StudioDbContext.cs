using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuinntyneBrownStudio.Infrastructure.Persistence;

public sealed class StudioDbContext(DbContextOptions<StudioDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<StoredRecord> Records => Set<StoredRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<StoredRecord>().HasKey(x => new { x.Kind, x.Id });
        b.Entity<StoredRecord>().Property(x => x.Kind).HasMaxLength(100);
        b.Entity<StoredRecord>().Property(x => x.Version).IsConcurrencyToken();
        b.Entity<StoredRecord>().Property(x => x.UniqueKey).HasMaxLength(300);
        b.Entity<StoredRecord>()
            .HasIndex(x => new { x.Kind, x.UniqueKey })
            .IsUnique()
            .HasFilter("[UniqueKey] IS NOT NULL");
    }
}
