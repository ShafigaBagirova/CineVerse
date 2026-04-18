import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/user"

export interface FollowStatusDto {
  isFollowing: boolean
}

export interface FollowStatsDto {
  followersCount: number
  followingsCount: number
}

export interface FollowUserItemDto {
  userId: string
  userName: string
  fullName?: string | null
  avatarUrl?: string | null
}

export interface SuggestedUserItemDto {
  userId: string
  userName: string
  fullName?: string | null
  avatarUrl?: string | null
  tasteScore: number
  commonMoviesCount: number
}

export interface FollowRelationshipDto {
  isFollowing: boolean
  isFollowedBy: boolean
  isMutual: boolean
  isSelf: boolean
}

export interface FollowInsightsDto {
  followersCount: number
  followingsCount: number
  mutualFollowersCount: number
  mutualFollowingsCount: number
  suggestedUsersCount: number
  tasteSimilarityScore: number
}

export async function followUser(userId: string) {
  return apiRequest<unknown>(`/api/follow/${encodeURIComponent(userId)}`, {
    method: "POST",
    auth: true,
  })
}

export async function unfollowUser(userId: string) {
  return apiRequest<unknown>(`/api/follow/${encodeURIComponent(userId)}`, {
    method: "DELETE",
    auth: true,
  })
}

export async function getFollowStatus(userId: string) {
  return apiRequest<FollowStatusDto>(`/api/follow/${encodeURIComponent(userId)}/status`, {
    method: "GET",
    auth: true,
  })
}

export async function getFollowStats(userId: string) {
  return apiRequest<FollowStatsDto>(`/api/follow/${encodeURIComponent(userId)}/stats`, {
    method: "GET",
    auth: false,
  })
}

export async function getFollowers(userId: string, page = 1, pageSize = 20) {
  return apiRequest<PaginatedResponse<FollowUserItemDto>>(
    `/api/follow/${encodeURIComponent(userId)}/followers?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: false }
  )
}

export async function getFollowings(userId: string, page = 1, pageSize = 20) {
  return apiRequest<PaginatedResponse<FollowUserItemDto>>(
    `/api/follow/${encodeURIComponent(userId)}/followings?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: false }
  )
}

export async function getSuggestedUsers(page = 1, pageSize = 12) {
  return apiRequest<PaginatedResponse<SuggestedUserItemDto>>(
    `/api/follow/suggested-users?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: true, quiet: true }
  )
}

export async function getFollowRelationship(userId: string) {
  return apiRequest<FollowRelationshipDto>(`/api/follow/${encodeURIComponent(userId)}/relationship`, {
    method: "GET",
    auth: true,
  })
}

export async function getMutualFollowings(userId: string, page = 1, pageSize = 20) {
  return apiRequest<PaginatedResponse<FollowUserItemDto>>(
    `/api/follow/${encodeURIComponent(userId)}/mutual-followings?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: true }
  )
}

export async function getFollowInsights(userId: string) {
  return apiRequest<FollowInsightsDto>(`/api/follow/${encodeURIComponent(userId)}/insights`, {
    method: "GET",
    auth: true,
  })
}
