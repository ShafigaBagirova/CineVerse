namespace Application.Halls.Dtos;

public class GetAllHallsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int CinemaId { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
}
