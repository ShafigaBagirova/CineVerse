using Application.Common.Responses;
using MediatR;

namespace Application.Reviews.Commands;

public record DeleteReviewByAdminCommand(int ReviewId)
    : IRequest<BaseResponse>;
