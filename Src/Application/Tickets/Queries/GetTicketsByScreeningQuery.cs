using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;

namespace Application.Tickets.Queries;

public sealed record GetTicketsByScreeningQuery(int ScreeningId,GetTicketsByScreeningRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetTicketsByScreeningResponse>>>;