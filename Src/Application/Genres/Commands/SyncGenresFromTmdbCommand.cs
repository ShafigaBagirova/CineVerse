using Application.Common.Responses;
using MediatR;

namespace Application.Genres.Commands;

public record SyncGenresFromTmdbCommand : IRequest<BaseResponse>;