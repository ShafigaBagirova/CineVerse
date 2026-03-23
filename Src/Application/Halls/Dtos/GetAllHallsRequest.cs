namespace Application.Halls.Dtos;

public class GetAllHallsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? CinemaId { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
}