using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Genres.Commands;

public sealed class UpdateGenreCommandHandler
    : IRequestHandler<UpdateGenreCommand, BaseResponse>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<UpdateGenreCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateGenreCommandHandler(
        IGenreRepository genreRepository,
        ILogger<UpdateGenreCommandHandler> logger,
        ICacheService cacheService)
    {
        _genreRepository = genreRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        UpdateGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateGenre started. Id: {Id}", request.Id);

        var genre = await _genreRepository.GetByIdAsync(request.Id, cancellationToken);

        if (genre is null)
        {
            _logger.LogWarning("Genre not found. Id: {Id}", request.Id);
            throw new KeyNotFoundException("Genre could not be found.");
        }
        var name = request.Request.Name.Trim();
        var existingGenre = await _genreRepository.GetByNameAsync(name, cancellationToken);

        if (existingGenre is not null && existingGenre.Id != request.Id)
            return BaseResponse.Fail("Genre with this name already exists.");
        genre.Name = request.Request.Name;

        await _genreRepository.UpdateAsync(genre, cancellationToken);
        await _genreRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync("genres:all", cancellationToken);
        _logger.LogInformation("UpdateGenre completed. Id: {Id}", request.Id);

        return BaseResponse.Ok("Genre updated successfully.");
    }
}