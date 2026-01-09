using project.Models.DTO;
using project.Models.Entities;

namespace project.Services.Interfaces;

public interface IAdoptionService
{
    Task<List<AdoptionDto>> GetAllAsync();
    Task<AdoptionDto?> GetByIdAsync(int id);
    Task<AdoptionDto> CreateAsync(CreateAdoptionDto dto, string? currentUserRole);
    Task<AdoptionDto> UpdateAsync(int id, UpdateAdoptionDto dto, string? currentUserRole);
    Task<bool> DeleteAsync(int id, string? currentUserRole);
}
