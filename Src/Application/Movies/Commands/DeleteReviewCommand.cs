using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public record DeleteReviewCommand(int MovieId)
    : IRequest<BaseResponse>;
