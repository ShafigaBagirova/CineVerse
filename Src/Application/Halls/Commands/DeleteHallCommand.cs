using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Halls.Commands;

public record DeleteHallCommand(int Id) : IRequest<BaseResponse>;