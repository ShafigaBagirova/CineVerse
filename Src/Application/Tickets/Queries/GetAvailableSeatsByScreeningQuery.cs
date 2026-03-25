using Application.Common.Responses;
using Application.Tickets.Dtos;
using MediatR;

namespace Application.Tickets.Queries;

public sealed record GetAvailableSeatsByScreeningQuery(int ScreeningId)
    : IRequest<BaseResponse<List<GetAvailableSeatsByScreeningResponse>>>;