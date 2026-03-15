using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.MoviePost.Commands;

public sealed class UploadMoviePosterCommandHandler
    : IRequestHandler<UploadMoviePosterCommand, int>
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMoviePosterRepository _posterRepository;

    public UploadMoviePosterCommandHandler(
        IFileStorageService fileStorageService,
        IMoviePosterRepository posterRepository)
    {
        _fileStorageService = fileStorageService;
        _posterRepository = posterRepository;
    }

    public async Task<int> Handle(UploadMoviePosterCommand request, CancellationToken ct)
    {
        await using var stream = request.File.OpenReadStream();

        var objectKey = await _fileStorageService.SaveAsync(
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.MovieId,
            ct);

        var poster = new MoviePoster
        {
            MovieId = request.MovieId,
            ObjectKey = objectKey,
            Order = request.Order
        };

        await _posterRepository.AddAsync(poster, ct);

        return poster.Id;
    }
}
