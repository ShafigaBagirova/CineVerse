namespace Application.Common.Helpers;

public static class FoodCategoryCacheKey
{
    public const string ByIdPrefix = "foodcategory:";
    public const string AllPrefix = "foodcategories:";
    public const string WithItemsPrefix = "foodcategories:withitems:";

    public static string GetById(int id) => $"{ByIdPrefix}{id}";
}