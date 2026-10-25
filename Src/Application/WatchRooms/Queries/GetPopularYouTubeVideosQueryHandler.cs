using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Queries;

public class GetPopularYouTubeVideosQueryHandler
    : IRequestHandler<GetPopularYouTubeVideosQuery, BaseResponse<GetPopularYouTubeVideosResponse>>
{
    private readonly IYouTubeSearchService _youTubeSearchService;
    private readonly ILogger<GetPopularYouTubeVideosQueryHandler> _logger;

    public GetPopularYouTubeVideosQueryHandler(
        IYouTubeSearchService youTubeSearchService,
        ILogger<GetPopularYouTubeVideosQueryHandler> logger)
    {
        _youTubeSearchService = youTubeSearchService;
        _logger = logger;
    }

    public async Task<BaseResponse<GetPopularYouTubeVideosResponse>> Handle(
        GetPopularYouTubeVideosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPopularYouTubeVideosQuery started.");

        if (!_youTubeSearchService.IsConfigured)
        {
            _logger.LogWarning("GetPopularYouTubeVideosQuery failed. YouTube ApiKey is not configured.");
            return BaseResponse<GetPopularYouTubeVideosResponse>.Fail("YouTube search is not configured.");
        }

        try
        {
            var results = await _youTubeSearchService.GetPopularAsync(request.PageToken, cancellationToken);

            _logger.LogInformation(
                "GetPopularYouTubeVideosQuery completed successfully. ReturnedCount: {ReturnedCount}, HasNextPage: {HasNextPage}",
                results.Items.Count,
                results.NextPageToken is not null);

            return BaseResponse<GetPopularYouTubeVideosResponse>.Ok(results, "Videos fetched successfully.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _logger.LogWarning(ex, "GetPopularYouTubeVideosQuery failed. YouTube refused the request (quota or key restrictions).");
            return BaseResponse<GetPopularYouTubeVideosResponse>.Fail(
                "YouTube search limit reached for now. Paste a video link instead or try again later.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "GetPopularYouTubeVideosQuery failed. YouTube request error.");
            return BaseResponse<GetPopularYouTubeVideosResponse>.Fail("YouTube is unavailable right now.");
        }
    }
}
