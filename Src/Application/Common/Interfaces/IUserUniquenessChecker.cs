namespace Application.Common.Interfaces;

public interface IUserUniquenessChecker
{
    Task<bool> IsUserNameTakenAsync(string userName, CancellationToken ct);
    Task<bool> IsEmailTakenAsync(string email, CancellationToken ct);
}
