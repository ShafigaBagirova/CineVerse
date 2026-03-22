namespace Domain.Entities;

public class Cinema:BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;

}
