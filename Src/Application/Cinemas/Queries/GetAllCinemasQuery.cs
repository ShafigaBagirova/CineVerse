using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Queries;

public record GetAllCinemasQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<GetAllCinemasResponse>>>;