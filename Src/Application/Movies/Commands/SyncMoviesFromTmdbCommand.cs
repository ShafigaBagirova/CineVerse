using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public sealed record SyncMoviesFromTmdbCommand(int Page = 1) : IRequest<BaseResponse>;
