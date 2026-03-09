using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Movies.Commands;

public sealed record UploadMoviePosterCommand(
    int MovieId,
    IFormFile File,
    int Order
) : IRequest<int>;
