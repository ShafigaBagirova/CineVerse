import { apiRequest } from "@/lib/api/http"
import type { ReviewDto } from "@/lib/api/reviews"

export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages?: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

export interface UserRatingDto {
  movieId: number
  movieTitle: string
  posterUrl?: string | null
  rating: number
  createdAt: string
}

export interface UserSearchDto {
  id: string
  userName: string
  fullName?: string | null
  avatarUrl?: string | null
}

function pickString(o: Record<string, unknown>, ...keys: string[]): string {
  for (const k of keys) {
    const v = o[k]
    if (typeof v === "string" && v.length > 0) return v
  }
  return ""
}

/** Maps API rows (userId or id) so `UserSearchDto.id` is always set when the backend sends userId. */
export function normalizeUserSearchItem(raw: unknown): UserSearchDto {
  if (raw === null || typeof raw !== "object") {
    return { id: "", userName: "", fullName: null, avatarUrl: null }
  }
  const o = raw as Record<string, unknown>
  const id = pickString(o, "id", "userId", "Id", "UserId")
  const userName = pickString(o, "userName", "UserName")
  const fn = o.fullName ?? o.FullName
  const av = o.avatarUrl ?? o.AvatarUrl
  return {
    id,
    userName,
    fullName: fn === null || fn === undefined ? null : String(fn),
    avatarUrl: av === null || av === undefined ? null : String(av),
  }
}

export async function getUserReviews(userId: string, page = 1, pageSize = 20) {
  return apiRequest<PaginatedResponse<ReviewDto>>(
    `/api/user/${encodeURIComponent(userId)}/reviews?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: false }
  )
}

export async function getUserRatings(userId: string, page = 1, pageSize = 50) {
  return apiRequest<PaginatedResponse<UserRatingDto>>(
    `/api/user/${encodeURIComponent(userId)}/ratings?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: false }
  )
}

/** Public profile row from GET /api/user/{id} (ASP.NET Identity user id string). */
export interface UserPublicProfileDto {
  id: string
  userId: string
  userName: string
  fullName: string | null
  avatarUrl: string | null
}

function normalizeUserPublicProfile(raw: unknown): UserPublicProfileDto | null {
  if (raw === null || typeof raw !== "object") return null
  const o = raw as Record<string, unknown>
  const id = pickString(o, "id", "userId", "Id", "UserId")
  if (!id) return null
  const userName = pickString(o, "userName", "UserName")
  const fn = o.fullName ?? o.FullName
  const av = o.avatarUrl ?? o.AvatarUrl
  return {
    id,
    userId: id,
    userName,
    fullName: fn === null || fn === undefined ? null : String(fn),
    avatarUrl: av === null || av === undefined ? null : String(av),
  }
}

/** Loads a user by backend user id (same string as search results and JWT NameIdentifier). */
export async function getUserProfile(userId: string): Promise<UserPublicProfileDto | null> {
  try {
    const data = await apiRequest<unknown>(`/api/user/${encodeURIComponent(userId)}`, {
      method: "GET",
      auth: false,
    })
    return normalizeUserPublicProfile(data)
  } catch {
    return null
  }
}

export async function searchUsers(searchTerm: string, pageNumber = 1, pageSize = 10) {
  const term = searchTerm.trim()
  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(pageSize),
  })
  if (term) query.set("searchTerm", term)

  const data = await apiRequest<PaginatedResponse<UserSearchDto>>(`/api/user?${query.toString()}`, {
    method: "GET",
    auth: false,
    quiet: true,
  })

  const rawItems = data.items ?? (data as unknown as { Items?: unknown[] }).Items ?? []
  const items = Array.isArray(rawItems) ? rawItems.map(normalizeUserSearchItem) : []
  return {
    ...data,
    items,
  }
}
