using Application.Common.Responses;
using Application.Screenings.Dtos;
using MediatR;

namespace Application.Screenings.Commands;

public sealed record CreateScreeningCommand(CreateScreeningRequest Request) : IRequest<BaseResponse>;