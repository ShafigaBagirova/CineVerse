using Application.Common.Responses;
using Application.Movies.Dtos;
using FluentValidation;
using MediatR;

namespace Application.Validations.Movie;

public class UpdateMovieGenreRequestValidator : AbstractValidator<UpdateMovieGenreCommand>
{
    public UpdateMovieGenreRequestValidator()
    {
        RuleFor(x => x.MovieId)
                 .GreaterThan(0).WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0).WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.Request.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be 0 or greater.");
    }
}