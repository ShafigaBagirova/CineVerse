using Application.Common.Responses;
using Application.Tickets.Dtos;
using Application.Tickets.Queries;
using FluentValidation;
using MediatR;
public sealed class GetMyTicketsQueryValidator : AbstractValidator<GetMyTicketsQuery>
{
    public GetMyTicketsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}