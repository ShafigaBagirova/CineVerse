using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Genres.Commands;

public sealed class CreateGenreCommandHandler
    : IRequestHandler<CreateGenreCommand, BaseResponse>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<CreateGenreCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateGenreCommandHandler(
        IGenreRepository genreRepository,
        ILogger<CreateGenreCommandHandler> logger,
        ICacheService cacheService)
    {
        _genreRepository = genreRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        CreateGenreCommand request,
        CancellationToken cancellationToken)
    {
        var name = request.Request.Name.Trim();
        _logger.LogInformation("CreateGenre started. Name: {Name}", name);

        var existing = await _genreRepository.GetByNameAsync(name, cancellationToken);
        if (existing is not null)
            return BaseResponse.Fail("Genre with this name already exists.");

        var genre = new Genre
        {
            Name = name,
            TmdbGenreId = -Random.Shared.Next(1, int.MaxValue)
        };

        await _genreRepository.AddAsync(genre, cancellationToken);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync("genres:all", cancellationToken);

        _logger.LogInformation("CreateGenre completed. GenreId: {Id}", genre.Id);

        return BaseResponse.Ok("Genre created successfully.");
    }
}