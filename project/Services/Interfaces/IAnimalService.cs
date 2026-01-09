using project.Models.DTO;
using project.Models.Entities;

namespace project.Services.Interfaces;

public interface IAnimalService
{
    Task<PagedResponse<AnimalDto>> GetPagedAsync(AnimalFilterDto filter);
    Task<AnimalDto?> GetByIdAsync(int id);
    Task<AnimalDto> CreateAsync(CreateAnimalDto dto, string? currentUserRole);
    Task<AnimalDto> UpdateAsync(int id, UpdateAnimalDto dto, string? currentUserRole);
    Task<bool> DeleteAsync(int id, string? currentUserRole);
    Task<List<AnimalDto>> GetByShelterIdAsync(int shelterId);
}
