using Application.Common.Responses;
using MediatR;

namespace Application.Watched.Commands;

public record RemoveFromWatchedCommand(int MovieId)
    : IRequest<BaseResponse>;