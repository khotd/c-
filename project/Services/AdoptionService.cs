using project.Models.DTO;
using project.Models.Entities;
using project.Repositories.Interfaces;
using project.Services.Interfaces;

namespace project.Services;

public class AdoptionService : IAdoptionService
{
    private readonly IAdoptionRepository _repository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AdoptionService> _logger;

    public AdoptionService(
        IAdoptionRepository repository,
        IAnimalRepository animalRepository,
        IUserRepository userRepository,
        ILogger<AdoptionService> logger)
    {
        _repository = repository;
        _animalRepository = animalRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<AdoptionDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all adoptions");
        var adoptions = await _repository.GetAllAsync();
        return adoptions.Select(MapToDto).ToList();
    }

    public async Task<AdoptionDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting adoption with ID {AdoptionId}", id);
        var adoption = await _repository.GetByIdAsync(id);
        return adoption == null ? null : MapToDto(adoption);
    }

    public async Task<AdoptionDto> CreateAsync(CreateAdoptionDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Creating new adoption by {Role}", currentUserRole);
        
        if (string.IsNullOrEmpty(currentUserRole))
            throw new UnauthorizedAccessException("Authentication required");

        var animal = await _animalRepository.GetByIdAsync(dto.AnimalId);
        if (animal == null)
            throw new KeyNotFoundException("Animal not found");

        if (animal.Status == "Adopted")
            throw new InvalidOperationException("Animal is already adopted");

        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        var adoption = new Adoption
        {
            AnimalId = dto.AnimalId,
            UserId = dto.UserId,
            AdoptionDate = dto.AdoptionDate,
            Status = "Pending",
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(adoption);
        _logger.LogInformation("Adoption created with ID {AdoptionId}", created.Id);
        return MapToDto(created);
    }

    public async Task<AdoptionDto> UpdateAsync(int id, UpdateAdoptionDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Updating adoption {AdoptionId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin" && currentUserRole != "Manager")
            throw new UnauthorizedAccessException("Only Admin and Manager can update adoptions");

        var adoption = await _repository.GetByIdAsync(id);
        if (adoption == null)
            throw new KeyNotFoundException("Adoption not found");

        if (dto.Status != null)
        {
            adoption.Status = dto.Status;
            
            if (dto.Status == "Approved" || dto.Status == "Completed")
            {
                var animal = await _animalRepository.GetByIdAsync(adoption.AnimalId);
                if (animal != null)
                {
                    animal.Status = "Adopted";
                    await _animalRepository.UpdateAsync(animal);
                }
            }
        }
        
        if (dto.Notes != null) adoption.Notes = dto.Notes;

        var updated = await _repository.UpdateAsync(adoption);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, string? currentUserRole)
    {
        _logger.LogInformation("Deleting adoption {AdoptionId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can delete adoptions");

        return await _repository.DeleteAsync(id);
    }

    private static AdoptionDto MapToDto(Adoption adoption)
    {
        return new AdoptionDto
        {
            Id = adoption.Id,
            AnimalId = adoption.AnimalId,
            AnimalName = adoption.Animal?.Name,
            UserId = adoption.UserId,
            UserName = adoption.User?.Username,
            AdoptionDate = adoption.AdoptionDate,
            Status = adoption.Status,
            Notes = adoption.Notes,
            CreatedAt = adoption.CreatedAt
        };
    }
}
