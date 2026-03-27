namespace Application.Common.Interfaces;

public interface IUserIdsProvider
{
    Task<List<string>> GetAllUserIdsAsync(CancellationToken cancellationToken);
}