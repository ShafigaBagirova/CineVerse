import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export interface WatchRoomMemberDto {
  userId: string
  userName?: string | null
  joinedAt: string
}

export interface WatchRoomDto {
  id: number
  code: string
  hostUserId: string
  platform: string
  contentId: string
  contentUrl: string
  title?: string | null
  createdAt: string
  members: WatchRoomMemberDto[]
}

export interface WatchRoomListItemDto {
  id: number
  code: string
  hostUserId: string
  platform: string
  contentUrl: string
  title?: string | null
  memberCount: number
  createdAt: string
}

export interface YouTubeVideoDto {
  videoId: string
  title: string
  channelTitle: string
  thumbnailUrl: string
}

export interface CreateWatchRoomRequest {
  contentUrl: string
  title?: string
  startPositionSeconds?: number
  startPlaying?: boolean
}

export interface UpdateWatchRoomRequest {
  title?: string
}

export interface PlaybackState {
  isPlaying: boolean
  positionSeconds: number
  updatedAtUtc: string
}

export interface WatchRoomSnapshot {
  room: WatchRoomDto
  playback: PlaybackState
  onlineUserIds: string[]
}

export type WatchAction = "play" | "pause" | "seek"

export interface WatchStateChangedEvent {
  userId: string
  action: WatchAction
  playback: PlaybackState
}

export interface WatchHeartbeatEvent {
  userId: string
  playback: PlaybackState
}

export interface WatchChatMessage {
  roomCode: string
  senderId: string
  senderName?: string | null
  content: string
  sentAtUtc: string
}

export interface WatchReaction {
  roomCode: string
  senderId: string
  senderName?: string | null
  emoji: string
  sentAtUtc: string
}

export interface WatchMemberEvent {
  userId: string
  userName?: string | null
}

export async function createWatchRoom(request: CreateWatchRoomRequest) {
  return apiRequest<WatchRoomDto>("/api/WatchRoom", {
    method: "POST",
    auth: true,
    body: request,
  })
}

export async function joinWatchRoom(code: string) {
  return apiRequest<WatchRoomDto>(`/api/WatchRoom/${encodeURIComponent(code)}/join`, {
    method: "POST",
    auth: true,
  })
}

export async function getMyWatchRooms(pageNumber = 1, pageSize = 20) {
  return apiRequest<PaginatedResponse<WatchRoomListItemDto>>(
    `/api/WatchRoom?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      method: "GET",
      auth: true,
      cache: "no-store",
    }
  )
}

export async function getWatchRoomById(id: number) {
  return apiRequest<WatchRoomDto>(`/api/WatchRoom/${id}`, {
    method: "GET",
    auth: true,
    cache: "no-store",
  })
}

export async function updateWatchRoom(id: number, request: UpdateWatchRoomRequest) {
  return apiRequest<void>(`/api/WatchRoom/${id}`, {
    method: "PUT",
    auth: true,
    body: request,
  })
}

export async function deleteWatchRoom(id: number) {
  return apiRequest<void>(`/api/WatchRoom/${id}`, {
    method: "DELETE",
    auth: true,
  })
}

export interface YouTubePopularPageDto {
  items: YouTubeVideoDto[]
  nextPageToken: string | null
}

export async function getPopularYouTubeVideos(pageToken?: string | null) {
  const query = pageToken ? `?pageToken=${encodeURIComponent(pageToken)}` : ""
  return apiRequest<YouTubePopularPageDto>(`/api/WatchRoom/youtube/popular${query}`, {
    method: "GET",
    auth: true,
    quiet: true,
    cache: "no-store",
  })
}

export async function searchYouTubeVideos(query: string) {
  return apiRequest<YouTubeVideoDto[]>(`/api/WatchRoom/youtube/search?query=${encodeURIComponent(query)}`, {
    method: "GET",
    auth: true,
    quiet: true,
    cache: "no-store",
  })
}
