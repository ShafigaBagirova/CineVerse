namespace Application.Common.Interfaces;

public interface IUserReadService
{
    Task<string?> GetUserNameAsync(string userId, CancellationToken cancellationToken);
}
