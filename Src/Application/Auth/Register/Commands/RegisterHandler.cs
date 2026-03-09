using Application.Auth.Register.Dtos;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using System.Security.Cryptography;

namespace Application.Auth.Register.Commands;

public sealed class RegisterHandler
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserUniquenessChecker _userUniquenessChecker;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IEmailSender _emailSender;

    public RegisterHandler(
        IIdentityService identityService,
        IUserUniquenessChecker userUniquenessChecker,
        IEmailVerificationCodeRepository codeRepository,
        IEmailSender emailSender)
    {
        _identityService = identityService;
        _userUniquenessChecker = userUniquenessChecker;
        _codeRepository = codeRepository;
        _emailSender = emailSender;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var req = request.Request;

        var emailTaken = await _userUniquenessChecker.IsEmailTakenAsync(req.Email, ct);
        if (emailTaken)
            return new RegisterResponse(false, "Email is already registered.");

        var userNameTaken = await _userUniquenessChecker.IsUserNameTakenAsync(req.UserName, ct);
        if (userNameTaken)
            return new RegisterResponse(false, "Username is already taken.");

        string? userId = null;

        try
        {
            var (success, errors, createdUserId) = await _identityService.RegisterAsync(req);
            if (!success || string.IsNullOrWhiteSpace(createdUserId))
            {
                var message = errors.Count > 0
                    ? string.Join(" ", errors)
                    : "Registration failed.";

                return new RegisterResponse(false, message);
            }

            userId = createdUserId;

            var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var codeHash = VerificationCodeHasher.Hash(code);

            var existing = await _codeRepository.GetActiveByEmailAsync(req.Email, ct);

            if (existing is not null)
            {
                existing.UserId = userId;
                existing.Email = req.Email;
                existing.CodeHash = codeHash;
                existing.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5);
                existing.IsUsed = false;

                await _codeRepository.UpdateAsync(existing, ct);
            }
            else
            {
                var entity = new EmailVerificationCode
                {
                    UserId = userId,
                    Email = req.Email,
                    CodeHash = codeHash,
                    ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                    IsUsed = false
                };

                await _codeRepository.AddAsync(entity, ct);
            }

            var htmlBody = $"""
            <h2>CineVerse Verification Code</h2>
            <p>Your verification code is:</p>
            <h1>{code}</h1>
            <p>This code will expire in 5 minutes.</p>
            """;

            var textBody = $"Your CineVerse verification code is: {code}. This code will expire in 5 minutes.";

            await _emailSender.SendAsync(
                req.Email,
                "CineVerse Verification Code",
                htmlBody,
                textBody,
                ct
            );

            return new RegisterResponse(true, "Verification code sent to your email.");
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await _identityService.DeleteUserAsync(userId);
            }

            return new RegisterResponse(false, "Registration failed. Please try again.");
        }
    }

}