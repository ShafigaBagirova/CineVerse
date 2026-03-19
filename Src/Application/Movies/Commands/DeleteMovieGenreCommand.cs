using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public sealed record DeleteMovieGenreCommand(int MovieId, int GenreId) : IRequest<BaseResponse>;