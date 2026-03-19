using Application.Common.Responses;
using MediatR;

namespace Application.Movies.Commands;

public record DeleteReviewByAdminCommand(int ReviewId)
    : IRequest<BaseResponse>;
