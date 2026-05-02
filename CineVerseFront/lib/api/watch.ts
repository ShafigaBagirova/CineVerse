import { apiRequest } from "@/lib/api/http"
import type { GetAllMoviesResponse } from "@/lib/api/movies"

export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages?: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

export interface WatchlistMovieDto {
  movieId: number
  title: string
  posterUrl?: string | null
  userAverageRating?: number | null
  addedAt: string
}

export interface WatchedMovieDto {
  movieId: number
  title: string
  posterPath?: string | null
  userAverageRating?: number | null
  createdAt: string
}

export async function addToWatchlist(movieId: number) {
  return apiRequest<unknown>(`/api/watchlistitem/${movieId}/watchlist`, {
    method: "POST",
    auth: true,
  })
}

export async function removeFromWatchlist(movieId: number) {
  return apiRequest<unknown>(`/api/watchlistitem/${movieId}/watchlist`, {
    method: "DELETE",
    auth: true,
  })
}

export async function getMyWatchlist(page = 1, pageSize = 50) {
  return apiRequest<PaginatedResponse<WatchlistMovieDto>>(
    `/api/watchlistitem/watchlist/me?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: true }
  )
}

export async function markAsWatched(movieId: number) {
  return apiRequest<unknown>(`/api/watchlog/${movieId}/watched`, {
    method: "POST",
    auth: true,
  })
}

export async function removeFromWatched(movieId: number) {
  return apiRequest<unknown>(`/api/watchlog/${movieId}/watched`, {
    method: "DELETE",
    auth: true,
  })
}

export async function getMyWatchedMovies(page = 1, pageSize = 50) {
  return apiRequest<PaginatedResponse<WatchedMovieDto>>(
    `/api/watchlog/watched/me?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: true }
  )
}

export async function getUserWatchlist(userId: string) {
  return apiRequest<GetAllMoviesResponse[]>(`/api/users/${encodeURIComponent(userId)}/watchlist`, {
    method: "GET",
    auth: false,
  })
}

export async function getUserWatchedMovies(userId: string) {
  return apiRequest<GetAllMoviesResponse[]>(`/api/users/${encodeURIComponent(userId)}/watched`, {
    method: "GET",
    auth: false,
  })
}
