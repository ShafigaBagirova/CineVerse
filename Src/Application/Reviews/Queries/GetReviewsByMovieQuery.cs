using Application.Common.Responses;
using Application.Reviews.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Reviews.Queries;

public record GetReviewsByMovieQuery(
    int MovieId,
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<ReviewDto>>>;