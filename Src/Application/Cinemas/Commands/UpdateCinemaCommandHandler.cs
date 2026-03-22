using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Commands;

public class UpdateCinemaCommandHandler : IRequestHandler<UpdateCinemaCommand, BaseResponse>
{
    private readonly ICinemaRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateCinemaCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateCinemaCommandHandler(
        ICinemaRepository repository,
        IMapper mapper,
        ILogger<UpdateCinemaCommandHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(UpdateCinemaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UpdateCinemaCommand started for Id: {CinemaId}", request.Id);

        var cinema = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (cinema is null)
        {
            _logger.LogWarning("Cinema not found for update. Id: {CinemaId}", request.Id);
            return BaseResponse.Fail("Cinema tapılmadı.");
        }

        if (!cinema.IsActive)
        {
            _logger.LogWarning("Inactive cinema cannot be updated. Id: {CinemaId}", request.Id);
            return BaseResponse.Fail("Inactive cinema cannot be updated.");
        }

        _mapper.Map(request.Request, cinema);

        await _repository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(CinemaCacheKey.CinemaById(request.Id));
        _logger.LogInformation("Cinema updated successfully. Id: {CinemaId}", request.Id);

        return BaseResponse.Ok("Cinema updated successfully.");
    }
}