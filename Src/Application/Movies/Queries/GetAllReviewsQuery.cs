using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public record GetAllReviewsQuery()
    : IRequest<BaseResponse<List<ReviewDto>>>;