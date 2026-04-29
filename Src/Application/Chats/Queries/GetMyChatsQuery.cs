using Application.Chats.Dtos;
using Application.Common.Responses;
using MediatR;

namespace Application.Chats.Queries;

public sealed record GetMyChatsQuery() : IRequest<BaseResponse<List<ChatDto>>>;
