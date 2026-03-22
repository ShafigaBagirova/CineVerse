using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Queries;

public record GetCinemasByLocationQuery(string? Country, string? City)
    : IRequest<BaseResponse<List<GetAllCinemasResponse>>>;