namespace BMGInsaat.Application.DTOs.ConstructionServices;

public class UpdateConstructionServiceRequest
{
    public string TitleTr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;

    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
