
using BMGInsaat.Application.DTOs.Contact;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    [HttpPost]
    public IActionResult SendMessage(
        [FromBody] SendContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        // [ApiController], DTO uzerindeki DataAnnotations
        // kurallarini otomatik olarak kontrol eder.

        // E-posta gonderim servisi henuz eklenmedi.
        // Mesajin gonderildigi izlenimini vermiyoruz.
        return StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new
            {
                message = "İletişim formu gönderimi henüz etkinleştirilmedi."
            });
    }
}
