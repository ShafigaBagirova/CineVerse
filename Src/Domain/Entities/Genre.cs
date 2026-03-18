using Domain.Enums;

namespace Domain.Entities;

public class Genre:BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public int TmdbGenreId { get; set; }

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}
