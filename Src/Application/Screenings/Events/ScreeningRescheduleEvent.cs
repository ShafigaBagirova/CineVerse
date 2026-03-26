using MediatR;

namespace Application.Screenings.Events;

public sealed record ScreeningRescheduledEvent(int ScreeningId) : INotification;