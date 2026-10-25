using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Queries;

public class SearchYouTubeVideosQueryHandler
    : IRequestHandler<SearchYouTubeVideosQuery, BaseResponse<List<SearchYouTubeVideosResponse>>>
{
    private readonly IYouTubeSearchService _youTubeSearchService;
    private readonly ILogger<SearchYouTubeVideosQueryHandler> _logger;

    public SearchYouTubeVideosQueryHandler(
        IYouTubeSearchService youTubeSearchService,
        ILogger<SearchYouTubeVideosQueryHandler> logger)
    {
        _youTubeSearchService = youTubeSearchService;
        _logger = logger;
    }

    public async Task<BaseResponse<List<SearchYouTubeVideosResponse>>> Handle(
        SearchYouTubeVideosQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SearchYouTubeVideosQuery started. Query: {Query}",
            request.Query);

        if (!_youTubeSearchService.IsConfigured)
        {
            _logger.LogWarning("SearchYouTubeVideosQuery failed. YouTube ApiKey is not configured.");
            return BaseResponse<List<SearchYouTubeVideosResponse>>.Fail("YouTube search is not configured.");
        }

        try
        {
            var results = await _youTubeSearchService.SearchAsync(request.Query.Trim(), cancellationToken);

            _logger.LogInformation(
                "SearchYouTubeVideosQuery completed successfully. ReturnedCount: {ReturnedCount}",
                results.Count);

            return BaseResponse<List<SearchYouTubeVideosResponse>>.Ok(results, "Videos fetched successfully.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // Google answers 403 when the daily quota is used up or the key is restricted away from this server.
            _logger.LogWarning(ex, "SearchYouTubeVideosQuery failed. YouTube refused the request (quota or key restrictions).");
            return BaseResponse<List<SearchYouTubeVideosResponse>>.Fail(
                "YouTube search limit reached for now. Paste a video link instead or try again later.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "SearchYouTubeVideosQuery failed. YouTube request error.");
            return BaseResponse<List<SearchYouTubeVideosResponse>>.Fail("YouTube search is unavailable right now.");
        }
    }
}
