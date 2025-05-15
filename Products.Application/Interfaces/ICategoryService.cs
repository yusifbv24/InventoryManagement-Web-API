using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default);
        Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default);
        Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
        Task ActivateCategoryAsync(int id, CancellationToken cancellationToken = default);
        Task DeactivateCategoryAsync(int id, CancellationToken cancellationToken = default);
    }
}
