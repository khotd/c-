using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using project.Models.DTO;
using project.Services.Interfaces;

namespace project.Controllers;

[ApiController]
[Route("api/shelters")]
[Produces("application/json")]
public class SheltersController : ControllerBase
{
    private readonly IShelterService _service;
    private readonly ILogger<SheltersController> _logger;

    public SheltersController(IShelterService service, ILogger<SheltersController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ShelterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ShelterDto>>> GetAll()
    {
        var shelters = await _service.GetAllAsync();
        return Ok(shelters);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShelterDto>> GetById(int id)
    {
        var shelter = await _service.GetByIdAsync(id);
        if (shelter == null)
            return NotFound();
        return Ok(shelter);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShelterDto>> Create([FromBody] CreateShelterDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var shelter = await _service.CreateAsync(dto, role);
        return CreatedAtAction(nameof(GetById), new { id = shelter.Id }, shelter);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ShelterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ShelterDto>> Update(int id, [FromBody] UpdateShelterDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var shelter = await _service.UpdateAsync(id, dto, role);
        return Ok(shelter);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var deleted = await _service.DeleteAsync(id, role);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
