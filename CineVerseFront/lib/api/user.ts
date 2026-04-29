import { apiRequest } from "@/lib/api/http"
import { ApiError } from "@/lib/api/types"
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
  email?: string | null
  fullName?: string | null
  avatarUrl?: string | null
  profileImageUrl?: string | null
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
  const email = pickString(o, "email", "Email")
  const fn = o.fullName ?? o.FullName
  const av = o.avatarUrl ?? o.AvatarUrl ?? o.profileImageUrl ?? o.ProfileImageUrl
  return {
    id,
    userName,
    email: email || null,
    fullName: fn === null || fn === undefined ? null : String(fn),
    avatarUrl: av === null || av === undefined ? null : String(av),
    profileImageUrl: av === null || av === undefined ? null : String(av),
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
  profileImageUrl?: string | null
}

function normalizeUserPublicProfile(raw: unknown): UserPublicProfileDto | null {
  if (raw === null || typeof raw !== "object") return null
  const o = raw as Record<string, unknown>
  const id = pickString(o, "id", "userId", "Id", "UserId")
  if (!id) return null
  const userName = pickString(o, "userName", "UserName")
  const fn = o.fullName ?? o.FullName
  const av =
    o.profileImageUrl ??
    o.ProfileImageUrl ??
    o.avatarUrl ??
    o.AvatarUrl ??
    o.imageUrl ??
    o.ImageUrl
  return {
    id,
    userId: id,
    userName,
    fullName: fn === null || fn === undefined ? null : String(fn),
    avatarUrl: av === null || av === undefined ? null : String(av),
    profileImageUrl: av === null || av === undefined ? null : String(av),
  }
}

/** Loads a user by backend user id (same string as search results and JWT NameIdentifier). */
export async function getUserProfile(userId: string): Promise<UserPublicProfileDto | null> {
  const url = `/api/user/${encodeURIComponent(userId)}`
  try {
    const data = await apiRequest<unknown>(url, {
      method: "GET",
      auth: false,
    })
    return normalizeUserPublicProfile(data)
  } catch (error) {
    if (error instanceof ApiError) {
      console.error("[profile-api] getUserProfile failed", {
        userId,
        status: error.status,
        message: error.message,
        response: error.parsedJson ?? error.rawText,
        url: error.path ?? url,
      })
    } else {
      console.error("[profile-api] getUserProfile failed", { userId, response: error, url })
    }
    return null
  }
}

export async function getCurrentUserProfile(): Promise<UserPublicProfileDto | null> {
  const url = "/api/user/me"
  try {
    const data = await apiRequest<unknown>(url, {
      method: "GET",
      auth: true,
    })
    return normalizeUserPublicProfile(data)
  } catch (error) {
    if (error instanceof ApiError) {
      console.error("[profile-api] getCurrentUserProfile failed", {
        status: error.status,
        message: error.message,
        response: error.parsedJson ?? error.rawText,
        url: error.path ?? url,
      })
    } else {
      console.error("[profile-api] getCurrentUserProfile failed", { response: error, url })
    }
    return null
  }
}

/** GET /api/user — paginated directory (same handler as search; use `searchTerm` to filter). */
export interface UserListItemDto {
  userId: string
  id: string
  userName: string
  fullName: string
  avatarUrl?: string | null
}

export async function getUsersList(params: { pageNumber?: number; pageSize?: number; searchTerm?: string }) {
  const query = new URLSearchParams({
    pageNumber: String(params.pageNumber ?? 1),
    pageSize: String(params.pageSize ?? 20),
  })
  const term = params.searchTerm?.trim()
  if (term) query.set("searchTerm", term)

  const data = await apiRequest<PaginatedResponse<UserListItemDto>>(`/api/user?${query.toString()}`, {
    method: "GET",
    auth: true,
  })
  const rawItems = data.items ?? (data as unknown as { Items?: unknown[] }).Items ?? []
  const items = Array.isArray(rawItems)
    ? rawItems.map((row) => {
        const n = normalizeUserSearchItem(row)
        const o = row as Record<string, unknown>
        const fullName = (o.fullName ?? o.FullName) as string | null | undefined
        return {
          userId: n.id,
          id: n.id,
          userName: n.userName,
          fullName: fullName != null && fullName !== "" ? String(fullName) : "",
          avatarUrl: n.avatarUrl,
        }
      })
    : []
  return { ...data, items }
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

export async function uploadUserAvatar(file: File) {
  const formData = new FormData()
  formData.append("Avatar", file)

  return apiRequest<string>("/api/user/avatar", {
    method: "POST",
    auth: true,
    body: formData,
  })
}
