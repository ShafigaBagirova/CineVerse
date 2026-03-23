namespace Application.Common.Helpers;

public static class CinemaCacheKey
{
    public static string CinemasPaged(int pageNumber,int pageSize,string? country,string? city, string? search,
     string? sortBy,
     bool desc)
    {
        var normalizedCountry = country?.Trim().ToLower() ?? string.Empty;
        var normalizedCity = city?.Trim().ToLower() ?? string.Empty;
        var normalizedSearch = search?.Trim().ToLower() ?? string.Empty;
        var normalizedSortBy = sortBy?.Trim().ToLower() ?? string.Empty;

        return $"cinemas:page:{pageNumber}:size:{pageSize}:country:{normalizedCountry}:city:{normalizedCity}:search:{normalizedSearch}:sort:{normalizedSortBy}:desc:{desc}";
    }

    public static string CinemaById(int id) => $"cinemas:{id}";

}
