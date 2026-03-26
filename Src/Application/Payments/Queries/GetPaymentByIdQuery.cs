using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Queries;

public sealed record GetPaymentByIdQuery(int Id)
    : IRequest<BaseResponse<GetPaymentByIdResponse>>;