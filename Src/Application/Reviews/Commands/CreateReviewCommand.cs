using Application.Common.Responses;
using Application.Reviews.Dtos;
using MediatR;

namespace Application.Reviews.Commands;

public record CreateReviewCommand(
    int MovieId,
    CreateReviewRequest Request)
    : IRequest<BaseResponse>;