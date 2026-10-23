
namespace BMGInsaat.Application.DTOs.About;

public class UpdateAboutContentRequest
{
    public string AboutTr { get; set; } = string.Empty;
    public string? AboutEn { get; set; }

    public string MissionTr { get; set; } = string.Empty;
    public string? MissionEn { get; set; }

    public string VisionTr { get; set; } = string.Empty;
    public string? VisionEn { get; set; }
}
