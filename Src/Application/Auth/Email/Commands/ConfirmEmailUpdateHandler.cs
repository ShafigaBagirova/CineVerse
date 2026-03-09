using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;

namespace Application.Auth.Email.Commands;

public sealed class ConfirmUpdateEmailCommandHandler
    : IRequestHandler<ConfirmUpdateEmailCommand, BaseResponse>
{
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IIdentityService _identityService;

    public ConfirmUpdateEmailCommandHandler(
        IEmailVerificationCodeRepository codeRepository,
        IIdentityService identityService)
    {
        _codeRepository = codeRepository;
        _identityService = identityService;
    }

    public async Task<BaseResponse> Handle(ConfirmUpdateEmailCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewEmail))
            return BaseResponse.Fail("New email is required.");

        if (string.IsNullOrWhiteSpace(request.Code))
            return BaseResponse.Fail("Code is required.");

        var entity = await _codeRepository.GetActiveByEmailAsync(request.NewEmail, ct);

        if (entity is null)
            return BaseResponse.Fail("Verification code not found.");

        if (entity.IsUsed)
            return BaseResponse.Fail("Verification code has already been used.");

        if (entity.ExpiresAtUtc <= DateTime.UtcNow)
            return BaseResponse.Fail("Verification code expired.");

        var hashedCode = VerificationCodeHasher.Hash(request.Code);

        if (!string.Equals(entity.CodeHash, hashedCode, StringComparison.Ordinal))
            return BaseResponse.Fail("Invalid verification code.");

        await _codeRepository.MarkAsUsedAsync(entity.Id, ct);

        return await _identityService.UpdateEmailAsync(request.UserId, request.NewEmail);
    }
}