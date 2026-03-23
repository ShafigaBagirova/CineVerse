using Application.Common.Responses;
using Application.Screenings.Dtos;
using MediatR;

namespace Application.Screenings.Queries;

public sealed record GetScreeningByIdQuery(int Id)
    : IRequest<BaseResponse<GetScreeningByIdResponse>>;