using System.ComponentModel.DataAnnotations;
using BMGInsaat.Domain.Enums;

namespace BMGInsaat.Application.DTOs.Projects;

public class CreateProjectRequest
{
    [Required]
    [MaxLength(200)]
    public string NameTr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string NameEn { get; set; } = string.Empty;

    public string DescriptionTr { get; set; } = string.Empty;

    public string DescriptionEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Ongoing;

    public bool IsPublished { get; set; } = false;
}
