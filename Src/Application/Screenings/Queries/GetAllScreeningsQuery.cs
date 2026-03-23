using Application.Common.Responses;
using Application.Screenings.Dtos;
using MediatR;

namespace Application.Screenings.Queries;

public sealed record GetAllScreeningsQuery(GetAllScreeningsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllScreeningsResponse>>>;