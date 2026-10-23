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

    public DbSet<AboutContent> AboutContents => Set<AboutContent>();

    public DbSet<CompanyValue> CompanyValues => Set<CompanyValue>();

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


        
        modelBuilder.Entity<ProjectImage>(entity =>
        {
            entity.HasKey(i => i.Id);

            entity.Property(i => i.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasIndex(i => new { i.ProjectId, i.DisplayOrder });
        });

        // Hakkımızda içerik yapılandırması
        modelBuilder.Entity<AboutContent>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.Property(a => a.AboutTr)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(a => a.AboutEn)
                .HasColumnType("text");

            entity.Property(a => a.MissionTr)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(a => a.MissionEn)
                .HasColumnType("text");

            entity.Property(a => a.VisionTr)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(a => a.VisionEn)
                .HasColumnType("text");

            entity.Property(a => a.CreatedAtUtc)
                .IsRequired();
        });

        
        // Şirket değerleri yapılandırması
        modelBuilder.Entity<CompanyValue>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.TitleTr)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(v => v.TitleEn)
                .HasMaxLength(150);

            entity.Property(v => v.DescriptionTr)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(v => v.DescriptionEn)
                .HasColumnType("text");

            entity.Property(v => v.DisplayOrder)
                .IsRequired();

            entity.Property(v => v.IsPublished)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(v => v.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(v => v.DisplayOrder);
        });

    }
}
