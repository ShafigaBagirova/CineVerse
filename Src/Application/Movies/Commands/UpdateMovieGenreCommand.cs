using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record UpdateMovieGenreCommand(int MovieId, int GenreId, UpdateMovieGenreRequest Request) : IRequest<BaseResponse>;