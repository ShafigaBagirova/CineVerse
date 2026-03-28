using Application.Auth.Login.Dtos;
using Application.Auth.Password.Dtos;
using Application.Auth.Register.Dtos;
using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            user.FullName ?? string.Empty,
            user.AvatarUrl
        );
    }
    public async Task<Dictionary<string, string>> GetUserNamesByIdsAsync(
    IEnumerable<string> userIds)
    {
        var ids = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (!ids.Any())
            return new Dictionary<string, string>();

        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        return users.ToDictionary(
            x => x.Id,
            x => x.UserName ?? string.Empty
        );
    }
    public async Task<bool> UserExistsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user is not null;
    }
    public async Task<List<FollowUserItemDto>> GetUsersByIdsAsync(List<string> userIds, CancellationToken cancellationToken)
    {
        return await _userManager.Users
            .Where(x => userIds.Contains(x.Id))
            .Select(x => new FollowUserItemDto
            {
                UserId = x.Id,
                UserName = x.UserName!,
                FullName = x.FullName,
                AvatarUrl = x.AvatarUrl
            })
            .ToListAsync(cancellationToken);
    }
    public async Task<UserInfoDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList()
        };
    }
    public async Task<(bool Success, List<string> Errors, string? UserId)> CreateGoogleUserAsync(
    string email,
    string? firstName,
    string? lastName,
    string provider,
    string providerKey,
    string? profilePictureUrl,
    CancellationToken cancellationToken)
    {
        var user = new CineVerseUser
        {
            UserName = email,
            Email = email,
            FullName = $"{firstName} {lastName}".Trim(),
            EmailConfirmed = true,
            AvatarUrl = profilePictureUrl
        };

        var createResult = await _userManager.CreateAsync(user);

        if (!createResult.Succeeded)
        {
            return (
                false,
                createResult.Errors.Select(e => e.Description).ToList(),
                null
            );
        }

        var loginResult = await _userManager.AddLoginAsync(
            user,
            new UserLoginInfo(provider, providerKey, provider));

        if (!loginResult.Succeeded)
        {
            return (
                false,
                loginResult.Errors.Select(e => e.Description).ToList(),
                null
            );
        }

        await _userManager.AddToRoleAsync(user, "User");

        return (true, new List<string>(), user.Id);
    }
    public async Task<UserInfoDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.UserName,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList()
        };
    }
    public async Task<BaseResponse> UpdateAvatarAsync(
    string userId,
    string? avatarUrl,
    CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return BaseResponse.Fail("User not found.");

        user.AvatarUrl = avatarUrl;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BaseResponse.Fail(
                string.Join(", ", result.Errors.Select(x => x.Description)));
        }

        return BaseResponse.Ok("Avatar updated successfully.");
    }

    public async Task<string?> GetAvatarUrlAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user?.AvatarUrl;
    }
    public async Task<PaginatedResponse<UserProfileDto>> GetUsersAsync(
    GetUsersRequest request,
    CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();

            query = query.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserProfileDto(
                u.Id,
                u.UserName ?? string.Empty,
                u.FullName ?? string.Empty,
                u.AvatarUrl
            ))
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<UserProfileDto>
        {
            Items = users,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
    public async Task<int> CountUsersAsync(CancellationToken cancellationToken)
    {
        return await _userManager.Users.CountAsync(cancellationToken);
    }

    public async Task<int> CountVipUsersAsync(CancellationToken cancellationToken)
    {
        return await _userManager.Users.CountAsync(x => x.IsVip, cancellationToken);
    }
}
