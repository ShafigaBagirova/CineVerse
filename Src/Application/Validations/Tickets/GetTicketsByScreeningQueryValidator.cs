using Application.Tickets.Queries;
using FluentValidation;

namespace Application.Validations.Tickets;

public sealed class GetTicketsByScreeningQueryValidator
    : AbstractValidator<GetTicketsByScreeningQuery>
{
    public GetTicketsByScreeningQueryValidator()
    {
        RuleFor(x => x.ScreeningId)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");

        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}