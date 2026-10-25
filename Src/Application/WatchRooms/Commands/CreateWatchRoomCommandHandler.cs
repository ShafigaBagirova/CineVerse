using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Application.WatchRooms.Commands;

public class CreateWatchRoomCommandHandler : IRequestHandler<CreateWatchRoomCommand, BaseResponse<GetWatchRoomByIdResponse>>
{
    private const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 8;
    private const int MaxCodeAttempts = 5;

    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateWatchRoomCommandHandler> _logger;

    public CreateWatchRoomCommandHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<CreateWatchRoomCommandHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<GetWatchRoomByIdResponse>> Handle(CreateWatchRoomCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "CreateWatchRoomCommand started. UserId: {UserId}, ContentUrl: {ContentUrl}",
            currentUserId,
            request.Request.ContentUrl);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Authenticated user not found.");

        if (!WatchContentUrlParser.TryParse(request.Request.ContentUrl, out var platform, out var contentId))
        {
            _logger.LogWarning(
                "CreateWatchRoomCommand failed. Unsupported content url. ContentUrl: {ContentUrl}",
                request.Request.ContentUrl);

            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Only valid YouTube links are supported right now.");
        }

        var code = await GenerateUniqueCodeAsync(cancellationToken);
        if (code is null)
        {
            _logger.LogError("CreateWatchRoomCommand failed. Could not generate a unique room code.");
            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Could not generate a room code. Please try again.");
        }

        var now = DateTime.UtcNow;

        var room = _mapper.Map<WatchRoom>(request.Request);
        room.Code = code;
        room.HostUserId = currentUserId;
        room.Platform = platform;
        room.ContentId = contentId;
        room.IsActive = true;
        room.CreatedAt = now;
        room.Members = [new WatchRoomMember { UserId = currentUserId, JoinedAt = now }];

        await _watchRoomRepository.AddAsync(room, cancellationToken);
        await _watchRoomRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateWatchRoomCommand completed successfully. WatchRoomId: {WatchRoomId}, Code: {Code}",
            room.Id,
            room.Code);

        var response = _mapper.Map<GetWatchRoomByIdResponse>(room);
        foreach (var member in response.Members)
            member.UserName = await _userReadService.GetUserNameAsync(member.UserId, cancellationToken);

        return BaseResponse<GetWatchRoomByIdResponse>.Ok(response, "Watch room created successfully.");
    }

    private async Task<string?> GenerateUniqueCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
        {
            var code = string.Create(CodeLength, 0, static (span, _) =>
            {
                for (var i = 0; i < span.Length; i++)
                    span[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
            });

            if (!await _watchRoomRepository.CodeExistsAsync(code, cancellationToken))
                return code;
        }

        return null;
    }
}
