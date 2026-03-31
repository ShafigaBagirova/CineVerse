using Application.Auth.Login.Commands;
using Application.Auth.Login.Dtos;
using Application.Auth.Refresh.Commands;
using Application.Auth.Refresh.Dtos;
using Application.Auth.Register.Commands;
using Application.Auth.Register.Dtos;
using Application.Auth.User.Commands;
using Application.Auth.User.Queries;
using Application.Common.Responses;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<RegisterResponse>>> Register(
           [FromBody] RegisterRequest request,
           CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterCommand(request), ct);

        if (result is null)
            return BadRequest(BaseResponse<RegisterResponse>.Fail("Registration failed."));

        return Ok(BaseResponse<RegisterResponse>.Ok(result, "Registration completed successfully."));
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<TokenResponse>>> Login(
         [FromBody] LoginRequest request,
         CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request), ct);

        if (result is null)
            return Unauthorized(BaseResponse<TokenResponse>.Fail("Invalid login or password."));

        return Ok(BaseResponse<TokenResponse>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<TokenResponse>>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(BaseResponse<TokenResponse>.Fail("Refresh token is required."));

        var result = await _mediator.Send(new RefreshCommand(request.RefreshToken), ct);

        if (result is null)
            return Unauthorized(BaseResponse<TokenResponse>.Fail("Invalid or expired refresh token."));

        return Ok(BaseResponse<TokenResponse>.Ok(result, "Token refreshed successfully."));
    }

    [HttpPost("confirm-registration")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse>> ConfirmRegistration(
        [FromBody] ConfirmRegistrationCodeRequest request,
        CancellationToken ct)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(BaseResponse.Fail("Email and code are required."));
        }

        var result = await _mediator.Send(
            new ConfirmRegisterCommand(request.Email, request.Code), ct);

        if (!result)
            return BadRequest(BaseResponse.Fail("Invalid or expired verification code."));

        return Ok(BaseResponse.Ok("Email confirmed successfully."));
    }

    [HttpGet("me")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse<JwtUserInfoDto>>> GetCurrentUser(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse<JwtUserInfoDto>.Fail("User is not authorized."));

        var result = await _mediator.Send(new GetCurrentUserQuery(userId), ct);

        if (result is null)
            return NotFound(BaseResponse<JwtUserInfoDto>.Fail("User could not be found."));

        return Ok(BaseResponse<JwtUserInfoDto>.Ok(result));
    }
    [AllowAnonymous]
    [HttpPost("google-login")]
    public async Task<ActionResult<BaseResponse>> GoogleLogin(
       [FromBody] GoogleLoginRequest request,
       CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GoogleLoginCommand(request.IdToken),
            cancellationToken);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

}
