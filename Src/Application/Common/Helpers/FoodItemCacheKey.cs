namespace Application.Common.Helpers;

public static class FoodItemCacheKey
{
    public const string ByIdPrefix = "fooditem:";
    public const string AllPrefix = "fooditems:";

    public static string GetById(int id) => $"{ByIdPrefix}{id}";
}