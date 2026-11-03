namespace BMGInsaat.Application.DTOs.ProjectVideos;

public class UpdateProjectVideoRequest
{
    public string? TitleTr { get; set; }

    public string? DescriptionTr { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
