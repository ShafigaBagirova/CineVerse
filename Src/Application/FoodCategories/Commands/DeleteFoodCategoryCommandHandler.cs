using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Commands;

public sealed class DeleteFoodCategoryCommandHandler
    : IRequestHandler<DeleteFoodCategoryCommand, BaseResponse>
{
    private readonly IFoodCategoryRepository _repository;
    private readonly ILogger<DeleteFoodCategoryCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteFoodCategoryCommandHandler(
        IFoodCategoryRepository repository,
        ILogger<DeleteFoodCategoryCommandHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        DeleteFoodCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Food category not found: {Id}", request.Id);
            return BaseResponse.Fail("Food category not found.");
        }

        category.IsActive = false;

        await _repository.UpdateAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"foodcategory:{category.Id}");
        await _cacheService.RemoveByPrefixAsync("foodcategories:");

        return BaseResponse.Ok("Food category deleted successfully.");
    }
}