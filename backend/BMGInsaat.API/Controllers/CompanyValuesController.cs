using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/company-values")]
[AllowAnonymous]
public class CompanyValuesController : ControllerBase
{
    private readonly ICompanyValueService _companyValueService;

    public CompanyValuesController(
        ICompanyValueService companyValueService)
    {
        _companyValueService = companyValueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublished(
        CancellationToken cancellationToken)
    {
        var values = await _companyValueService.GetPublishedAsync(
            cancellationToken);

        return Ok(values);
    }
}
