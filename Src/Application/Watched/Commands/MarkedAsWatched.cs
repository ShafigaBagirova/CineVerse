using Application.Common.Responses;
using MediatR;

namespace Application.Watched.Commands;

public record MarkAsWatchedCommand(int MovieId)
    : IRequest<BaseResponse>;
