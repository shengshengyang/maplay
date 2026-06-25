using Maplay.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maplay.Data;

/// <summary>
/// EF Core 僅作查詢/對應，schema 擁有權歸 Flyway。
/// 嚴禁呼叫 Migrate()/EnsureCreated() 或啟用 EF Core Migrations。
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<SpotImage> SpotImages => Set<SpotImage>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ImportHistory> ImportHistory => Set<ImportHistory>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.Role).HasMaxLength(20);
            e.Property(x => x.Provider).HasMaxLength(20);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Ignore(x => x.IsActive);
            e.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Spot>(e =>
        {
            e.ToTable("spots");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.AgeGroups).HasColumnType("text[]");
            e.Property(x => x.Facilities).HasColumnType("text[]");
            e.Property(x => x.Location).HasColumnType("geometry(Point,4326)");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.HasMany(x => x.Images).WithOne(i => i.Spot!).HasForeignKey(i => i.SpotId);
            e.HasMany(x => x.Reviews).WithOne(r => r.Spot!).HasForeignKey(r => r.SpotId);
        });

        b.Entity<SpotImage>(e =>
        {
            e.ToTable("spot_images");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });

        b.Entity<Review>(e =>
        {
            e.ToTable("reviews");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            e.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        b.Entity<ImportHistory>(e =>
        {
            e.ToTable("import_history");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
        });
    }
}
