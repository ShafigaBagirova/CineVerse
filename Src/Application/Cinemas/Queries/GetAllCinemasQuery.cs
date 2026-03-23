using Application.Cinemas.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Cinemas.Queries;

public sealed record GetAllCinemasQuery(GetAllCinemasRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllCinemasResponse>>>;