using Application.Auth.User.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Auth.User.Queries;

public sealed class GetUsersQueryHandler
    : IRequestHandler<GetUsersQuery, PaginatedResponse<UserProfileDto>>
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetUsersQueryHandler> _logger;

    public GetUsersQueryHandler(
        IIdentityService identityService,
        ILogger<GetUsersQueryHandler> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<PaginatedResponse<UserProfileDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting users. PageNumber: {PageNumber}, PageSize: {PageSize}, SearchTerm: {SearchTerm}",
            request.Request.PageNumber,
            request.Request.PageSize,
            request.Request.SearchTerm);

        var response = await _identityService.GetUsersAsync(request.Request, cancellationToken);

        _logger.LogInformation(
            "Users retrieved successfully. Count: {Count}, TotalCount: {TotalCount}",
            response.Items.Count,
            response.TotalCount);

        return response;
    }
}