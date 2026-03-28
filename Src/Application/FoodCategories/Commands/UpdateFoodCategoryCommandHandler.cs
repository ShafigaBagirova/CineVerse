using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Commands;

public sealed class UpdateFoodCategoryCommandHandler
    : IRequestHandler<UpdateFoodCategoryCommand, BaseResponse>
{
    private readonly IFoodCategoryRepository _repository;
    private readonly ILogger<UpdateFoodCategoryCommandHandler> _logger;
    private readonly IMapper _mapper;

    public UpdateFoodCategoryCommandHandler(
        IFoodCategoryRepository repository,
        ILogger<UpdateFoodCategoryCommandHandler> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<BaseResponse> Handle(
        UpdateFoodCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Food category not found: {Id}", request.Id);
            return BaseResponse.Fail("Food category not found.");
        }

        _mapper.Map(request.Request, category);

        category.Name = category.Name.Trim();

        await _repository.UpdateAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return BaseResponse.Ok("Food category updated successfully.");
    }
}
