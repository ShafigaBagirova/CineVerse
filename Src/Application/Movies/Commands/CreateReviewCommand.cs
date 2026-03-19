using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Commands;

public record CreateReviewCommand(
    int MovieId,
    CreateReviewRequest Request)
    : IRequest<BaseResponse>;