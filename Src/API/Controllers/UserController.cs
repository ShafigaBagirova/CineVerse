using Application.Auth.Email.Commands;
using Application.Auth.Email.Dtos;
using Application.Auth.Password.Commands;
using Application.Auth.Password.Dtos;
using Application.Auth.User.Commands;
using Application.Auth.User.Dtos;
using Application.Auth.User.Queries;
using Application.Auth.UserName.Commands;
using Application.Auth.UserName.Dtos;
using Application.Common.Responses;
using Application.Reviews.Dtos;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
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

    [HttpPut("username")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> UpdateUserName(
        [FromBody] UpdateUserNameRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse.Fail("User is not authorized."));

        var result = await _mediator.Send(
            new UpdateUserNameCommand(userId, request.NewUserName), ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("password")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse.Fail("User is not authorized."));

        var result = await _mediator.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("email")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> UpdateEmail(
        [FromBody] UpdateEmailRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse.Fail("User is not authorized."));

        var result = await _mediator.Send(
            new UpdateEmailCommand(userId, request.NewEmail),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("confirm-email-update")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> ConfirmUpdateEmail(
        [FromBody] ConfirmUpdateEmailRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse.Fail("User is not authorized."));

        var result = await _mediator.Send(
            new ConfirmUpdateEmailCommand(userId, request.NewEmail, request.Code),
            ct);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{userId}/reviews")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<ReviewDto>>>> GetUserReviews(
        [FromRoute] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserReviewsQuery(userId, page, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [HttpGet("{userId}/ratings")]
    public async Task<ActionResult<BaseResponse<PaginatedResponse<UserRatingDto>>>> GetUserRatings(
        [FromRoute] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserRatingsQuery(userId, page, pageSize);

        var response = await _mediator.Send(query, cancellationToken);

        return response.Success ? Ok(response) : BadRequest(response);
    }

    [Authorize]
    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteUserAvatarCommand(),
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [Authorize]
    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BaseResponse>> UploadAvatar(
        [FromForm] UploadAvatarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UploadUserAvatarCommand(request),
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
    [HttpPost("vip/subscribe")]
    [Authorize(Policy = Policies.Authenticated)]
    public async Task<ActionResult<BaseResponse>> SubscribeVip(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(BaseResponse.Fail("User is not authorized."));

        var result = await _mediator.Send(new SubscribeVipCommand(userId), ct);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<BaseResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetMyProfileQuery(),
            cancellationToken);

        if (result is null)
            return NotFound(BaseResponse.Fail("User not found."));

        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BaseResponse<UserProfileDto>>> GetUserById(
        [FromRoute] string id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);

        if (result is null)
            return NotFound(BaseResponse<UserProfileDto>.Fail("User could not be found."));

        return Ok(BaseResponse<UserProfileDto>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse>> GetUsers(
    [FromQuery] GetUsersRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUsersQuery(request),
            cancellationToken);

        return Ok(result);
    }
}
