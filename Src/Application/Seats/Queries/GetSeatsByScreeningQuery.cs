using Application.Common.Responses;
using Application.Seats.Dtos;
using MediatR;

namespace Application.Seats.Queries;

public sealed record GetSeatsByScreeningQuery(int ScreeningId)
    : IRequest<BaseResponse<List<GetSeatsByScreeningResponse>>>;
