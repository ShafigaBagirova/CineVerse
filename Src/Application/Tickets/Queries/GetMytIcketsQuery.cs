using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;

namespace Application.Tickets.Queries;

public sealed record GetMyTicketsQuery(GetMyTicketsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetMyTicketsResponse>>>;