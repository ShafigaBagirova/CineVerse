using Application.Common.Responses;
using MediatR;

namespace Application.Reviews.Commands;

public record DeleteReviewCommand(int MovieId)
    : IRequest<BaseResponse>;
