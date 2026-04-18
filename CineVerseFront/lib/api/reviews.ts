import { apiRequest } from "@/lib/api/http"

export interface ReviewDto {
  id: number
  movieId: number
  movieTitle?: string | null
  posterPath?: string | null
  userId: string
  userName?: string | null
  content: string
  isEdited: boolean
  isDeleted: boolean
  createdAt: string
  updatedAt?: string | null
  isSpoiler: boolean
}

export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages?: number
  hasPreviousPage?: boolean
  hasNextPage?: boolean
}

export interface CreateReviewRequest {
  content: string
  isSpoiler: boolean
}

export interface UpdateReviewRequest {
  content: string
  isSpoiler: boolean
}

interface OptionalFetchOptions {
  quiet?: boolean
}

export async function getReviewsByMovie(movieId: number, page = 1, pageSize = 10, options?: OptionalFetchOptions) {
  return apiRequest<PaginatedResponse<ReviewDto>>(
    `/api/review/${movieId}/reviews?page=${page}&pageSize=${pageSize}`,
    { method: "GET", auth: false, quiet: options?.quiet }
  )
}

export async function getMyReview(movieId: number, options?: OptionalFetchOptions) {
  return apiRequest<ReviewDto>(`/api/review/${movieId}/reviews/me`, {
    method: "GET",
    auth: true,
    quiet: options?.quiet,
  })
}

export async function createReview(movieId: number, request: CreateReviewRequest) {
  return apiRequest<unknown>(`/api/review/${movieId}/reviews`, {
    method: "POST",
    auth: true,
    body: request,
  })
}

export async function updateReview(movieId: number, request: UpdateReviewRequest) {
  return apiRequest<unknown>(`/api/review/${movieId}/reviews`, {
    method: "PUT",
    auth: true,
    body: request,
  })
}

export async function deleteReview(movieId: number) {
  return apiRequest<unknown>(`/api/review/${movieId}/reviews`, {
    method: "DELETE",
    auth: true,
  })
}
