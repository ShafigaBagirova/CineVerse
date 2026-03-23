using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Commands;

public record UpdateCinemaCommand(int Id, UpdateCinemaRequest Request) : IRequest<BaseResponse>;