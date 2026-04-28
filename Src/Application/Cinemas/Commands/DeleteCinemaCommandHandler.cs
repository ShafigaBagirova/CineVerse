using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Commands;

public class DeleteCinemaCommandHandler : IRequestHandler<DeleteCinemaCommand, BaseResponse>
{
    private readonly ICinemaRepository _repository;
    private readonly ILogger<DeleteCinemaCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteCinemaCommandHandler(
        ICinemaRepository repository,
        ILogger<DeleteCinemaCommandHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(DeleteCinemaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeleteCinemaCommand started for Id: {CinemaId}", request.Id);

        var cinema = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (cinema is null)
        {
            _logger.LogWarning("Cinema not found. Id: {CinemaId}", request.Id);
            return BaseResponse.Fail("Cinema not found.");
        }

        if (!cinema.IsActive)
        {
            _logger.LogWarning("Cinema already inactive. Id: {CinemaId}", request.Id);
            return BaseResponse.Fail("Cinema already inactive.");
        }

        cinema.IsActive = false;

        await _repository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CinemaCacheKey.CinemaById(request.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CinemaCacheKey.CinemasPagedPrefix);

        _logger.LogInformation("Cinema deleted successfully. Id: {CinemaId}", request.Id);

        return BaseResponse.Ok("Cinema deleted successfully.");
    }
}