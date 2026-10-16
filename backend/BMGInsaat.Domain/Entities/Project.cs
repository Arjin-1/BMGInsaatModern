
using BMGInsaat.Domain.Enums;

namespace BMGInsaat.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    public string NameTr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;

    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();
}
