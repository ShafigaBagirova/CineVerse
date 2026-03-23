namespace Application.Cinemas.Dtos;

public class GetAllCinemasResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string? Phone { get; set; }
    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
}