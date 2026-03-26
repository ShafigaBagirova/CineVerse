using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Queries;

public sealed record GetMyPaymentsQuery(GetMyPaymentsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetMyPaymentsResponse>>>;