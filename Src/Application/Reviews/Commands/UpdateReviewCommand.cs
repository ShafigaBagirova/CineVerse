using Application.Common.Responses;
using Application.Reviews.Dtos;
using MediatR;

namespace Application.Reviews.Commands;

public record UpdateReviewCommand(
    int MovieId,
    UpdateReviewRequest Request)
    : IRequest<BaseResponse>;
