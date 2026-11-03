
namespace BMGInsaat.Domain.Entities;

public class ProjectVideo
{
    public int Id { get; set; }

    // Video basligi - Turkce ve Ingilizce
    public string TitleTr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;

    // Video aciklamasi - Turkce ve Ingilizce
    public string DescriptionTr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;

    // Sunucuya yuklenen video dosyasinin adresi
    public string VideoUrl { get; set; } = string.Empty;

    // Video web sitesinde gorunsun mu?
    public bool IsPublished { get; set; } = false;

    // Videolarin goruntulenme sirasi
    public int DisplayOrder { get; set; } = 0;

    // Kayit tarihleri
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
