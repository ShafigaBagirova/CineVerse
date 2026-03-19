using Application.Common.Responses;
using MediatR;

namespace Application.WatchListItems.Commands;

public record RemoveFromWatchlistCommand(int MovieId)
    : IRequest<BaseResponse>;