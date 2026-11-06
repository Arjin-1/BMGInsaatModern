using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.API.Contracts.Auth;

public sealed class ResetPasswordRequest
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre sifirlama kodu zorunludur.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre zorunludur.")]
    [MinLength(12, ErrorMessage = "Sifre en az 12 karakter olmalidir.")]
    [StringLength(128)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre tekrari zorunludur.")]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "Sifreler eslesmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
