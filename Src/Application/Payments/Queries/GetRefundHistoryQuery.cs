using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Queries;

public sealed record GetRefundHistoryQuery(GetRefundHistoryRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetRefundHistoryResponse>>>;