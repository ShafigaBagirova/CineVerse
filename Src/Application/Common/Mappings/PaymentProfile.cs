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
    }
}
