using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using project.Models.DTO;
using project.Services.Interfaces;

namespace project.Controllers;

[ApiController]
[Route("api/animals")]
[Produces("application/json")]
public class AnimalsController : ControllerBase
{
    private readonly IAnimalService _service;
    private readonly ILogger<AnimalsController> _logger;

    public AnimalsController(IAnimalService service, ILogger<AnimalsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<AnimalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AnimalDto>>> GetPaged([FromQuery] AnimalFilterDto filter)
    {
        var result = await _service.GetPagedAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AnimalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AnimalDto>> GetById(int id)
    {
        var animal = await _service.GetByIdAsync(id);
        if (animal == null)
            return NotFound();
        return Ok(animal);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(AnimalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnimalDto>> Create([FromBody] CreateAnimalDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var animal = await _service.CreateAsync(dto, role);
        return CreatedAtAction(nameof(GetById), new { id = animal.Id }, animal);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(AnimalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AnimalDto>> Update(int id, [FromBody] UpdateAnimalDto dto)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var animal = await _service.UpdateAsync(id, dto, role);
        return Ok(animal);
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

    [HttpGet("shelter/{shelterId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<AnimalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AnimalDto>>> GetByShelter(int shelterId)
    {
        var animals = await _service.GetByShelterIdAsync(shelterId);
        return Ok(animals);
    }
}
