using BMGInsaat.Domain.Entities;
using BMGInsaat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace BMGInsaat.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.NameTr)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.NameEn)
                .HasMaxLength(200);

            entity.Property(p => p.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.DescriptionTr)
                .HasColumnType("text");

            entity.Property(p => p.DescriptionEn)
                .HasColumnType("text");

            entity.HasMany(p => p.Images)
                .WithOne(i => i.Project)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectImage>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasIndex(i => new { i.ProjectId, i.DisplayOrder });
        });
    }
}
