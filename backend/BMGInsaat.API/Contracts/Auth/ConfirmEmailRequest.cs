using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.API.Contracts.Auth;

public sealed class ConfirmEmailRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;
}
