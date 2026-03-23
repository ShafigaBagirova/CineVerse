using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;

namespace Application.Halls.Queries;

public record GetHallByIdQuery(int Id) : IRequest<BaseResponse<GetHallByIdResponse>>;
