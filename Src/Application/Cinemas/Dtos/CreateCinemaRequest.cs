namespace Application.Cinemas.Dtos;

public class CreateCinemaRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
