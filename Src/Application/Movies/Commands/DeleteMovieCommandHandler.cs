using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public sealed class DeleteMovieCommandHandler
    : IRequestHandler<DeleteMovieCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;

    public DeleteMovieCommandHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<BaseResponse> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie is null)
        {
            return new BaseResponse
            {
                Success = false,
                Message = "Movie could not be found."
            };
        }

        await _movieRepository.DeleteAsync(movie, cancellationToken);
        await _movieRepository.SaveChangesAsync(cancellationToken);

        return new BaseResponse
        {
            Success = true,
            Message = "Movie deleted successfully."
        };
    }
}
