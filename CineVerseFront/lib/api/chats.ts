
import { apiRequest } from "@/lib/api/http"

export interface ChatParticipantDto {
  id: number
  chatId: number
  userId: string
  userName?: string | null
  joinedAt: string
}

export interface MessageDto {
  id: number
  chatId: number
  senderId: string
  senderName?: string | null
  content: string
  createdAt: string
  isRead: boolean
}

export interface ChatDto {
  id: number
  createdAt: string
  isGroup: boolean
  participants: ChatParticipantDto[]
  lastMessage?: MessageDto | null
  unreadCount?: number
}

export async function createPrivateChat(receiverUserId: string) {
  return apiRequest<ChatDto>(`/api/chats/private/${encodeURIComponent(receiverUserId)}`, {
    method: "POST",
    auth: true,
  })
}

export async function getMyChats() {
  return apiRequest<ChatDto[]>("/api/chats/my", {
    method: "GET",
    auth: true,
    cache: "no-store",
    headers: {
      "Cache-Control": "no-cache",
      Pragma: "no-cache",
    },
  })
}

export async function getChatMessages(chatId: number) {
  return apiRequest<MessageDto[]>(`/api/chats/${chatId}/messages`, {
    method: "GET",
    auth: true,
    cache: "no-store",
    headers: {
      "Cache-Control": "no-cache",
      Pragma: "no-cache",
    },
  })
}

export async function sendMessage(chatId: number, content: string) {
  return apiRequest<MessageDto>("/api/messages", {
    method: "POST",
    auth: true,
    body: { chatId, content },
  })
}

export async function markMessagesAsRead(chatId: number) {
  return apiRequest<void>(`/api/messages/${chatId}/read`, {
    method: "PUT",
    auth: true,
  })
}
