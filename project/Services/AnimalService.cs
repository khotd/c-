using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using project.Models.DTO;
using project.Models.Entities;
using project.Repositories.Interfaces;
using project.Services.Interfaces;

namespace project.Services;

public class AnimalService : IAnimalService
{
    private readonly IAnimalRepository _repository;
    private readonly IShelterRepository _shelterRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AnimalService> _logger;

    public AnimalService(
        IAnimalRepository repository,
        IShelterRepository shelterRepository,
        IDistributedCache cache,
        ILogger<AnimalService> logger)
    {
        _repository = repository;
        _shelterRepository = shelterRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResponse<AnimalDto>> GetPagedAsync(AnimalFilterDto filter)
    {
        _logger.LogInformation("Getting paged animals with filter: Page={Page}, PageSize={PageSize}, Search={Search}",
            filter.Page, filter.PageSize, filter.Search);

        var cacheKey = $"animals_paged_{filter.Page}_{filter.PageSize}_{filter.Search}_{filter.Species}_{filter.Status}_{filter.ShelterId}";
        
        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached animals data");
            return JsonSerializer.Deserialize<PagedResponse<AnimalDto>>(cached)!;
        }

        var result = await _repository.GetPagedAsync(filter);
        var dtos = result.Items.Select(MapToDto).ToList();

        var response = new PagedResponse<AnimalDto>
        {
            Items = dtos,
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize
        };

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), options);

        return response;
    }

    public async Task<AnimalDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting animal with ID {AnimalId}", id);
        var animal = await _repository.GetByIdAsync(id);
        return animal == null ? null : MapToDto(animal);
    }

    public async Task<AnimalDto> CreateAsync(CreateAnimalDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Creating new animal: {Name} by {Role}", dto.Name, currentUserRole);
        
        if (currentUserRole != "Admin" && currentUserRole != "Manager")
            throw new UnauthorizedAccessException("Only Admin and Manager can create animals");

        var shelter = await _shelterRepository.GetByIdAsync(dto.ShelterId);
        if (shelter == null)
            throw new KeyNotFoundException("Shelter not found");

        var animal = new Animal
        {
            Name = dto.Name,
            Species = dto.Species,
            Breed = dto.Breed,
            Age = dto.Age,
            Gender = dto.Gender,
            Status = dto.Status,
            Description = dto.Description,
            ShelterId = dto.ShelterId,
            ArrivalDate = dto.ArrivalDate,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(animal);
        
        await InvalidateCacheAsync();
        
        _logger.LogInformation("Animal created with ID {AnimalId}", created.Id);
        return MapToDto(created);
    }

    public async Task<AnimalDto> UpdateAsync(int id, UpdateAnimalDto dto, string? currentUserRole)
    {
        _logger.LogInformation("Updating animal {AnimalId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin" && currentUserRole != "Manager")
            throw new UnauthorizedAccessException("Only Admin and Manager can update animals");

        var animal = await _repository.GetByIdAsync(id);
        if (animal == null)
            throw new KeyNotFoundException("Animal not found");

        if (dto.Name != null) animal.Name = dto.Name;
        if (dto.Species != null) animal.Species = dto.Species;
        if (dto.Breed != null) animal.Breed = dto.Breed;
        if (dto.Age.HasValue) animal.Age = dto.Age.Value;
        if (dto.Gender != null) animal.Gender = dto.Gender;
        if (dto.Status != null) animal.Status = dto.Status;
        if (dto.Description != null) animal.Description = dto.Description;
        if (dto.ShelterId.HasValue)
        {
            var shelter = await _shelterRepository.GetByIdAsync(dto.ShelterId.Value);
            if (shelter == null)
                throw new KeyNotFoundException("Shelter not found");
            animal.ShelterId = dto.ShelterId.Value;
        }

        var updated = await _repository.UpdateAsync(animal);
        
        await InvalidateCacheAsync();
        
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id, string? currentUserRole)
    {
        _logger.LogInformation("Deleting animal {AnimalId} by {Role}", id, currentUserRole);
        
        if (currentUserRole != "Admin")
            throw new UnauthorizedAccessException("Only Admin can delete animals");

        var result = await _repository.DeleteAsync(id);
        
        if (result) await InvalidateCacheAsync();
        
        return result;
    }

    public async Task<List<AnimalDto>> GetByShelterIdAsync(int shelterId)
    {
        _logger.LogInformation("Getting animals for shelter {ShelterId}", shelterId);
        var animals = await _repository.GetByShelterIdAsync(shelterId);
        return animals.Select(MapToDto).ToList();
    }

    private async Task InvalidateCacheAsync()
    {
        _logger.LogInformation("Invalidating animal cache");
    }

    private static AnimalDto MapToDto(Animal animal)
    {
        return new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Species = animal.Species,
            Breed = animal.Breed,
            Age = animal.Age,
            Gender = animal.Gender,
            Status = animal.Status,
            Description = animal.Description,
            ShelterId = animal.ShelterId,
            ShelterName = animal.Shelter?.Name,
            ArrivalDate = animal.ArrivalDate,
            CreatedAt = animal.CreatedAt
        };
    }
}
