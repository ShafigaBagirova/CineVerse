using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateMovieCommandHandler> _logger;

    public UpdateMovieCommandHandler(
        IMovieRepository movieRepository,
        IMapper mapper,
        ILogger<UpdateMovieCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateMovieCommand started for MovieId {MovieId}", request.Id);

        var dto = request.UpdateMovieRequest;

        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning("UpdateMovieCommand failed. Movie with Id {MovieId} not found.", request.Id);

            return new BaseResponse
            {
                Success = false,
                Message = "Movie could not be found."
            };
        }

        if (!string.IsNullOrWhiteSpace(dto.Title))
        {
            var normalizedTitle = dto.Title.Trim();

            var duplicateExists = await _movieRepository
                .ExistsByTitleAndReleaseDateAsync(normalizedTitle, dto.ReleaseDate ?? movie.ReleaseDate, cancellationToken);

            if (duplicateExists &&
                !(movie.Title == normalizedTitle && movie.ReleaseDate == (dto.ReleaseDate ?? movie.ReleaseDate)))
            {
                _logger.LogWarning(
                    "UpdateMovieCommand failed. Movie with Title {Title} and ReleaseDate {ReleaseDate} already exists.",
                    normalizedTitle,
                    dto.ReleaseDate ?? movie.ReleaseDate);

                return new BaseResponse
                {
                    Success = false,
                    Message = "Movie with this Title and RelaseDate already exists."
                };
            }

            movie.Title = normalizedTitle;
            movie.Slug = await GenerateUniqueSlugForUpdateAsync(movie.Id, normalizedTitle, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(dto.Description))
            movie.Description = dto.Description.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Country))
            movie.Country = dto.Country.Trim();

        if (!string.IsNullOrWhiteSpace(dto.AgeRating))
            movie.AgeRating = dto.AgeRating.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Tagline))
            movie.Tagline = dto.Tagline.Trim();

        if (dto.ReleaseDate.HasValue)
            movie.ReleaseDate = dto.ReleaseDate.Value;

        if (!string.IsNullOrWhiteSpace(dto.Director))
            movie.Director = dto.Director.Trim();

        if (dto.DurationMinutes.HasValue)
            movie.DurationMinutes = dto.DurationMinutes.Value;

        if (!string.IsNullOrWhiteSpace(dto.Language))
            movie.Language = dto.Language.Trim();

        if (dto.Status.HasValue)
            movie.Status = dto.Status.Value;

        await _movieRepository.UpdateAsync(movie, cancellationToken);
        await _movieRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Movie with Id {MovieId} updated successfully.", request.Id);

        return new BaseResponse
        {
            Success = true,
            Message = "Movie craeted successfully."
        };
    }

    private async Task<string> GenerateUniqueSlugForUpdateAsync(int movieId, string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(title);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "movie";

        var slug = baseSlug;
        var counter = 1;

        while (await _movieRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            var existingMovie = await _movieRepository.GetBySlugAsync(slug, cancellationToken);

            if (existingMovie is not null && existingMovie.Id == movieId)
                return slug;

            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
