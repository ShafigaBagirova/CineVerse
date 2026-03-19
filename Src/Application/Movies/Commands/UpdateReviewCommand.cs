using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record UpdateReviewCommand(
    int MovieId,
    UpdateReviewRequest Request)
    : IRequest<BaseResponse>;
