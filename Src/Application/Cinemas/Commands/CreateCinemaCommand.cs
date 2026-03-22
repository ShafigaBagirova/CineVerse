using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Commands;

public record CreateCinemaCommand(CreateCinemaRequest Request) : IRequest<BaseResponse>;
