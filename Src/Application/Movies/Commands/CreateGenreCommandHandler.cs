using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class CreateGenreCommandHandler
    : IRequestHandler<CreateGenreCommand, BaseResponse>
{
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<CreateGenreCommandHandler> _logger;

    public CreateGenreCommandHandler(
        IGenreRepository genreRepository,
        ILogger<CreateGenreCommandHandler> logger)
    {
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        CreateGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateGenre started. Name: {Name}", request.Request.Name);

        var genre = new Genre
        {
            Name = request.Request.Name
        };

        await _genreRepository.AddAsync(genre, cancellationToken);
        await _genreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("CreateGenre completed. GenreId: {Id}", genre.Id);

        return BaseResponse.Ok("Genre created successfully.");
    }
}