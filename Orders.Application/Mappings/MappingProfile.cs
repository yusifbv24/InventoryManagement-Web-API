using AutoMapper;
using Orders.Application.DTOs;
using Orders.Domain.Entities;

namespace Orders.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CanCancel, opt => opt.MapFrom(src => src.CanCancel()))
                .ForMember(dest => dest.CanUpdateItems, opt => opt.MapFrom(src => src.CanUpdateItems()));

            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => OrderStatus.Created))
                .ForMember(dest => dest.Items, opt => opt.Ignore())
                .ForMember(dest => dest.StatusHistory, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

            // OrderItem mappings
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Price * src.Quantity));

            CreateMap<CreateOrderItemDto, OrderItem>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductSKU, opt => opt.Ignore())
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ForMember(dest => dest.Order, opt => opt.Ignore())
                .ForMember(dest => dest.IsReserved, opt => opt.MapFrom(_ => false))
                .ForMember(dest => dest.ReservationNotes, opt => opt.Ignore());

            // OrderStatusHistory mappings
            CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
                .ForMember(dest => dest.PreviousStatus, opt => opt.MapFrom(src => src.PreviousStatus.ToString()))
                .ForMember(dest => dest.NewStatus, opt => opt.MapFrom(src => src.NewStatus.ToString()));
        }
    }
}
