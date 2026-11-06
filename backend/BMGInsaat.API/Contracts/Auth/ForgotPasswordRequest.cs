using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.API.Contracts.Auth;

public sealed class ForgotPasswordRequest
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
