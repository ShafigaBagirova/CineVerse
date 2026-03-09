using Application.Auth.Email.Commands;
using Application.Auth.Email.Dtos;
using Application.Auth.Login.Commands;
using Application.Auth.Login.Dtos;
using Application.Auth.Password.Commands;
using Application.Auth.Password.Dtos;
using Application.Auth.Refresh.Commands;
using Application.Auth.Refresh.Dtos;
using Application.Auth.Register.Commands;
using Application.Auth.Register.Dtos;
using Application.Auth.User.Dtos;
using Application.Auth.User.Queries;
using Application.Auth.UserName.Commands;
using Application.Auth.UserName.Dtos;
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

        return Ok(BaseResponse<RegisterResponse>.Ok(result));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<TokenResponse>>> Login(
       [FromBody] LoginRequest request,
       CancellationToken ct)
    {
        var token = await _mediator.Send(new LoginCommand(request.Login, request.Password), ct);

        if (token is null)
            return Unauthorized(BaseResponse<TokenResponse>.Fail("Invalid login or password."));

        return Ok(BaseResponse<TokenResponse>.Ok(token));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<TokenResponse>>> Refresh(
    [FromBody] RefreshTokenRequest request,
    CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(BaseResponse<TokenResponse>.Fail("RefreshToken is required."));

        var token = await _mediator.Send(new RefreshCommand(request.RefreshToken), ct);

        if (token is null)
            return Unauthorized(BaseResponse<TokenResponse>.Fail("Invalid or expired refresh token."));

        return Ok(BaseResponse<TokenResponse>.Ok(token));
    }
    [HttpPost("confirm-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmRegistration(
       [FromBody] ConfirmRegistrationCodeRequest request,
       CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Email and code are required.");
        }

        var result = await _mediator.Send(
            new ConfirmRegisterCommand(request.Email, request.Code), ct);

        if (!result)
            return BadRequest("Invalid or expired verification code.");

        return Ok("Email confirmed successfully.");
    }
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse>> ResetPassword(
    [FromBody] ResetPasswordRequest request,
    CancellationToken ct)
    {
        if (request is null)
            return BadRequest(BaseResponse.Fail("Request is required."));

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(BaseResponse.Fail("Email, token and new password are required."));
        }

        var result = await _mediator.Send(new ResetPasswordCommand(request), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse>> ForgotPassword(
    [FromBody] ForgotPasswordRequest request,
    CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(BaseResponse.Fail("Email is required."));

        var result = await _mediator.Send(new ForgotPasswordCommand(request), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("update-username")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> UpdateUserName(
        [FromBody] UpdateUserNameRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(
            new UpdateUserNameCommand(userId, request.NewUserName), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> ChangePassword(
    [FromBody] ChangePasswordRequest request,
    CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpPost("update-email")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> UpdateEmail(
        [FromBody] UpdateEmailRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(
            new UpdateEmailCommand(userId, request.NewEmail),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("confirm-update-email")]
    [Authorize]
    public async Task<ActionResult<BaseResponse>> ConfirmUpdateEmail(
        [FromBody] ConfirmUpdateEmailRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(
            new ConfirmUpdateEmailCommand(userId, request.NewEmail, request.Code),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<JwtUserInfoDto>> GetCurrentUser(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetCurrentUserQuery(userId), ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<UserProfileDto>> GetUserById(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
    [HttpGet]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<ActionResult<List<UserProfileDto>>> GetAllUsers(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(result);
    }
}
