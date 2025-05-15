using AutoMapper;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Application.Messages;
using Products.Domain.Entities;
using Products.Domain.Repositories;

namespace Products.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMessageBus _messageBus;
        private readonly IProductNotificationService _notificationService;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, IMessageBus messageBus, IProductNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messageBus = messageBus;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.Products.GetByCategoryIdAsync(categoryId, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsBySupplierAsync(int supplierId, CancellationToken cancellationToken = default)
        {
            var products = await _unitOfWork.Products.GetBySupplierIdAsync(supplierId, cancellationToken);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto, CancellationToken cancellationToken = default)
        {
            // Validate category and supplier existence
            if (!await _unitOfWork.Categories.ExistsByIdAsync(createProductDto.CategoryId, cancellationToken))
                throw new ApplicationException($"Category with ID {createProductDto.CategoryId} does not exist.");

            if (!await _unitOfWork.Suppliers.ExistsByIdAsync(createProductDto.SupplierId, cancellationToken))
                throw new ApplicationException($"Supplier with ID {createProductDto.SupplierId} does not exist.");

            // Check SKU uniqueness
            if (await _unitOfWork.Products.ExistsBySKUAsync(createProductDto.SKU, cancellationToken))
                throw new ApplicationException($"Product with SKU {createProductDto.SKU} already exists.");

            var product = _mapper.Map<Product>(createProductDto);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                product = await _unitOfWork.Products.AddAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish message to RabbitMQ
                var message = new ProductCreatedMessage
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    SKU = product.SKU,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    SupplierId = product.SupplierId
                };

                _messageBus.Publish(message, "product_events", "product.created");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var productDto = _mapper.Map<ProductDto>(product);

                // Send real-time notification via SignalR
                await _notificationService.NotifyProductCreated(productDto);

                return productDto;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task UpdateProductAsync(UpdateProductDto updateProductDto, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(updateProductDto.Id, cancellationToken);
            if (product == null)
                throw new ApplicationException($"Product with ID {updateProductDto.Id} not found.");

            // Validate category and supplier existence
            if (!await _unitOfWork.Categories.ExistsByIdAsync(updateProductDto.CategoryId, cancellationToken))
                throw new ApplicationException($"Category with ID {updateProductDto.CategoryId} does not exist.");

            if (!await _unitOfWork.Suppliers.ExistsByIdAsync(updateProductDto.SupplierId, cancellationToken))
                throw new ApplicationException($"Supplier with ID {updateProductDto.SupplierId} does not exist.");

            // Check SKU uniqueness (if changed)
            if (product.SKU != updateProductDto.SKU && await _unitOfWork.Products.ExistsBySKUAsync(updateProductDto.SKU, cancellationToken))
                throw new ApplicationException($"Product with SKU {updateProductDto.SKU} already exists.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                product.UpdateDetails(
                    updateProductDto.Name,
                    updateProductDto.Description,
                    updateProductDto.Price,
                    updateProductDto.SKU,
                    updateProductDto.CategoryId,
                    updateProductDto.SupplierId,
                    updateProductDto.ImageUrl,
                    updateProductDto.Weight,
                    updateProductDto.Dimensions
                );

                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish message to RabbitMQ
                var message = new ProductUpdatedMessage
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    SKU = product.SKU,
                    Price = product.Price,
                    CategoryId = product.CategoryId,
                    SupplierId = product.SupplierId
                };

                _messageBus.Publish(message, "product_events", "product.updated");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var productDto = _mapper.Map<ProductDto>(product);

                // Send real-time notification via SignalR
                await _notificationService.NotifyProductUpdated(productDto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task DeleteProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
                throw new ApplicationException($"Product with ID {id} not found.");

            var sku = product.SKU; // Save SKU for message publishing

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await _unitOfWork.Products.DeleteAsync(id, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish message to RabbitMQ
                var message = new ProductDeletedMessage
                {
                    ProductId = id,
                    SKU = sku
                };

                _messageBus.Publish(message, "product_events", "product.deleted");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Send real-time notification via SignalR
                await _notificationService.NotifyProductDeleted(id);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task ActivateProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
                throw new ApplicationException($"Product with ID {id} not found.");

            product.Activate();
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);
            await _notificationService.NotifyProductUpdated(productDto);
        }

        public async Task DeactivateProductAsync(int id, CancellationToken cancellationToken = default)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
                throw new ApplicationException($"Product with ID {id} not found.");

            product.Deactivate();
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);
            await _notificationService.NotifyProductUpdated(productDto);
        }
    }
}
