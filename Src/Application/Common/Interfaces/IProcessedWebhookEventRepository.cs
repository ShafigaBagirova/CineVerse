using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces;

public interface IProcessedWebhookEventRepository:IRepository<ProcessedWebhookEvent,int>
{
    Task<bool> ExistsAsync(string eventId, CancellationToken cancellationToken);
}
