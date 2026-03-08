using Application.Auth.Login.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Auth.Refresh.Commands;

public sealed record RefreshTokenValidateAndConsumeCommand(string RefreshToken) : IRequest<JwtUserInfoDto>;