using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Genres.Commands;

public sealed class DeleteGenreCommandHandler
    : IRequestHandler<DeleteGenreCommand, BaseResponse>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeleteGenreCommandHandler> _logger;

    public DeleteGenreCommandHandler(
        IGenreRepository genreRepository,
        ICacheService cacheService,
        ILogger<DeleteGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeleteGenre started. Id: {Id}", request.Id);

        var genre = await _genreRepository.GetByIdAsync(request.Id, cancellationToken);

        if (genre is null)
        {
            _logger.LogWarning("Genre not found. Id: {Id}", request.Id);
            throw new KeyNotFoundException("Genre could not be found.");
        }

        await _genreRepository.DeleteAsync(genre, cancellationToken);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync("genres:all", cancellationToken);

        _logger.LogInformation("DeleteGenre completed. Id: {Id}", request.Id);

        return BaseResponse.Ok("Genre deleted successfully.");
    }
}