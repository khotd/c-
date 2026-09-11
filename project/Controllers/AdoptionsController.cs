using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using project.Models.DTO;
using project.Services.Interfaces;

namespace project.Controllers;

[Authorize]
[ApiController]
[Route("api/adoptions")]
[Produces("application/json")]
public class AdoptionsController : ControllerBase
{
    private readonly IAdoptionService _service;
    private readonly ILogger<AdoptionsController> _logger;

    public AdoptionsController(IAdoptionService service, ILogger<AdoptionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(List<AdoptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdoptionDto>>> GetAll()
    {
        var adoptions = await _service.GetAllAsync();
        return Ok(adoptions);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,User")]
    [ProducesResponseType(typeof(AdoptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdoptionDto>> GetById(int id)
    {
        var adoption = await _service.GetByIdAsync(id);
        if (adoption == null)
            return NotFound();
        return Ok(adoption);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,User")]
    [ProducesResponseType(typeof(AdoptionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdoptionDto>> Create([FromBody] CreateAdoptionDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var adoption = await _service.CreateAsync(dto, role);
        return CreatedAtAction(nameof(GetById), new { id = adoption.Id }, adoption);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(AdoptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdoptionDto>> Update(int id, [FromBody] UpdateAdoptionDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var adoption = await _service.UpdateAsync(id, dto, role);
        return Ok(adoption);
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
