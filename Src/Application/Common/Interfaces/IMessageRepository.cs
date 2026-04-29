using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMessageRepository : IRepository<Message, int>
{
    Task<List<Message>> GetMessagesByChatIdAsync(int chatId, CancellationToken cancellationToken);
    Task<int> MarkAsReadAsync(int chatId, string currentUserId, CancellationToken cancellationToken);
}
