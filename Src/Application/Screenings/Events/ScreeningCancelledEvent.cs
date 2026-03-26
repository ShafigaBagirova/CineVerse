using MediatR;

namespace Application.Screenings.Events;

public sealed record ScreeningCancelledEvent(int ScreeningId) : INotification;
