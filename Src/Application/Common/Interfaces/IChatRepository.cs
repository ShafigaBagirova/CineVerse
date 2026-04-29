using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IChatRepository : IRepository<Chat, int>
{
    Task<Chat?> GetPrivateChatByUsersAsync(string userId1, string userId2, CancellationToken cancellationToken);
    Task<List<Chat>> GetChatsByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<Chat?> GetByIdWithParticipantsAsync(int chatId, CancellationToken cancellationToken);
}
