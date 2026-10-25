using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Queries;

public record GetPopularYouTubeVideosQuery(string? PageToken)
    : IRequest<BaseResponse<GetPopularYouTubeVideosResponse>>;
