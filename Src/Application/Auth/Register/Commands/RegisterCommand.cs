using Application.Auth.Login.Dtos;
using Application.Auth.Register.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Register.Commands;

public sealed record RegisterCommand(RegisterRequest Request)
    : IRequest<RegisterResponse>;