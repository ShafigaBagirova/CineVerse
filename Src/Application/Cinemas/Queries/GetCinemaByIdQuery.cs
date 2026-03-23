using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Queries;

public record GetCinemaByIdQuery(int Id) : IRequest<BaseResponse<GetCinemaByIdResponse>>;