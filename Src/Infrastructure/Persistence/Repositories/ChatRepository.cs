using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ChatRepository : GenericRepository<Chat, int>, IChatRepository
{
    private readonly CineVerseDbContext _context;

    public ChatRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Chat?> GetPrivateChatByUsersAsync(string userId1, string userId2, CancellationToken cancellationToken)
    {
        return await _context.Chats
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .Where(c => !c.IsGroup)
            .Where(c => c.Participants.Count == 2)
            .Where(c => c.Participants.Any(p => p.UserId == userId1) && c.Participants.Any(p => p.UserId == userId2))
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Chat>> GetChatsByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Chats
            .AsNoTracking()
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.CreatedAt) ?? c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Chat?> GetByIdWithParticipantsAsync(int chatId, CancellationToken cancellationToken)
    {
        return await _context.Chats
            .Include(c => c.Participants)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }
}
