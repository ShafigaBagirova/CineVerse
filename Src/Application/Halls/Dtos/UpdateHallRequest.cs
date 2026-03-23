namespace Application.Halls.Dtos;

public class UpdateHallRequest
{
    public string? Name { get; set; }
    public int? CinemaId { get; set; }
    public int? Capacity { get; set; }
}