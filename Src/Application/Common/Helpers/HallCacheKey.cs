namespace Application.Common.Helpers;

public class HallCacheKey
{
    public static string All => "halls:all";

    public static string ById(int id) => $"halls:id:{id}";

    public static string ByCinemaId(int cinemaId) => $"halls:cinema:{cinemaId}";
    public static string HallsPaged(
    int pageNumber,
    int pageSize,
    int? cinemaId,
    string? search,
    string? sortBy,
    bool desc)
    {
        var normalizedSearch = search?.Trim().ToLower() ?? string.Empty;
        var normalizedSortBy = sortBy?.Trim().ToLower() ?? string.Empty;

        return $"halls:page:{pageNumber}:size:{pageSize}:cinema:{cinemaId}:search:{normalizedSearch}:sort:{normalizedSortBy}:desc:{desc}";
    }
}
