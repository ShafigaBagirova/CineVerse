using Application.Common.Responses;
using MediatR;

namespace Application.WatchListItems.Commands;

public record AddToWatchlistCommand(int MovieId)
    : IRequest<BaseResponse>;