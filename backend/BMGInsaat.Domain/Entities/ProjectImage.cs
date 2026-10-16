namespace BMGInsaat.Domain.Entities;

public class ProjectImage
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsCover { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
}
