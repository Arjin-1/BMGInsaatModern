namespace BMGInsaat.Application.DTOs.About;

public class SaveCompanyValueRequest
{
    public string TitleTr { get; set; } = string.Empty;
    public string? TitleEn { get; set; }

    public string DescriptionTr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
