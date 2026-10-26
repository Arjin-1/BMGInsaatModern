using BMGInsaat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BMGInsaat.Application.DTOs.About;

namespace BMGInsaat.API.Controllers;

[ApiController]
[Route("api/admin/company-values")]
[Authorize(Roles = "SuperAdmin")]
public class AdminCompanyValuesController : ControllerBase
{
    private readonly ICompanyValueService _companyValueService;

    public AdminCompanyValuesController(
        ICompanyValueService companyValueService)
    {
        _companyValueService = companyValueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var values = await _companyValueService.GetAllAsync(
            cancellationToken);

        return Ok(values);
    }

    
[HttpGet("{id:int}")]
public async Task<IActionResult> GetById(
    int id,
    CancellationToken cancellationToken)
{
    var companyValue = await _companyValueService.GetByIdAsync(
        id,
        cancellationToken);

    if (companyValue is null)
    {
        return NotFound(new
        {
            message = "Sirket degeri bulunamadi."
        });
    }

    return Ok(companyValue);
}


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    [FromBody] SaveCompanyValueRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        var createdValue = await _companyValueService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdValue.Id },
            createdValue);
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}


[HttpPut("{id:int}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Update(
    int id,
    [FromBody] SaveCompanyValueRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        var updated = await _companyValueService.UpdateAsync(
            id,
            request,
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Sirket degeri bulunamadi."
            });
        }

        return NoContent();
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}


[HttpDelete("{id:int}")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(
    int id,
    CancellationToken cancellationToken)
{
    var deleted = await _companyValueService.DeleteAsync(
        id,
        cancellationToken);

    if (!deleted)
    {
        return NotFound(new
        {
            message = "Sirket degeri bulunamadi."
        });
    }

    return NoContent();
}


}
