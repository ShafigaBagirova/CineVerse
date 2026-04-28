using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cinemas.Commands;

public class CreateCinemaCommandHandler
    : IRequestHandler<CreateCinemaCommand, BaseResponse>
{
    private readonly ICinemaRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateCinemaCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateCinemaCommandHandler(
        ICinemaRepository repository,
        IMapper mapper,
        ILogger<CreateCinemaCommandHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(CreateCinemaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateCinemaCommand started");

        var dto = request.Request;

        var cinema = _mapper.Map<Cinema>(dto);

        await _repository.AddAsync(cinema, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CinemaCacheKey.CinemasPagedPrefix);

        _logger.LogInformation("Cinema created successfully: {CinemaName}", cinema.Name);

        return BaseResponse.Ok("Cinema successfully created");
    }
}
