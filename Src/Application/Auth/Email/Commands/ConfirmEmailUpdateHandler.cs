using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.Email.Commands;

public sealed class ConfirmUpdateEmailCommandHandler
    : IRequestHandler<ConfirmUpdateEmailCommand, BaseResponse>
{
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<ConfirmUpdateEmailCommandHandler> _logger;

    public ConfirmUpdateEmailCommandHandler(
        IEmailVerificationCodeRepository codeRepository,
        IIdentityService identityService,
        ILogger<ConfirmUpdateEmailCommandHandler> logger)
    {
        _codeRepository = codeRepository;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(ConfirmUpdateEmailCommand request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Confirm email update attempt. UserId: {UserId}, NewEmail: {NewEmail}",
            request.UserId,
            request.NewEmail);

        if (string.IsNullOrWhiteSpace(request.NewEmail))
        {
            _logger.LogWarning("Confirm email update failed. NewEmail is empty.");
            return BaseResponse.Fail("New email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            _logger.LogWarning(
                "Confirm email update failed. Verification code missing. UserId: {UserId}",
                request.UserId);
            return BaseResponse.Fail("Code is required.");
        }

        var entity = await _codeRepository.GetActiveByEmailAsync(request.NewEmail, ct);

        if (entity is null)
        {
            _logger.LogWarning(
                "Confirm email update failed. Verification record not found. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            return BaseResponse.Fail("Verification code not found.");
        }

        if (entity.IsUsed)
        {
            _logger.LogWarning(
                "Confirm email update failed. Code already used. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            return BaseResponse.Fail("Verification code has already been used.");
        }

        if (entity.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _logger.LogWarning(
                "Confirm email update failed. Code expired. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            return BaseResponse.Fail("Verification code expired.");
        }

        var hashedCode = VerificationCodeHasher.Hash(request.Code);

        if (!string.Equals(entity.CodeHash, hashedCode, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Confirm email update failed. Invalid verification code. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);

            return BaseResponse.Fail("Invalid verification code.");
        }

        await _codeRepository.MarkAsUsedAsync(entity.Id, ct);

        _logger.LogInformation(
            "Verification code validated successfully. Updating email for UserId: {UserId}",
            request.UserId);

        var response = await _identityService.UpdateEmailAsync(request.UserId, request.NewEmail);

        if (response.Success)
        {
            _logger.LogInformation(
                "Email updated successfully. UserId: {UserId}, NewEmail: {NewEmail}",
                request.UserId,
                request.NewEmail);
        }
        else
        {
            _logger.LogWarning(
                "Email update failed. UserId: {UserId}, Message: {Message}",
                request.UserId,
                response.Message);
        }

        return response;
    }
}