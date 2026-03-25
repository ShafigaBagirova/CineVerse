using Application.Tickets.Queries;
using FluentValidation;

namespace Application.Validations.Tickets;

public sealed class GetTicketByIdQueryValidator : AbstractValidator<GetTicketByIdQuery>
{
    public GetTicketByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ticket id must be greater than 0.");
    }
}