using Application.Common.Responses;
using Application.Seats.Dtos;
using MediatR;

namespace Application.Seats.Queries;

public sealed record GetSeatByIdQuery(int Id) : IRequest<BaseResponse<GetSeatByIdResponse>>;
