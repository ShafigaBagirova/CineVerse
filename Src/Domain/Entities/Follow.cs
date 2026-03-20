namespace Domain.Entities;

public class Follow:BaseAuditableEntity
{
    public string FollowerId { get; set; }=default!;
    public string FollowingId { get; set; }=default!;
}
