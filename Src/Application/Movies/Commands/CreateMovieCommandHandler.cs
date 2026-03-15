using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Movies.Commands;

public sealed class CreateMovieCommandHandler
    : IRequestHandler<CreateMovieCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public CreateMovieCommandHandler(IMovieRepository movieRepository, IMapper mapper)
    {
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task<BaseResponse> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var dto = request.CreateMovieRequest;

        var title = dto.Title.Trim();
        var description = dto.Description.Trim();
        var country = dto.Country.Trim();
        var ageRating = dto.AgeRating.Trim();
        var tagline = dto.Tagline.Trim();
        var director = dto.Director.Trim();
        var language = dto.Language.Trim();

        var tmdbExists = await _movieRepository.ExistsByTmdbIdAsync(dto.TmdbId, cancellationToken);
        if (tmdbExists)
        {  
            return new BaseResponse
            {
                Success = false,
                Message = "This TMDB Id already exists."
            };
            
        }

        var duplicateMovieExists = await _movieRepository
            .ExistsByTitleAndReleaseDateAsync(title, dto.ReleaseDate, cancellationToken);

        if (duplicateMovieExists)
        {
            return new BaseResponse
            {
                Success = false,
                Message = "Movie with this Title and RelaseDate already exists."
            };
          
        }

        var slug = await GenerateUniqueSlugAsync(title, cancellationToken);

        var movie = _mapper.Map<Movie>(dto);

        movie.Title = title;
        movie.Description = description;
        movie.Country = country;
        movie.AgeRating = ageRating;
        movie.Tagline = tagline;
        movie.Director = director;
        movie.Language = language;

        movie.Slug = slug;
        movie.Status = MovieStatus.Upcoming;
        movie.RatingCount = 0;

        await _movieRepository.AddAsync(movie, cancellationToken);
        await _movieRepository.SaveChangesAsync(cancellationToken);
        
        return new BaseResponse
        {
            Success = true,
            Message = "Movie created successfully."
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(title);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "movie";

        var slug = baseSlug;
        var counter = 1;

        while (await _movieRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}

