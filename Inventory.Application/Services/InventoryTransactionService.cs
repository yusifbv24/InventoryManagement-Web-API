using AutoMapper;
using Inventory.Application.DTOs.InventoryTransaction;
using Inventory.Application.Interfaces;
using Inventory.Domain.Repositories;

namespace Inventory.Application.Services
{
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductApiClient _productApiClient;

        public InventoryTransactionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IProductApiClient productApiClient)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productApiClient = productApiClient;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetAllTransactionsAsync(CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.Transactions.GetAllAsync(cancellationToken);
            var transactionDtos = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);

            // Enrich with product information
            await EnrichTransactionsWithProductInfo(transactionDtos, cancellationToken);

            return transactionDtos;
        }

        public async Task<InventoryTransactionDto?> GetTransactionByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id, cancellationToken);
            if (transaction == null)
                return null;

            var transactionDto = _mapper.Map<InventoryTransactionDto>(transaction);

            // Enrich with product information
            var product = await _productApiClient.GetProductByIdAsync(transactionDto.ProductId, cancellationToken);
            if (product != null)
            {
                transactionDto.ProductName = product.Name;
                transactionDto.ProductSku = product.SKU;
            }

            // Enrich with warehouse information
            if (transaction.SourceWarehouseId.HasValue)
            {
                var sourceWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(transaction.SourceWarehouseId.Value, cancellationToken);
                if (sourceWarehouse != null)
                    transactionDto.SourceWarehouseName = sourceWarehouse.Name;
            }

            if (transaction.DestinationWarehouseId.HasValue)
            {
                var destWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(transaction.DestinationWarehouseId.Value, cancellationToken);
                if (destWarehouse != null)
                    transactionDto.DestinationWarehouseName = destWarehouse.Name;
            }

            return transactionDto;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.Transactions.GetByProductIdAsync(productId, cancellationToken);
            var transactionDtos = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);

            // Enrich with product information
            var product = await _productApiClient.GetProductByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                foreach (var dto in transactionDtos)
                {
                    dto.ProductName = product.Name;
                    dto.ProductSku = product.SKU;
                }
            }

            return transactionDtos;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.Transactions.GetByWarehouseIdAsync(warehouseId, cancellationToken);
            var transactionDtos = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);

            // Enrich with product information
            await EnrichTransactionsWithProductInfo(transactionDtos, cancellationToken);

            return transactionDtos;
        }

        public async Task<IEnumerable<InventoryTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.Transactions.GetByDateRangeAsync(startDate, endDate, cancellationToken);
            var transactionDtos = _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);

            // Enrich with product information
            await EnrichTransactionsWithProductInfo(transactionDtos, cancellationToken);

            return transactionDtos;
        }

        // Helper method to enrich transactions with product information
        private async Task EnrichTransactionsWithProductInfo(IEnumerable<InventoryTransactionDto> transactionDtos, CancellationToken cancellationToken)
        {
            var productIds = transactionDtos.Select(t => t.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var dto in transactionDtos)
            {
                if (productDict.TryGetValue(dto.ProductId, out var product))
                {
                    dto.ProductName = product.Name;
                    dto.ProductSku = product.SKU;
                }
            }
        }
    }
}
