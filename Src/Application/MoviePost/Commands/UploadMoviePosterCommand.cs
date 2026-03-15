using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.MoviePost.Commands;

public sealed record UploadMoviePosterCommand(
    int MovieId,
    IFormFile File,
    int Order
) : IRequest<int>;
