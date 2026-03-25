using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;

namespace Application.Tickets.Queries;

public sealed record GetOccupiedSeatsByScreeningQuery(int ScreeningId)
    : IRequest<BaseResponse<List<GetOccupiedSeatsByScreeningResponse>>>;