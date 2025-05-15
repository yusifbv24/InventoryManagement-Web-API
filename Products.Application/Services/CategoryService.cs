using AutoMapper;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductNotificationService _notificationService;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IProductNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto, CancellationToken cancellationToken = default)
        {
            // Check if category with same name already exists
            if (await _unitOfWork.Categories.ExistsByNameAsync(createCategoryDto.Name, cancellationToken))
                throw new ApplicationException($"Category with name '{createCategoryDto.Name}' already exists.");

            var category = _mapper.Map<Category>(createCategoryDto);

            category = await _unitOfWork.Categories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = _mapper.Map<CategoryDto>(category);

            // Send real-time notification via SignalR
            await _notificationService.NotifyCategoryCreated(categoryDto);

            return categoryDto;
        }

        public async Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(updateCategoryDto.Id, cancellationToken);
            if (category == null)
                throw new ApplicationException($"Category with ID {updateCategoryDto.Id} not found.");

            // Check if another category with the same name exists
            if (category.Name != updateCategoryDto.Name &&
                await _unitOfWork.Categories.ExistsByNameAsync(updateCategoryDto.Name, cancellationToken))
                throw new ApplicationException($"Category with name '{updateCategoryDto.Name}' already exists.");

            category.Update(updateCategoryDto.Name, updateCategoryDto.Description);

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = _mapper.Map<CategoryDto>(category);

            // Send real-time notification via SignalR
            await _notificationService.NotifyCategoryUpdated(categoryDto);
        }

        public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            // Check if category exists
            if (!await _unitOfWork.Categories.ExistsByIdAsync(id, cancellationToken))
                throw new ApplicationException($"Category with ID {id} not found.");

            // Check if there are products in this category
            var productsInCategory = await _unitOfWork.Products.GetByCategoryIdAsync(id, cancellationToken);
            if (productsInCategory.Any())
                throw new ApplicationException($"Cannot delete category with ID {id} because there are products associated with it.");

            await _unitOfWork.Categories.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send real-time notification via SignalR
            await _notificationService.NotifyCategoryDeleted(id);
        }

        public async Task ActivateCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
            if (category == null)
                throw new ApplicationException($"Category with ID {id} not found.");

            category.Activate();
            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = _mapper.Map<CategoryDto>(category);
            await _notificationService.NotifyCategoryUpdated(categoryDto);
        }

        public async Task DeactivateCategoryAsync(int id, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);
            if (category == null)
                throw new ApplicationException($"Category with ID {id} not found.");

            category.Deactivate();
            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var categoryDto = _mapper.Map<CategoryDto>(category);
            await _notificationService.NotifyCategoryUpdated(categoryDto);
        }
    }
}
