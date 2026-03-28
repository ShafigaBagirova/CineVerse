namespace Application.Common.Helpers;

public static class FoodOrderCacheKey
{
    public const string ByIdPrefix = "foodorder:";
    public const string AllPrefix = "foodorders:";
    public const string MyOrdersPrefix = "myfoodorders:";
    public const string SummaryPrefix = "foodordersummary:";
    public const string TopSellingPrefix = "foodorders:top-selling:";
    public const string OrdersByDayPrefix = "foodorders:ordersbyday:";
    public static string GetById(int id) => $"{ByIdPrefix}{id}";
}