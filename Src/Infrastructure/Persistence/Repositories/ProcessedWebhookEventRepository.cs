using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ProcessedWebhookEventRepository:GenericRepository<ProcessedWebhookEvent,int>, IProcessedWebhookEventRepository
{
    private readonly CineVerseDbContext _context;

    public ProcessedWebhookEventRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken)
    {
        return await _context.ProcessedWebhookEvents
            .AnyAsync(x => x.EventId == eventId, cancellationToken);
    }

}
