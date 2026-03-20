using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;

namespace Application.Follows.Queries;

public sealed record GetSuggestedUsersQuery(
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<SuggestedUserItemDto>>>;
