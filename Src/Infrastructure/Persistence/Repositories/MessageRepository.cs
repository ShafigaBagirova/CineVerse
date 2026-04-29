using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MessageRepository : GenericRepository<Message, int>, IMessageRepository
{
    private readonly CineVerseDbContext _context;

    public MessageRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetMessagesByChatIdAsync(int chatId, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .AsNoTracking()
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> MarkAsReadAsync(int chatId, string currentUserId, CancellationToken cancellationToken)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != currentUserId && !m.IsRead)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(m => m.IsRead, true),
                cancellationToken);
    }
}
