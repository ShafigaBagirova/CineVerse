using MediatR;

namespace Application.Movies.Events;

public sealed record MovieCreatedEvent(int MovieId, string Title) : INotification;