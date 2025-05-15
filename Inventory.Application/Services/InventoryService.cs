using AutoMapper;
using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.InventoryTransaction;
using Inventory.Application.DTOs.StockReservation;
using Inventory.Application.Interfaces;
using Inventory.Application.Messages;
using Inventory.Domain.Entities;
using Inventory.Domain.Repositories;

namespace Inventory.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductApiClient _productApiClient;
        private readonly IMessageBus _messageBus;
        private readonly IInventoryNotificationService _notificationService;

        public InventoryService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IProductApiClient productApiClient,
            IMessageBus messageBus,
            IInventoryNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productApiClient = productApiClient;
            _messageBus = messageBus;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetAllInventoryItemsAsync(CancellationToken cancellationToken = default)
        {
            var inventoryItems = await _unitOfWork.InventoryItems.GetAllAsync(cancellationToken);
            var itemDtos = _mapper.Map<IEnumerable<InventoryItemDto>>(inventoryItems);

            // Enrich with product information
            var productIds = itemDtos.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in itemDtos)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    item.ProductName = product.Name;
                    item.ProductSku = product.SKU;
                }
            }

            return itemDtos;
        }

        public async Task<InventoryItemDto?> GetInventoryItemByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var inventoryItem = await _unitOfWork.InventoryItems.GetByIdAsync(id, cancellationToken);
            if (inventoryItem == null)
                return null;

            var itemDto = _mapper.Map<InventoryItemDto>(inventoryItem);

            // Enrich with product information
            var product = await _productApiClient.GetProductByIdAsync(itemDto.ProductId, cancellationToken);
            if (product != null)
            {
                itemDto.ProductName = product.Name;
                itemDto.ProductSku = product.SKU;
            }

            return itemDto;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetInventoryItemsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
        {
            var inventoryItems = await _unitOfWork.InventoryItems.GetByProductIdAsync(productId, cancellationToken);
            var itemDtos = _mapper.Map<IEnumerable<InventoryItemDto>>(inventoryItems);

            // Enrich with product information
            var product = await _productApiClient.GetProductByIdAsync(productId, cancellationToken);
            if (product != null)
            {
                foreach (var item in itemDtos)
                {
                    item.ProductName = product.Name;
                    item.ProductSku = product.SKU;
                }
            }

            return itemDtos;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetInventoryItemsByWarehouseIdAsync(int warehouseId, CancellationToken cancellationToken = default)
        {
            var inventoryItems = await _unitOfWork.InventoryItems.GetByWarehouseIdAsync(warehouseId, cancellationToken);
            var itemDtos = _mapper.Map<IEnumerable<InventoryItemDto>>(inventoryItems);

            // Enrich with product information
            var productIds = itemDtos.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in itemDtos)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    item.ProductName = product.Name;
                    item.ProductSku = product.SKU;
                }
            }

            return itemDtos;
        }

        public async Task<IEnumerable<InventoryItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken = default)
        {
            var lowStockItems = await _unitOfWork.InventoryItems.GetLowStockItemsAsync(cancellationToken);
            var itemDtos = _mapper.Map<IEnumerable<InventoryItemDto>>(lowStockItems);

            // Enrich with product information
            var productIds = itemDtos.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            foreach (var item in itemDtos)
            {
                if (productDict.TryGetValue(item.ProductId, out var product))
                {
                    item.ProductName = product.Name;
                    item.ProductSku = product.SKU;
                }
            }

            return itemDtos;
        }

        public async Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto createInventoryItemDto, CancellationToken cancellationToken = default)
        {
            // Validate warehouse existence
            if (!await _unitOfWork.Warehouses.ExistsByIdAsync(createInventoryItemDto.WarehouseId, cancellationToken))
                throw new ApplicationException($"Warehouse with ID {createInventoryItemDto.WarehouseId} does not exist.");

            // Validate product existence via Products API
            var product = await _productApiClient.GetProductByIdAsync(createInventoryItemDto.ProductId, cancellationToken);
            if (product == null)
                throw new ApplicationException($"Product with ID {createInventoryItemDto.ProductId} does not exist.");

            // Check if inventory item already exists for this product, warehouse, and location
            var existingItem = await _unitOfWork.InventoryItems.GetByProductAndWarehouseAsync(
                createInventoryItemDto.ProductId,
                createInventoryItemDto.WarehouseId,
                createInventoryItemDto.LocationCode,
                cancellationToken);

            if (existingItem != null)
                throw new ApplicationException($"Inventory item already exists for product {createInventoryItemDto.ProductId} in warehouse {createInventoryItemDto.WarehouseId} at location {createInventoryItemDto.LocationCode}.");

            // Validate inventory levels
            if (createInventoryItemDto.ReorderThreshold < 0)
                throw new ApplicationException("Reorder threshold cannot be negative.");

            if (createInventoryItemDto.TargetStockLevel < createInventoryItemDto.ReorderThreshold)
                throw new ApplicationException("Target stock level must be greater than or equal to reorder threshold.");

            var inventoryItem = _mapper.Map<InventoryItem>(createInventoryItemDto);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                inventoryItem = await _unitOfWork.InventoryItems.AddAsync(inventoryItem, cancellationToken);

                // Create an initial transaction record if quantity > 0
                if (inventoryItem.Quantity > 0)
                {
                    var transaction = new InventoryTransaction
                    {
                        ProductId = inventoryItem.ProductId,
                        WarehouseId = inventoryItem.WarehouseId,
                        LocationCode = inventoryItem.LocationCode,
                        Type = TransactionType.Received,
                        Quantity = inventoryItem.Quantity,
                        Timestamp = DateTime.UtcNow,
                        Notes = "Initial inventory setup",
                        CreatedBy = "System"
                    };

                    await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish inventory level changed message
                var message = new InventoryLevelChangedMessage
                {
                    ProductId = inventoryItem.ProductId,
                    WarehouseId = inventoryItem.WarehouseId,
                    LocationCode = inventoryItem.LocationCode,
                    NewQuantity = inventoryItem.Quantity,
                    OldQuantity = 0
                };

                _messageBus.Publish(message, "inventory_events", "inventory.level.changed");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var itemDto = _mapper.Map<InventoryItemDto>(inventoryItem);
                itemDto.ProductName = product.Name;
                itemDto.ProductSku = product.SKU;

                // Send real-time notification via SignalR
                await _notificationService.NotifyInventoryChanged(itemDto);

                return itemDto;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task UpdateInventoryItemAsync(UpdateInventoryItemDto updateInventoryItemDto, CancellationToken cancellationToken = default)
        {
            var inventoryItem = await _unitOfWork.InventoryItems.GetByIdAsync(updateInventoryItemDto.Id, cancellationToken);
            if (inventoryItem == null)
                throw new ApplicationException($"Inventory item with ID {updateInventoryItemDto.Id} not found.");

            // Validate inventory levels
            if (updateInventoryItemDto.ReorderThreshold < 0)
                throw new ApplicationException("Reorder threshold cannot be negative.");

            if (updateInventoryItemDto.TargetStockLevel < updateInventoryItemDto.ReorderThreshold)
                throw new ApplicationException("Target stock level must be greater than or equal to reorder threshold.");

            // Update inventory levels
            inventoryItem.UpdateStockLevels(updateInventoryItemDto.ReorderThreshold, updateInventoryItemDto.TargetStockLevel);

            await _unitOfWork.InventoryItems.UpdateAsync(inventoryItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Get product information for notifications
            var product = await _productApiClient.GetProductByIdAsync(inventoryItem.ProductId, cancellationToken);

            var itemDto = _mapper.Map<InventoryItemDto>(inventoryItem);
            if (product != null)
            {
                itemDto.ProductName = product.Name;
                itemDto.ProductSku = product.SKU;
            }

            // Send real-time notification via SignalR
            await _notificationService.NotifyInventoryChanged(itemDto);

            // Check for low stock after update
            if (inventoryItem.IsLowStock)
            {
                // Send low stock alert message
                var alertMessage = new LowStockAlertMessage
                {
                    ProductId = inventoryItem.ProductId,
                    ProductName = product?.Name ?? string.Empty,
                    ProductSku = product?.SKU ?? string.Empty,
                    WarehouseId = inventoryItem.WarehouseId,
                    WarehouseName = inventoryItem.Warehouse?.Name ?? string.Empty,
                    LocationCode = inventoryItem.LocationCode,
                    CurrentQuantity = inventoryItem.Quantity,
                    ReorderThreshold = inventoryItem.ReorderThreshold,
                    TargetStockLevel = inventoryItem.TargetStockLevel,
                    SuggestedOrderQuantity = inventoryItem.TargetStockLevel - inventoryItem.Quantity
                };

                _messageBus.Publish(alertMessage, "inventory_events", "inventory.low.stock");

                // Send real-time notification via SignalR
                await _notificationService.NotifyLowStock(itemDto);
            }
        }

        public async Task DeleteInventoryItemAsync(int id, CancellationToken cancellationToken = default)
        {
            var inventoryItem = await _unitOfWork.InventoryItems.GetByIdAsync(id, cancellationToken);
            if (inventoryItem == null)
                throw new ApplicationException($"Inventory item with ID {id} not found.");

            // Check if there are active reservations for this item
            var activeReservations = await _unitOfWork.Reservations.GetActiveReservationsAsync(cancellationToken);
            var hasActiveReservations = activeReservations.Any(r =>
                r.ProductId == inventoryItem.ProductId &&
                r.WarehouseId == inventoryItem.WarehouseId &&
                r.LocationCode == inventoryItem.LocationCode);

            if (hasActiveReservations)
                throw new ApplicationException($"Cannot delete inventory item with ID {id} because it has active reservations.");

            await _unitOfWork.InventoryItems.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<InventoryItemDto> AdjustInventoryAsync(AdjustInventoryDto adjustInventoryDto, CancellationToken cancellationToken = default)
        {
            var inventoryItem = await _unitOfWork.InventoryItems.GetByIdAsync(adjustInventoryDto.InventoryItemId, cancellationToken);
            if (inventoryItem == null)
                throw new ApplicationException($"Inventory item with ID {adjustInventoryDto.InventoryItemId} not found.");

            if (adjustInventoryDto.Quantity <= 0)
                throw new ApplicationException("Adjustment quantity must be greater than zero.");

            int oldQuantity = inventoryItem.Quantity;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Adjust the inventory
                if (adjustInventoryDto.IsAddition)
                {
                    inventoryItem.AddStock(adjustInventoryDto.Quantity);
                }
                else
                {
                    inventoryItem.RemoveStock(adjustInventoryDto.Quantity);
                }

                await _unitOfWork.InventoryItems.UpdateAsync(inventoryItem, cancellationToken);

                // Create a transaction record
                var transaction = new InventoryTransaction
                {
                    ProductId = inventoryItem.ProductId,
                    WarehouseId = inventoryItem.WarehouseId,
                    LocationCode = inventoryItem.LocationCode,
                    Type = adjustInventoryDto.IsAddition ? TransactionType.Received : TransactionType.Shipped,
                    Quantity = adjustInventoryDto.Quantity,
                    Timestamp = DateTime.UtcNow,
                    Notes = adjustInventoryDto.Notes,
                    CreatedBy = adjustInventoryDto.CreatedBy
                };

                await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish inventory level changed message
                var message = new InventoryLevelChangedMessage
                {
                    ProductId = inventoryItem.ProductId,
                    WarehouseId = inventoryItem.WarehouseId,
                    LocationCode = inventoryItem.LocationCode,
                    NewQuantity = inventoryItem.Quantity,
                    OldQuantity = oldQuantity
                };

                _messageBus.Publish(message, "inventory_events", "inventory.level.changed");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Get product information for notifications
                var product = await _productApiClient.GetProductByIdAsync(inventoryItem.ProductId, cancellationToken);

                var itemDto = _mapper.Map<InventoryItemDto>(inventoryItem);
                if (product != null)
                {
                    itemDto.ProductName = product.Name;
                    itemDto.ProductSku = product.SKU;
                }

                // Send real-time notification via SignalR
                await _notificationService.NotifyInventoryChanged(itemDto);

                // Check for low stock after adjustment
                if (inventoryItem.IsLowStock)
                {
                    // Send low stock alert message
                    var alertMessage = new LowStockAlertMessage
                    {
                        ProductId = inventoryItem.ProductId,
                        ProductName = product?.Name ?? string.Empty,
                        ProductSku = product?.SKU ?? string.Empty,
                        WarehouseId = inventoryItem.WarehouseId,
                        WarehouseName = inventoryItem.Warehouse?.Name ?? string.Empty,
                        LocationCode = inventoryItem.LocationCode,
                        CurrentQuantity = inventoryItem.Quantity,
                        ReorderThreshold = inventoryItem.ReorderThreshold,
                        TargetStockLevel = inventoryItem.TargetStockLevel,
                        SuggestedOrderQuantity = inventoryItem.TargetStockLevel - inventoryItem.Quantity
                    };

                    _messageBus.Publish(alertMessage, "inventory_events", "inventory.low.stock");

                    // Send real-time notification via SignalR
                    await _notificationService.NotifyLowStock(itemDto);
                }

                return itemDto;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<(InventoryItemDto Source, InventoryItemDto Destination)> TransferInventoryAsync(TransferInventoryDto transferInventoryDto, CancellationToken cancellationToken = default)
        {
            if (transferInventoryDto.Quantity <= 0)
                throw new ApplicationException("Transfer quantity must be greater than zero.");

            if (transferInventoryDto.SourceWarehouseId == transferInventoryDto.DestinationWarehouseId &&
                transferInventoryDto.SourceLocationCode == transferInventoryDto.DestinationLocationCode)
                throw new ApplicationException("Source and destination locations must be different.");

            // Check source and destination warehouses
            if (!await _unitOfWork.Warehouses.ExistsByIdAsync(transferInventoryDto.SourceWarehouseId, cancellationToken))
                throw new ApplicationException($"Source warehouse with ID {transferInventoryDto.SourceWarehouseId} does not exist.");

            if (!await _unitOfWork.Warehouses.ExistsByIdAsync(transferInventoryDto.DestinationWarehouseId, cancellationToken))
                throw new ApplicationException($"Destination warehouse with ID {transferInventoryDto.DestinationWarehouseId} does not exist.");

            // Get source inventory item
            var sourceItem = await _unitOfWork.InventoryItems.GetByProductAndWarehouseAsync(
                transferInventoryDto.ProductId,
                transferInventoryDto.SourceWarehouseId,
                transferInventoryDto.SourceLocationCode,
                cancellationToken);

            if (sourceItem == null)
                throw new ApplicationException($"Inventory item for product {transferInventoryDto.ProductId} not found in source warehouse {transferInventoryDto.SourceWarehouseId} at location {transferInventoryDto.SourceLocationCode}.");

            if (sourceItem.Quantity < transferInventoryDto.Quantity)
                throw new ApplicationException($"Insufficient stock. Available: {sourceItem.Quantity}, Requested: {transferInventoryDto.Quantity}");

            // Get or create destination inventory item
            var destinationItem = await _unitOfWork.InventoryItems.GetByProductAndWarehouseAsync(
                transferInventoryDto.ProductId,
                transferInventoryDto.DestinationWarehouseId,
                transferInventoryDto.DestinationLocationCode,
                cancellationToken);

            bool isNewDestination = false;

            if (destinationItem == null)
            {
                // Create new inventory item at destination
                destinationItem = new InventoryItem
                {
                    ProductId = transferInventoryDto.ProductId,
                    WarehouseId = transferInventoryDto.DestinationWarehouseId,
                    LocationCode = transferInventoryDto.DestinationLocationCode,
                    Quantity = 0,
                    ReorderThreshold = sourceItem.ReorderThreshold,
                    TargetStockLevel = sourceItem.TargetStockLevel,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                isNewDestination = true;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update inventory quantities
                sourceItem.RemoveStock(transferInventoryDto.Quantity);

                if (isNewDestination)
                {
                    destinationItem.Quantity = transferInventoryDto.Quantity;
                    destinationItem = await _unitOfWork.InventoryItems.AddAsync(destinationItem, cancellationToken);
                }
                else
                {
                    destinationItem.AddStock(transferInventoryDto.Quantity);
                    await _unitOfWork.InventoryItems.UpdateAsync(destinationItem, cancellationToken);
                }

                await _unitOfWork.InventoryItems.UpdateAsync(sourceItem, cancellationToken);

                // Create transaction record
                var transaction = new InventoryTransaction
                {
                    ProductId = transferInventoryDto.ProductId,
                    WarehouseId = transferInventoryDto.SourceWarehouseId,
                    LocationCode = transferInventoryDto.SourceLocationCode,
                    Type = TransactionType.Transferred,
                    Quantity = transferInventoryDto.Quantity,
                    Timestamp = DateTime.UtcNow,
                    SourceWarehouseId = transferInventoryDto.SourceWarehouseId,
                    DestinationWarehouseId = transferInventoryDto.DestinationWarehouseId,
                    Notes = transferInventoryDto.Notes,
                    CreatedBy = transferInventoryDto.CreatedBy
                };

                await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Get product information for notifications
                var product = await _productApiClient.GetProductByIdAsync(transferInventoryDto.ProductId, cancellationToken);

                var sourceDto = _mapper.Map<InventoryItemDto>(sourceItem);
                var destinationDto = _mapper.Map<InventoryItemDto>(destinationItem);

                if (product != null)
                {
                    sourceDto.ProductName = product.Name;
                    sourceDto.ProductSku = product.SKU;
                    destinationDto.ProductName = product.Name;
                    destinationDto.ProductSku = product.SKU;
                }

                // Send real-time notification via SignalR
                await _notificationService.NotifyInventoryTransferred(sourceDto, destinationDto);

                // Check for low stock after transfer
                if (sourceItem.IsLowStock)
                {
                    // Get warehouse details
                    var sourceWarehouse = await _unitOfWork.Warehouses.GetByIdAsync(sourceItem.WarehouseId, cancellationToken);

                    // Send low stock alert message
                    var alertMessage = new LowStockAlertMessage
                    {
                        ProductId = sourceItem.ProductId,
                        ProductName = product?.Name ?? string.Empty,
                        ProductSku = product?.SKU ?? string.Empty,
                        WarehouseId = sourceItem.WarehouseId,
                        WarehouseName = sourceWarehouse?.Name ?? string.Empty,
                        LocationCode = sourceItem.LocationCode,
                        CurrentQuantity = sourceItem.Quantity,
                        ReorderThreshold = sourceItem.ReorderThreshold,
                        TargetStockLevel = sourceItem.TargetStockLevel,
                        SuggestedOrderQuantity = sourceItem.TargetStockLevel - sourceItem.Quantity
                    };

                    _messageBus.Publish(alertMessage, "inventory_events", "inventory.low.stock");

                    // Send real-time notification via SignalR
                    await _notificationService.NotifyLowStock(sourceDto);
                }

                return (sourceDto, destinationDto);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IEnumerable<StockReservationDto>> ReserveStockAsync(ReserveStockDto reserveStockDto, CancellationToken cancellationToken = default)
        {
            if (reserveStockDto.Items.Count == 0)
                throw new ApplicationException("No items to reserve.");

            var productIds = reserveStockDto.Items.Select(i => i.ProductId).Distinct().ToList();

            // Get product information
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            // Prepare the reservation results
            var reservations = new List<StockReservation>();
            var insufficientItems = new List<InventoryInsufficientMessage.InsufficientItem>();

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var item in reserveStockDto.Items)
                {
                    if (item.Quantity <= 0)
                        throw new ApplicationException($"Quantity for product {item.ProductId} must be greater than zero.");

                    // Get inventory items for this product sorted by warehouse priority
                    var inventoryItems = (await _unitOfWork.InventoryItems.GetByProductIdAsync(item.ProductId, cancellationToken))
                        .Where(i => i.Quantity > 0)
                        .OrderBy(i => i.WarehouseId) // In a real system, could have a more sophisticated prioritization
                        .ToList();

                    int remainingQuantity = item.Quantity;

                    if (!inventoryItems.Any() || inventoryItems.Sum(i => i.Quantity) < remainingQuantity)
                    {
                        // Insufficient stock
                        var insufficientItem = new InventoryInsufficientMessage.InsufficientItem
                        {
                            ProductId = item.ProductId,
                            ProductName = productDict.TryGetValue(item.ProductId, out var product) ? product.Name : string.Empty,
                            ProductSku = productDict.TryGetValue(item.ProductId, out var prodSku) ? prodSku.SKU : string.Empty,
                            RequestedQuantity = item.Quantity,
                            AvailableQuantity = inventoryItems.Sum(i => i.Quantity)
                        };

                        insufficientItems.Add(insufficientItem);
                        continue;
                    }

                    // Reserve from inventory items
                    foreach (var inventoryItem in inventoryItems)
                    {
                        if (remainingQuantity <= 0)
                            break;

                        int quantityToReserve = Math.Min(remainingQuantity, inventoryItem.Quantity);

                        // Create reservation
                        var reservation = new StockReservation
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventoryItem.WarehouseId,
                            LocationCode = inventoryItem.LocationCode,
                            Quantity = quantityToReserve,
                            OrderId = reserveStockDto.OrderId,
                            ReservationDate = DateTime.UtcNow,
                            ExpiryDate = DateTime.UtcNow.Add(reserveStockDto.ReservationDuration),
                            IsActive = true
                        };

                        reservation = await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
                        reservations.Add(reservation);

                        // Update inventory item
                        inventoryItem.RemoveStock(quantityToReserve);
                        await _unitOfWork.InventoryItems.UpdateAsync(inventoryItem, cancellationToken);

                        // Create transaction record
                        var transaction = new InventoryTransaction
                        {
                            ProductId = item.ProductId,
                            WarehouseId = inventoryItem.WarehouseId,
                            LocationCode = inventoryItem.LocationCode,
                            Type = TransactionType.Reserved,
                            Quantity = quantityToReserve,
                            Timestamp = DateTime.UtcNow,
                            ReferenceNumber = reserveStockDto.OrderId.ToString(),
                            Notes = $"Reserved for order {reserveStockDto.OrderId}",
                            CreatedBy = "System"
                        };

                        await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);

                        remainingQuantity -= quantityToReserve;
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // If there are insufficient items, publish message
                if (insufficientItems.Any())
                {
                    var insufficientMessage = new InventoryInsufficientMessage
                    {
                        OrderId = reserveStockDto.OrderId,
                        Items = insufficientItems
                    };

                    _messageBus.Publish(insufficientMessage, "inventory_events", "inventory.insufficient");
                }

                // If there are reservations, publish message
                if (reservations.Any())
                {
                    // Get warehouse details for the reservations
                    var warehouseIds = reservations.Select(r => r.WarehouseId).Distinct().ToList();
                    var warehouses = await Task.WhenAll(warehouseIds.Select(id => _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken)));
                    var warehouseDict = warehouses.Where(w => w != null).ToDictionary(w => w!.Id);

                    var reservedItems = reservations
                        .GroupBy(r => new { r.ProductId, r.WarehouseId })
                        .Select(g => new InventoryReservedMessage.ReservedItem
                        {
                            ProductId = g.Key.ProductId,
                            ProductName = productDict.TryGetValue(g.Key.ProductId, out var product) ? product.Name : string.Empty,
                            ProductSku = productDict.TryGetValue(g.Key.ProductId, out var prodSku) ? prodSku.SKU : string.Empty,
                            Quantity = g.Sum(r => r.Quantity),
                            WarehouseId = g.Key.WarehouseId,
                            WarehouseName = warehouseDict.TryGetValue(g.Key.WarehouseId, out var warehouse) ? warehouse!.Name : string.Empty
                        })
                        .ToList();

                    var reservedMessage = new InventoryReservedMessage
                    {
                        OrderId = reserveStockDto.OrderId,
                        Items = reservedItems
                    };

                    _messageBus.Publish(reservedMessage, "inventory_events", "inventory.reserved");
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // Map to DTOs
                var reservationDtos = _mapper.Map<IEnumerable<StockReservationDto>>(reservations);

                // Enrich with product and warehouse information
                foreach (var dto in reservationDtos)
                {
                    if (productDict.TryGetValue(dto.ProductId, out var product))
                    {
                        dto.ProductName = product.Name;
                        dto.ProductSku = product.SKU;
                    }

                    var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, cancellationToken);
                    if (warehouse != null)
                    {
                        dto.WarehouseName = warehouse.Name;
                    }
                }

                // Send real-time notification via SignalR
                await _notificationService.NotifyStockReserved(reservationDtos);

                return reservationDtos;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task CommitReservationAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var reservations = await _unitOfWork.Reservations.GetByOrderIdAsync(orderId, cancellationToken);
            if (!reservations.Any())
                throw new ApplicationException($"No active reservations found for order {orderId}.");

            var productIds = reservations.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var reservation in reservations)
                {
                    if (!reservation.IsActive)
                        continue;

                    // Release the reservation (mark as inactive)
                    reservation.Release();
                    await _unitOfWork.Reservations.UpdateAsync(reservation, cancellationToken);

                    // Create transaction record for the shipment
                    var transaction = new InventoryTransaction
                    {
                        ProductId = reservation.ProductId,
                        WarehouseId = reservation.WarehouseId,
                        LocationCode = reservation.LocationCode,
                        Type = TransactionType.Shipped,
                        Quantity = reservation.Quantity,
                        Timestamp = DateTime.UtcNow,
                        ReferenceNumber = orderId.ToString(),
                        Notes = $"Shipped for order {orderId}",
                        CreatedBy = "System"
                    };

                    await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task ReleaseReservationAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var reservations = await _unitOfWork.Reservations.GetByOrderIdAsync(orderId, cancellationToken);
            if (!reservations.Any())
                throw new ApplicationException($"No active reservations found for order {orderId}.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                foreach (var reservation in reservations)
                {
                    if (!reservation.IsActive)
                        continue;

                    // Get the inventory item
                    var inventoryItem = await _unitOfWork.InventoryItems.GetByProductAndWarehouseAsync(
                        reservation.ProductId,
                        reservation.WarehouseId,
                        reservation.LocationCode,
                        cancellationToken);

                    if (inventoryItem == null)
                    {
                        // Create a new inventory item if it doesn't exist
                        inventoryItem = new InventoryItem
                        {
                            ProductId = reservation.ProductId,
                            WarehouseId = reservation.WarehouseId,
                            LocationCode = reservation.LocationCode,
                            Quantity = 0,
                            ReorderThreshold = 1,
                            TargetStockLevel = 10,
                            CreatedAt = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow
                        };

                        inventoryItem = await _unitOfWork.InventoryItems.AddAsync(inventoryItem, cancellationToken);
                    }

                    // Add stock back to the inventory
                    inventoryItem.AddStock(reservation.Quantity);
                    await _unitOfWork.InventoryItems.UpdateAsync(inventoryItem, cancellationToken);

                    // Release the reservation
                    reservation.Release();
                    await _unitOfWork.Reservations.UpdateAsync(reservation, cancellationToken);

                    // Create transaction record for the release
                    var transaction = new InventoryTransaction
                    {
                        ProductId = reservation.ProductId,
                        WarehouseId = reservation.WarehouseId,
                        LocationCode = reservation.LocationCode,
                        Type = TransactionType.Released,
                        Quantity = reservation.Quantity,
                        Timestamp = DateTime.UtcNow,
                        ReferenceNumber = orderId.ToString(),
                        Notes = $"Released reservation for order {orderId}",
                        CreatedBy = "System"
                    };

                    await _unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IEnumerable<StockReservationDto>> GetReservationsByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var reservations = await _unitOfWork.Reservations.GetByOrderIdAsync(orderId, cancellationToken);
            var reservationDtos = _mapper.Map<IEnumerable<StockReservationDto>>(reservations);

            if (!reservationDtos.Any())
                return reservationDtos;

            // Enrich with product and warehouse information
            var productIds = reservationDtos.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);
            var productDict = products.ToDictionary(p => p.Id);

            var warehouseIds = reservationDtos.Select(r => r.WarehouseId).Distinct().ToList();
            var warehouses = await Task.WhenAll(warehouseIds.Select(id => _unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken)));
            var warehouseDict = warehouses.Where(w => w != null).ToDictionary(w => w!.Id);

            foreach (var dto in reservationDtos)
            {
                if (productDict.TryGetValue(dto.ProductId, out var product))
                {
                    dto.ProductName = product.Name;
                    dto.ProductSku = product.SKU;
                }

                if (warehouseDict.TryGetValue(dto.WarehouseId, out var warehouse))
                {
                    dto.WarehouseName = warehouse!.Name;
                }
            }

            return reservationDtos;
        }
    }
}
