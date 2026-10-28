
using System.ComponentModel.DataAnnotations;

namespace BMGInsaat.Application.DTOs.Contact;

public class SendContactMessageRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(5000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
}
