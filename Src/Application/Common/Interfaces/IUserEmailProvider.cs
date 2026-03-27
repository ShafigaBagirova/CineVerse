namespace Application.Common.Interfaces;

public interface IUserEmailProvider
{
    Task<string?> GetEmailByUserIdAsync(string userId, CancellationToken cancellationToken);
}