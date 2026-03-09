using Application.Auth.Login.Dtos;
using Application.Auth.Password.Dtos;
using Application.Auth.Register.Dtos;
using Application.Auth.User.Dtos;
using Application.Auth.UserName.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Constants;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<CineVerseUser> _userManager;
    private readonly SignInManager<CineVerseUser> _signInManager;

    public IdentityService(
        UserManager<CineVerseUser> userManager,
        SignInManager<CineVerseUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Success, List<string> Errors, string? UserId)> RegisterAsync(RegisterRequest registerRequest) 
    {
        var user = new CineVerseUser
        {
            UserName = registerRequest.UserName,
            Email = registerRequest.Email,
            FullName = registerRequest.FullName,
            DateOfBirth = registerRequest.DateOfBirth,
            Gender = registerRequest.Gender,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description)
                .ToList();

            return (false, errors, null);
        }
        var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.User);

        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors
                .Select(e => e.Description)
                .ToList();

            return (false, errors, null);
        }

        return (true, new List<string>(), user.Id);
    }
    public async Task<(bool Success, string? UserId)> ValidateUserAsync(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByNameAsync(loginRequest.Login)
                   ?? await _userManager.FindByEmailAsync(loginRequest.Login);

        if (user is null)
            return (false, null);

        var signIn = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, true);

        return signIn.Succeeded
            ? (true, user.Id)
            : (false, null);
    }
    public async Task<JwtUserInfoDto?> GetUserInfoAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return null;
        var roles = await _userManager.GetRolesAsync(user);

        return new JwtUserInfoDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            roles
        );
    }
    public async Task<bool> ConfirmEmailAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return false;

        if (user.EmailConfirmed)
            return true;

        user.EmailConfirmed = true;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }
    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }
    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return false;

        return user.EmailConfirmed;
    }
    public async Task<JwtUserInfoDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new JwtUserInfoDto(
            user.Id,
            user.UserName!,
            user.Email!,
            roles
        );
    }
    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }
    public async Task<BaseResponse> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        if (string.IsNullOrWhiteSpace(resetPasswordRequest.Email))
            return BaseResponse.Fail("Email is required.");

        if (string.IsNullOrWhiteSpace(resetPasswordRequest.Token))
            return BaseResponse.Fail("Token is required.");

        if (string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            return BaseResponse.Fail("New password is required.");

        var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);
        if (user is null)
            return BaseResponse.Fail("User not found.");

        var decodedToken = WebUtility.UrlDecode(resetPasswordRequest.Token);

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, resetPasswordRequest.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(x => x.Description)
                .ToList();

            return BaseResponse.Fail(string.Join(", ", errors));
        }

        return BaseResponse.Ok("Password reset successfully.");
    }
    public async Task<BaseResponse> UpdateUserNameAsync(string userId, string newUserName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BaseResponse.Fail("User not found.");

        user.UserName = newUserName;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BaseResponse.Fail(string.Join(", ", errors));
        }

        return BaseResponse.Ok("Username updated successfully.");
    }
    public async Task<BaseResponse> ChangePasswordAsync(
    string userId,
    string currentPassword,
    string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BaseResponse.Fail("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BaseResponse.Fail(string.Join(", ", errors));
        }

        return BaseResponse.Ok("Password changed successfully.");
    }
    public async Task<BaseResponse> UpdateEmailAsync(string userId, string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            return BaseResponse.Fail("New email is required.");

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BaseResponse.Fail("User not found.");

        var existingUser = await _userManager.FindByEmailAsync(newEmail);
        if (existingUser is not null && existingUser.Id != userId)
            return BaseResponse.Fail("This email is already in use.");

        user.Email = newEmail;
        user.NormalizedEmail = _userManager.NormalizeEmail(newEmail);
        user.EmailConfirmed = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BaseResponse.Fail(string.Join(", ", errors));
        }

        return BaseResponse.Ok("Email updated successfully. Please confirm your new email.");
    }
    public async Task<(bool Success, string? Token, string? Message)> GenerateChangeEmailTokenAsync(
    string userId,
    string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            return (false, null, "New email is required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return (false, null, "User not found.");

        var existingUser = await _userManager.FindByEmailAsync(newEmail);
        if (existingUser is not null && existingUser.Id != userId)
            return (false, null, "This email is already in use.");

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

        return (true, token, null);
    }
    public async Task<BaseResponse> ConfirmEmailChangeAsync(
    string userId,
    string newEmail,
    string token)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            return BaseResponse.Fail("New email is required.");

        if (string.IsNullOrWhiteSpace(token))
            return BaseResponse.Fail("Token is required.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return BaseResponse.Fail("User not found.");

        var decodedToken = WebUtility.UrlDecode(token);

        var result = await _userManager.ChangeEmailAsync(user, newEmail, decodedToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return BaseResponse.Fail(string.Join(", ", errors));
        }

        user.UserName ??= newEmail;
        user.NormalizedEmail = _userManager.NormalizeEmail(newEmail);

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return BaseResponse.Fail(string.Join(", ", errors));
        }

        return BaseResponse.Ok("Email changed successfully.");
    }
    public async Task<UserProfileDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        return new UserProfileDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.FullName ?? string.Empty
        );
    }
    public async Task<List<UserProfileDto>> GetAllUsersAsync()
    {
        var users = _userManager.Users
            .Select(u => new UserProfileDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.FullName ?? string.Empty
            ))
            .ToList();

        return await Task.FromResult(users);
    }
}