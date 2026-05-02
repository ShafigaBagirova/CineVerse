using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IWatchListItemRepository _watchListItemRepository;
    private readonly IWatchLogRepository _watchLogRepository;
    private readonly IMapper _mapper;

    public UsersController(
        IWatchListItemRepository watchListItemRepository,
        IWatchLogRepository watchLogRepository,
        IMapper mapper)
    {
        _watchListItemRepository = watchListItemRepository;
        _watchLogRepository = watchLogRepository;
        _mapper = mapper;
    }

    [HttpGet("{userId}/watchlist")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<List<GetAllMoviesResponse>>>> GetUserWatchlist(
        [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var items = await _watchListItemRepository.GetByUserIdAsync(userId, cancellationToken);
        var movies = items
            .Select(x => x.Movie)
            .Where(m => m is not null)
            .Cast<Movie>()
            .ToList();

        var response = _mapper.Map<List<GetAllMoviesResponse>>(movies);
        return Ok(BaseResponse<List<GetAllMoviesResponse>>.Ok(response));
    }

    [HttpGet("{userId}/watched")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<List<GetAllMoviesResponse>>>> GetUserWatched(
        [FromRoute] string userId,
        CancellationToken cancellationToken)
    {
        var items = await _watchLogRepository.GetByUserIdAsync(userId, cancellationToken);
        var movies = items
            .Select(x => x.Movie)
            .Where(m => m is not null)
            .Cast<Movie>()
            .ToList();

        var response = _mapper.Map<List<GetAllMoviesResponse>>(movies);
        return Ok(BaseResponse<List<GetAllMoviesResponse>>.Ok(response));
    }
}
