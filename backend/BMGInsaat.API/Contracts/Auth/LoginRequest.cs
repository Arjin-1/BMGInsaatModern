using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.API.Contracts.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
}
