using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public record SyncGenresFromTmdbCommand : IRequest<BaseResponse>;