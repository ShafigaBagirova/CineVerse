using Application.Auth.Login.Dtos;

namespace Application.Common.Interfaces;


public interface IGoogleTokenValidator
{
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}