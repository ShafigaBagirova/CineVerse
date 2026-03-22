namespace Application.Common.Helpers;

public static class CinemaCacheKey
{
    public static string CinemasPaged(int pageNumber, int pageSize)
    => $"cinemas:page:{pageNumber}:size:{pageSize}";
    public const string CinemasAllPrefix = "cinemas:";

    public static string CinemaById(int id) => $"cinemas:{id}";
  
}
