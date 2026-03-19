namespace Application.Movies.Dtos;

public class GetMovieRatingSummaryResponse
{
    public decimal? UserAverageRating { get; set; }
    public int RatingCount { get; set; }
    public decimal? MyRating { get; set; }
}
