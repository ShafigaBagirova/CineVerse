using Application.Auth.Login.Dtos;
using Application.Auth.Password.Dtos;
using Application.Auth.Register.Dtos;
using Application.Auth.User.Dtos;
using Application.Auth.UserName.Dtos;
using Application.Common.Responses;
using Application.Follows.Dtos;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Success, List<string> Errors, string? UserId)> RegisterAsync(RegisterRequest registerRequest);
    Task<(bool Success, string? UserId)> ValidateUserAsync(LoginRequest loginRequest);
    Task<JwtUserInfoDto?> GetUserInfoAsync(string userId);
    Task<bool> ConfirmEmailAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> IsEmailConfirmedAsync(string userId);
    Task<JwtUserInfoDto?> GetUserByEmailAsync(string email);
    Task<string?> GeneratePasswordResetTokenAsync(string email);
    Task<BaseResponse> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
     Task<BaseResponse> UpdateUserNameAsync(string userId, string newUserName);
    Task<BaseResponse> ChangePasswordAsync( string userId, string currentPassword,string newPassword);
    Task<BaseResponse> UpdateEmailAsync(string userId, string newEmail);
    Task<(bool Success, string? Token, string? Message)> GenerateChangeEmailTokenAsync(string userId, string newEmail);
    Task<BaseResponse> ConfirmEmailChangeAsync(string userId,string newEmail,string token);
    Task<UserProfileDto?> GetUserByIdAsync(string userId);
    Task<Dictionary<string, string>> GetUserNamesByIdsAsync(IEnumerable<string> userIds);
    Task<bool> UserExistsAsync(string userId);
    Task<List<FollowUserItemDto>> GetUsersByIdsAsync(List<string> userIds, CancellationToken cancellationToken);
    Task<UserInfoDto?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<(bool Success, List<string> Errors, string? UserId)> CreateGoogleUserAsync(
        string email,
        string? firstName,
        string? lastName,
        string provider,
        string providerKey,
        string? profilePictureUrl,
        CancellationToken cancellationToken);
    Task<UserInfoDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken);
    Task<BaseResponse> UpdateAvatarAsync(string userId, string? avatarUrl, CancellationToken cancellationToken);
    Task<string?> GetAvatarUrlAsync(string userId, CancellationToken cancellationToken);
    Task<PaginatedResponse<UserProfileDto>> GetUsersAsync(GetUsersRequest request,CancellationToken cancellationToken);
    Task<int> CountUsersAsync(CancellationToken cancellationToken);
    Task<int> CountVipUsersAsync(CancellationToken cancellationToken);

}
