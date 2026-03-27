using MediatR;

namespace Application.Movies.Events;

public sealed record MoviesBulkImportedEvent(int Count) : INotification;
