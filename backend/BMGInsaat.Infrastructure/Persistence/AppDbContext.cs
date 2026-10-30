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
    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<ConstructionService> ConstructionServices
        => Set<ConstructionService>();


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


            // Projenin isvereni / ilgili kurum
            entity.Property(p => p.EmployerTr)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(p => p.EmployerEn)
                .HasMaxLength(250);

            // Projenin baslangic ve bitis tarihleri
            entity.Property(p => p.StartDate)
                .HasColumnType("date");

            entity.Property(p => p.EndDate)
                .HasColumnType("date");

            // Ana sayfada one cikan proje
            entity.Property(p => p.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);


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

        // Hakkımızda içerik yapılandırması
        modelBuilder.Entity<AboutContent>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.ToTable(table =>
                table.HasCheckConstraint(
                "CK_AboutContents_SingleRecord",
                "\"Id\" = 1"));

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


        // Sertifika yapılandırması
        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.NameTr)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.NameEn)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(c => c.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(c => c.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(c => c.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(c => c.DisplayOrder);
        });


        // Hizmetler yapılandırması
        modelBuilder.Entity<ConstructionService>(entity =>
        {
            entity.HasKey(s => s.Id);

            // Türkçe ve İngilizce başlıklar
            entity.Property(s => s.TitleTr)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.TitleEn)
                .IsRequired()
                .HasMaxLength(200);

            // Hizmet açıklamaları
            entity.Property(s => s.DescriptionTr)
                .IsRequired()
                .HasColumnType("text");

            entity.Property(s => s.DescriptionEn)
                .IsRequired()
                .HasColumnType("text");

            // Hizmet görseli
            entity.Property(s => s.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            // Yayın durumu
            entity.Property(s => s.IsPublished)
                .IsRequired()
                .HasDefaultValue(false);

            // Görüntülenme sırası
            entity.Property(s => s.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Kayıt tarihleri
            entity.Property(s => s.CreatedAtUtc)
                .IsRequired();

            entity.Property(s => s.UpdatedAtUtc);

            // Yayınlanmış hizmetlerin sıralı listelenmesi
            entity.HasIndex(s => new
            {
                s.IsPublished,
                s.DisplayOrder
            });
        });



    }
}
