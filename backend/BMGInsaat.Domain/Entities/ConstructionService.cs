namespace BMGInsaat.Domain.Entities;

public class ConstructionService
{
    public int Id { get; set; }

    // Hizmet basligi - Turkce ve Ingilizce
    public string TitleTr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;

    // Hizmet aciklamasi - Turkce ve Ingilizce
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    // Hizmete ait gorselin adresi
    public string ImageUrl { get; set; } = string.Empty;

    // Hizmet web sitesinde gorunsun mu?
    public bool IsPublished { get; set; } = false;

    // Hizmetlerin goruntulenme sirasi
    public int DisplayOrder { get; set; } = 0;

    // Kayit tarihleri
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
