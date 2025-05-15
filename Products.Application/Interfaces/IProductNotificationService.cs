using Products.Application.DTOs;

namespace Products.Application.Interfaces
{
    public interface IProductNotificationService
    {
        Task NotifyProductCreated(ProductDto product);
        Task NotifyProductUpdated(ProductDto product);
        Task NotifyProductDeleted(int productId);
        Task NotifyCategoryCreated(CategoryDto category);
        Task NotifyCategoryUpdated(CategoryDto category);
        Task NotifyCategoryDeleted(int categoryId);
        Task NotifySupplierCreated(SupplierDto supplier);
        Task NotifySupplierUpdated(SupplierDto supplier);
        Task NotifySupplierDeleted(int supplierId);
    }
}
