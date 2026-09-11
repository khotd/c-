using project.Models.DTO;
using project.Models.Entities;
using project.Repositories.Interfaces;
using project.Services.Interfaces;

namespace project.Services;

public class ShelterService : IShelterService
{
    private readonly IShelterRepository _repository;
    private readonly ILogger<ShelterService> _logger;

    public ShelterService(IShelterRepository repository, ILogger<ShelterService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<ShelterDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all shelters");
        var shelters = await _repository.GetAllAsync();
        var dtos = new List<ShelterDto>();
        
        foreach (var shelter in shelters)
        {
            var animalCount = await _repository.GetAnimalCountAsync(shelter.Id);
            dtos.Add(MapToDto(shelter, animalCount));
        }
        
        return dtos;
    }

    public async Task<ShelterDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting shelter with ID {ShelterId}", id);
        var shelter = await _repository.GetByIdAsync(id);
        if (shelter == null) return null;
        
        var animalCount = await _repository.GetAnimalCountAsync(id);
        return MapToDto(shelter, animalCount);
    }

    public async Task<ShelterDto> CreateAsync(CreateShelterDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Creating new shelter: {Name} by {Role}", dto.Name, currentUserRole);
        
        if (currentUserRole != "Admin" && currentUserRole != "Manager")
            throw new UnauthorizedAccessException("Only Admin and Manager can create shelters");

        var shelter = new Shelter
        {
            Name = dto.Name,
            Address = dto.Address,
            Phone = dto.Phone,
            Email = dto.Email,
            Capacity = dto.Capacity,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(shelter);
        _logger.LogInformation("Shelter created with ID {ShelterId}", created.Id);
        return MapToDto(created, 0);
    }

    public async Task<ShelterDto> UpdateAsync(int id, UpdateShelterDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Updating shelter {ShelterId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can update shelters");

        var shelter = await _repository.GetByIdAsync(id);
        if (shelter == null)
            throw new KeyNotFoundException("Shelter not found");

        if (dto.Name != null) shelter.Name = dto.Name;
        if (dto.Address != null) shelter.Address = dto.Address;
        if (dto.Phone != null) shelter.Phone = dto.Phone;
        if (dto.Email != null) shelter.Email = dto.Email;
        if (dto.Capacity.HasValue) shelter.Capacity = dto.Capacity.Value;

        var updated = await _repository.UpdateAsync(shelter);
        var animalCount = await _repository.GetAnimalCountAsync(id);
        return MapToDto(updated, animalCount);
    }

    public async Task<bool> DeleteAsync(int id, string? currentUserRole)
    {
        _logger.LogInformation("Deleting shelter {ShelterId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can delete shelters");

        return await _repository.DeleteAsync(id);
    }

    private static ShelterDto MapToDto(Shelter shelter, int animalCount)
    {
        return new ShelterDto
        {
            Id = shelter.Id,
            Name = shelter.Name,
            Address = shelter.Address,
            Phone = shelter.Phone,
            Email = shelter.Email,
            Capacity = shelter.Capacity,
            CurrentAnimals = animalCount,
            CreatedAt = shelter.CreatedAt
        };
    }
}
