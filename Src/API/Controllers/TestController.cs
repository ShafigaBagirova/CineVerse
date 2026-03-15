using Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IMovieProvider _movieProvider;

    public TestController(IMovieProvider movieProvider)
    {
        _movieProvider = movieProvider;
    }
    [HttpGet("tmdb-now-playing")]
    public async Task<IActionResult> TestNowPlaying()
    {
        var movies = await _movieProvider.GetNowPlayingAsync();
        return Ok(movies);
    }
}
