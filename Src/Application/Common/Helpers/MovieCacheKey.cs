using Domain.Enums;

namespace Application.Common.Helpers;

public static class CacheKeys
{
    public static string MoviesPaged(int pageNumber,int pageSize,string? search,int? genreId,string? language,MovieStatus? status,int? year, decimal? minTmdbRating,
        decimal? maxTmdbRating,
        decimal? minUserRating,
        decimal? maxUserRating,
        string? sortBy,
        bool desc)
    {
        var normalizedSearch = search?.Trim().ToLower() ?? string.Empty;
        var normalizedLanguage = language?.Trim().ToLower() ?? string.Empty;
        var normalizedStatus = status?.ToString().ToLower() ?? string.Empty;
        var normalizedSortBy = sortBy?.Trim().ToLower() ?? string.Empty;

        return $"movies:page:{pageNumber}:size:{pageSize}:search:{normalizedSearch}:genre:{genreId}:language:{normalizedLanguage}:status:{normalizedStatus}:year:{year}:minTmdb:{minTmdbRating}:maxTmdb:{maxTmdbRating}:minUser:{minUserRating}:maxUser:{maxUserRating}:sort:{normalizedSortBy}:desc:{desc}";
    }

    public static string MovieById(int id) => $"movies:{id}";
    public static string MovieBySlug(string slug) => $"movies:slug:{slug.Trim().ToLower()}";
}
