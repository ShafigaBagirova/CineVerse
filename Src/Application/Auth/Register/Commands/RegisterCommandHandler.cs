using Application.Auth.Register.Dtos;
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.Auth.Register.Commands;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IUserUniquenessChecker _userUniquenessChecker;
    private readonly IEmailVerificationCodeRepository _codeRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IUserUniquenessChecker userUniquenessChecker,
        IEmailVerificationCodeRepository codeRepository,
        IEmailSender emailSender,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _userUniquenessChecker = userUniquenessChecker;
        _codeRepository = codeRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var req = request.Request;

        _logger.LogInformation(
            "User registration started. Email: {Email}, UserName: {UserName}",
            req.Email,
            req.UserName);

        var emailTaken = await _userUniquenessChecker.IsEmailTakenAsync(req.Email, ct);
        if (emailTaken)
        {
            _logger.LogWarning(
                "Registration failed. Email already registered. Email: {Email}",
                req.Email);

            return new RegisterResponse(false, "Email is already registered.");
        }

        var userNameTaken = await _userUniquenessChecker.IsUserNameTakenAsync(req.UserName, ct);
        if (userNameTaken)
        {
            _logger.LogWarning(
                "Registration failed. Username already taken. UserName: {UserName}",
                req.UserName);

            return new RegisterResponse(false, "Username is already taken.");
        }

        string? userId = null;

        try
        {
            var (success, errors, createdUserId) = await _identityService.RegisterAsync(req);

            if (!success || string.IsNullOrWhiteSpace(createdUserId))
            {
                var message = errors.Count > 0
                    ? string.Join(" ", errors)
                    : "Registration failed.";

                _logger.LogWarning(
                    "Registration failed in identity service. Email: {Email}, UserName: {UserName}, Errors: {Errors}",
                    req.Email,
                    req.UserName,
                    message);

                return new RegisterResponse(false, message);
            }

            userId = createdUserId;

            _logger.LogInformation(
                "User created successfully in identity service. UserId: {UserId}, Email: {Email}",
                userId,
                req.Email);

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

                _logger.LogInformation(
                    "Existing email verification record updated. UserId: {UserId}, Email: {Email}",
                    userId,
                    req.Email);
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
                await _codeRepository.SaveChangesAsync(ct);
                _logger.LogInformation(
                    "New email verification record created. UserId: {UserId}, Email: {Email}",
                    userId,
                    req.Email);
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

            _logger.LogInformation(
                "Verification email sent successfully. UserId: {UserId}, Email: {Email}",
                userId,
                req.Email);

            return new RegisterResponse(true, "Verification code sent to your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during registration. Email: {Email}, UserName: {UserName}, UserId: {UserId}",
                req.Email,
                req.UserName,
                userId);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning(
                    "Rolling back created user due to registration failure. UserId: {UserId}",
                    userId);

                await _identityService.DeleteUserAsync(userId);
                await _codeRepository.SaveChangesAsync( ct);
            }

            return new RegisterResponse(false, "Registration failed. Please try again.");
        }
    }
}