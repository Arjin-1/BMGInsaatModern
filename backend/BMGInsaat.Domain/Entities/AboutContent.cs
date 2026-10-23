namespace BMGInsaat.Domain.Entities;

public class AboutContent
{
    public int Id { get; set; }

    public string AboutTr { get; set; } = string.Empty;
     public string AboutEn { get; set; } = string.Empty;

    
    public string MissionTr { get; set; } = string.Empty;
    public string MissionEn { get; set; } = string.Empty;

   
    public string VisionTr { get; set; } = string.Empty;
    public string VisionEn { get; set; } = string.Empty;

   
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}