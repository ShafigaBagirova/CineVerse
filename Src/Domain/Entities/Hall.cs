namespace Domain.Entities;

public class Hall:BaseEntity<int>
{
    public string Name { get; set; } = default!;
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = default!;
    public int Capacity { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
