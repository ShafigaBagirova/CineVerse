using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class DeleteFoodItemCommandHandler
    : IRequestHandler<DeleteFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ILogger<DeleteFoodItemCommandHandler> _logger;

    public DeleteFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        ILogger<DeleteFoodItemCommandHandler> logger)
    {
        _foodItemRepository = foodItemRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteFoodItemCommand started. FoodItemId: {FoodItemId}",
            request.Id);

        var item = await _foodItemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null)
        {
            _logger.LogWarning(
                "DeleteFoodItemCommand failed. Food item not found. FoodItemId: {FoodItemId}",
                request.Id);

            return BaseResponse.Fail("Food item not found.");
        }

        item.IsActive = false;

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item deleted successfully.");
    }
}