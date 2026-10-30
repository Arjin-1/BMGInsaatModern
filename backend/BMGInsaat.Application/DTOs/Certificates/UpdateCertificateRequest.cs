using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.Application.DTOs.Certificates;

public class UpdateCertificateRequest
{
    [Required(ErrorMessage = "Turkce sertifika adi zorunludur.")]
    [StringLength(200, MinimumLength = 1)]
    public string NameTr { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingilizce sertifika adi zorunludur.")]
    [StringLength(200, MinimumLength = 1)]
    public string NameEn { get; set; } = string.Empty;

    [Range(0, int.MaxValue,
        ErrorMessage = "Goruntuleme sirasi negatif olamaz.")]
    public int DisplayOrder { get; set; } = 0;

    public bool IsPublished { get; set; } = false;
}
