namespace Application.Halls.Dtos;

public class CreateHallRequest
{
    public string Name { get; set; }= default!;
    public int CinemaId { get; set; }
    public int Capacity { get; set; }
}
