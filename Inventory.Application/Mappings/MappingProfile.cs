using AutoMapper;
using Inventory.Application.DTOs.InventoryItem;
using Inventory.Application.DTOs.InventoryTransaction;
using Inventory.Application.DTOs.StockReservation;
using Inventory.Application.DTOs.Warehouse;
using Inventory.Domain.Entities;

namespace Inventory.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // InventoryItem mappings
            CreateMap<InventoryItem, InventoryItemDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.ProductSku, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock));

            CreateMap<CreateInventoryItemDto, InventoryItem>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Warehouse, opt => opt.Ignore());

            // Warehouse mappings
            CreateMap<Warehouse, WarehouseDto>();
            CreateMap<CreateWarehouseDto, Warehouse>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                .ForMember(dest => dest.InventoryItems, opt => opt.Ignore())
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());

            // InventoryTransaction mappings
            CreateMap<InventoryTransaction, InventoryTransactionDto>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.ProductSku, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.SourceWarehouseName, opt => opt.Ignore()) // Will be populated manually
                .ForMember(dest => dest.DestinationWarehouseName, opt => opt.Ignore()); // Will be populated manually

            // StockReservation mappings
            CreateMap<StockReservation, StockReservationDto>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.ProductSku, opt => opt.Ignore()) // Will be populated from Products API
                .ForMember(dest => dest.WarehouseName, opt => opt.Ignore()) // Will be populated manually
                .ForMember(dest => dest.IsExpired, opt => opt.MapFrom(src => src.IsExpired));
        }
    }
}
