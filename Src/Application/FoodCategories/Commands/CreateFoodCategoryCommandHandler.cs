using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Commands;

public sealed class CreateFoodCategoryCommandHandler
    : IRequestHandler<CreateFoodCategoryCommand, BaseResponse>
{
    private readonly IFoodCategoryRepository _repository;
    private readonly ILogger<CreateFoodCategoryCommandHandler> _logger;
    private readonly IMapper _mapper;

    public CreateFoodCategoryCommandHandler(
        IFoodCategoryRepository repository,
        ILogger<CreateFoodCategoryCommandHandler> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseResponse> Handle(
        CreateFoodCategoryCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating food category: {Name}", request.Request.Name);

        var entity = _mapper.Map<FoodCategory>(request.Request);

        entity.Name = entity.Name.Trim();
        entity.IsActive = true;

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return BaseResponse.Ok("Food category created successfully.");
    }
}