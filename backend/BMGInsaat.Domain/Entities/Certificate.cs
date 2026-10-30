namespace BMGInsaat.Domain.Entities;

public class Certificate
{
    public int Id { get; set; }

    // Sertifika adi - Turkce ve Ingilizce
    public string NameTr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;

    // Yuklenen sertifika gorselinin adresi
    public string ImageUrl { get; set; } = string.Empty;

    // Sertifika web sitesinde gorunsun mu?
    public bool IsPublished { get; set; } = false;

    // Sertifikalarin goruntulenme sirasi
    public int DisplayOrder { get; set; } = 0;

    // Kayit tarihleri
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
