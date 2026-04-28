import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/user"

/** Backend serializes `Domain.Enums.InAppNotificationType` as the enum member name (e.g. `NewMovieAdded`). */
export type InAppNotificationType = string

export interface GetMyNotificationsResponse {
  id: number
  title: string
  message: string
  isRead: boolean
  type: string
  createdAt: string
}

export interface GetUnreadNotificationCountResponse {
  unreadCount: number
}

type PaginatedNotifications = PaginatedResponse<GetMyNotificationsResponse> & {
  Items?: GetMyNotificationsResponse[]
}

export async function getMyNotifications(pageNumber = 1, pageSize = 10) {
  const data = await apiRequest<PaginatedNotifications>(
    `/api/notification/my?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    { method: "GET", auth: true }
  )
  const items = Array.isArray(data.items) ? data.items : Array.isArray(data.Items) ? data.Items : []
  return {
    ...data,
    items,
  }
}

export async function markNotificationAsRead(id: number) {
  return apiRequest<unknown>(`/api/notification/${id}/read`, {
    method: "PATCH",
    auth: true,
  })
}

export async function getUnreadNotificationCount() {
  const data = await apiRequest<GetUnreadNotificationCountResponse & { UnreadCount?: number }>(
    "/api/notification/unread-count",
    {
      method: "GET",
      auth: true,
    }
  )
  const unreadCount =
    typeof data.unreadCount === "number"
      ? data.unreadCount
      : typeof data.UnreadCount === "number"
        ? data.UnreadCount
        : 0
  return { unreadCount }
}

/** Readable badge text for known notification types; unknown types still display safely. */
export function formatNotificationType(type: string): string {
  const labels: Record<string, string> = {
    NewMovieAdded: "New movie",
    Campaign: "Campaign",
    SystemAnnouncement: "Announcement",
    NewCinemaAdded: "New cinema",
    MovieRecommendation: "Movie recommendation",
    UserRecommendation: "Suggested user",
  }
  const t = type?.trim() ?? ""
  if (labels[t]) return labels[t]
  if (!t) return "Notification"
  return t.replace(/([A-Z])/g, " $1").replace(/^ /, "").trim()
}
