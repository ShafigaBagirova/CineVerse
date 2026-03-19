using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Movies.Queries;

public record GetReviewsByMovieQuery(
    int MovieId,
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<ReviewDto>>>;