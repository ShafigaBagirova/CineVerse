using Application.Common.Responses;
using MediatR;

namespace Application.Screenings.Commands;


public sealed record DeleteScreeningCommand(int Id) : IRequest<BaseResponse>;
