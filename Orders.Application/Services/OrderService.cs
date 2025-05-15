using AutoMapper;
using Orders.Application.DTOs;
using Orders.Application.Interfaces;
using Orders.Application.Messages;
using Orders.Domain.Entities;
using Orders.Domain.Repositories;

namespace Orders.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductApiClient _productApiClient;
        private readonly IInventoryApiClient _inventoryApiClient;
        private readonly IMessageBus _messageBus;
        private readonly IOrderNotificationService _notificationService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IProductApiClient productApiClient,
            IInventoryApiClient inventoryApiClient,
            IMessageBus messageBus,
            IOrderNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productApiClient = productApiClient;
            _inventoryApiClient = inventoryApiClient;
            _messageBus = messageBus;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.Orders.GetByStatusAsync(status, cancellationToken);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.Orders.GetByCustomerEmailAsync(email, cancellationToken);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.Orders.GetRecentOrdersAsync(count, cancellationToken);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, CancellationToken cancellationToken = default)
        {
            if (createOrderDto.Items.Count == 0)
                throw new ApplicationException("Order must contain at least one item.");

            // Get product details from Products service
            var productIds = createOrderDto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productApiClient.GetProductsByIdsAsync(productIds, cancellationToken);

            // Verify all products exist
            var productDict = products.ToDictionary(p => p.Id);
            foreach (var item in createOrderDto.Items)
            {
                if (!productDict.ContainsKey(item.ProductId))
                    throw new ApplicationException($"Product with ID {item.ProductId} not found.");

                if (item.Quantity <= 0)
                    throw new ApplicationException($"Quantity for product {item.ProductId} must be greater than zero.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create order
                var order = _mapper.Map<Order>(createOrderDto);

                // Create order items with product details
                foreach (var itemDto in createOrderDto.Items)
                {
                    var product = productDict[itemDto.ProductId];
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductSKU = product.SKU,
                        Quantity = itemDto.Quantity,
                        Price = product.Price,
                        IsReserved = false
                    };

                    order.AddItem(orderItem);
                }

                // Add initial status history
                order.StatusHistory.Add(new OrderStatusHistory
                {
                    PreviousStatus = OrderStatus.Created, // Same as initial for first entry
                    NewStatus = OrderStatus.Created,
                    ChangedAt = DateTime.UtcNow,
                    Notes = "Order created"
                });

                // Save to database
                order = await _unitOfWork.Orders.AddAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update order status to PendingInventory
                order.UpdateStatus(OrderStatus.PendingInventory, "Waiting for inventory reservation");
                await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish OrderCreated message
                var message = new OrderCreatedMessage
                {
                    OrderId = order.Id,
                    CustomerEmail = order.CustomerEmail,
                    Items = order.Items.Select(i => new OrderCreatedMessage.OrderItemMessage
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                _messageBus.Publish(message, "order_events", "order.created");

                // Try to reserve inventory
                var reserveRequest = new ReserveStockRequest
                {
                    OrderId = order.Id,
                    Items = order.Items.Select(i => new ReserveStockItemRequest
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var reservationResult = await _inventoryApiClient.ReserveStockAsync(reserveRequest, cancellationToken);

                // Update order based on reservation result
                if (reservationResult.Success)
                {
                    // Mark items as reserved
                    foreach (var item in order.Items)
                    {
                        var reservedItem = reservationResult.ReservedItems.FirstOrDefault(r => r.ProductId == item.ProductId);
                        if (reservedItem != null)
                        {
                            item.IsReserved = true;
                            item.ReservationNotes = $"Reserved from warehouse {reservedItem.WarehouseName}";
                        }
                    }

                    // Update order status
                    order.UpdateStatus(OrderStatus.Reserved, "Inventory successfully reserved");
                }
                else if (reservationResult.UnavailableItems.Any())
                {
                    // Some items couldn't be reserved
                    var unavailableItemsInfo = string.Join(", ",
                        reservationResult.UnavailableItems.Select(u =>
                            $"{u.ProductName} (requested: {u.RequestedQuantity}, available: {u.AvailableQuantity})"));

                    order.UpdateStatus(OrderStatus.PendingInventory, $"Insufficient inventory: {unavailableItemsInfo}");
                }

                await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var orderDto = _mapper.Map<OrderDto>(order);

                // Send real-time notification
                await _notificationService.NotifyOrderCreated(orderDto);

                return orderDto;
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OrderDto> UpdateOrderStatusAsync(UpdateOrderStatusDto updateOrderStatusDto, CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(updateOrderStatusDto.OrderId, cancellationToken);
            if (order == null)
                throw new ApplicationException($"Order with ID {updateOrderStatusDto.OrderId} not found.");

            var previousStatus = order.Status;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update order status
                order.UpdateStatus(updateOrderStatusDto.NewStatus, updateOrderStatusDto.Notes);

                // Handle side effects of status change
                switch (updateOrderStatusDto.NewStatus)
                {
                    case OrderStatus.Processing:
                        // Commit inventory reservations
                        await _inventoryApiClient.CommitReservationAsync(order.Id, cancellationToken);
                        break;

                    case OrderStatus.Cancelled:
                        // Release inventory reservations
                        if (previousStatus == OrderStatus.Reserved || previousStatus == OrderStatus.PendingInventory)
                        {
                            await _inventoryApiClient.ReleaseReservationAsync(order.Id, cancellationToken);
                        }
                        break;
                }

                await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish status change message
                var message = new OrderStatusChangedMessage
                {
                    OrderId = order.Id,
                    PreviousStatus = previousStatus.ToString(),
                    NewStatus = order.Status.ToString()
                };

                _messageBus.Publish(message, "order_events", "order.status.changed");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var orderDto = _mapper.Map<OrderDto>(order);

                // Send real-time notification
                await _notificationService.NotifyOrderStatusChanged(orderDto, previousStatus.ToString());

                return orderDto;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OrderDto> CancelOrderAsync(int id, string? reason = null, CancellationToken cancellationToken = default)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
            if (order == null)
                throw new ApplicationException($"Order with ID {id} not found.");

            if (!order.CanCancel())
                throw new ApplicationException($"Order with ID {id} cannot be cancelled. Current status: {order.Status}");

            var previousStatus = order.Status;

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update order status
                order.UpdateStatus(OrderStatus.Cancelled, reason ?? "Order cancelled by customer");

                // Release inventory reservations if needed
                if (previousStatus == OrderStatus.Reserved || previousStatus == OrderStatus.PendingInventory)
                {
                    await _inventoryApiClient.ReleaseReservationAsync(order.Id, cancellationToken);
                }

                await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Publish cancellation message
                var message = new OrderCancelledMessage
                {
                    OrderId = order.Id,
                    Reason = reason ?? "Customer initiated"
                };

                _messageBus.Publish(message, "order_events", "order.cancelled");

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var orderDto = _mapper.Map<OrderDto>(order);

                // Send real-time notification
                await _notificationService.NotifyOrderCancelled(orderDto);

                return orderDto;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId, CancellationToken cancellationToken = default)
        {
            var statusHistory = await _unitOfWork.OrderStatusHistory.GetByOrderIdAsync(orderId, cancellationToken);
            return _mapper.Map<IEnumerable<OrderStatusHistoryDto>>(statusHistory);
        }
    }
}
