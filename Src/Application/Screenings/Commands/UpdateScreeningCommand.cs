using Application.Common.Responses;
using Application.Screenings.Dtos;
using MediatR;

namespace Application.Screenings.Commands;

public sealed record UpdateScreeningCommand(int Id, UpdateScreeningRequest Request)
    : IRequest<BaseResponse>;
