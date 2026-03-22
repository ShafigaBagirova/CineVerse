using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Commands;

public record DeleteCinemaCommand(int Id) : IRequest<BaseResponse>;
