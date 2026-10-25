using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Queries;

public record SearchYouTubeVideosQuery(string Query) : IRequest<BaseResponse<List<SearchYouTubeVideosResponse>>>;
