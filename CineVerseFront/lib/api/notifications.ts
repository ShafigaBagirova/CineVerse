import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/user"

export type InAppNotificationType =
  | "NewMovieAdded"
  | "Campaign"
  | "SystemAnnouncement"
  | "NewCinemaAdded"
  | "MovieRecommendation"
  | "UserRecommendation"

export interface GetMyNotificationsResponse {
  id: number
  title: string
  message: string
  isRead: boolean
  type: InAppNotificationType
  createdAt: string
}

export interface GetUnreadNotificationCountResponse {
  unreadCount: number
}

export async function getMyNotifications(pageNumber = 1, pageSize = 10) {
  return apiRequest<PaginatedResponse<GetMyNotificationsResponse>>(
    `/api/notification/my?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    { method: "GET", auth: true }
  )
}

export async function markNotificationAsRead(id: number) {
  return apiRequest<unknown>(`/api/notification/${id}/read`, {
    method: "PATCH",
    auth: true,
  })
}

export async function getUnreadNotificationCount() {
  return apiRequest<GetUnreadNotificationCountResponse>("/api/notification/unread-count", {
    method: "GET",
    auth: true,
  })
}
