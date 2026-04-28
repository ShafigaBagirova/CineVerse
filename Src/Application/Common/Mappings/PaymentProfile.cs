using Application.Payments.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, CreatePaymentIntentResponse>()
            .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Payment, GetPaymentStatusBySeatHoldIdResponse>()
            .ForMember(dest => dest.HasPayment, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Payment, GetPaymentByIdResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Payment, GetMyPaymentsResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ScreeningId, opt => opt.MapFrom(src => src.SeatHold.ScreeningId))
            .ForMember(dest => dest.SeatId, opt => opt.MapFrom(src => src.SeatHold.SeatId));
        CreateMap<Payment, GetAllPaymentsResponse>()
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.TotalAmount))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.ToString()))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()));
        CreateMap<Payment, GetRefundHistoryResponse>()
    .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.ToString()))
    .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency.ToString()))
    .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.Id));
    }
}
