import { apiRequest } from "@/lib/api/http"

export interface GetMovieRatingSummaryResponse {
  userAverageRating?: number | null
  ratingCount: number
  myRating?: number | null
}

export interface CreateMovieRatingRequest {
  rating: number
}

interface GetMovieRatingSummaryOptions {
  quiet?: boolean
}

export async function getMovieRatingSummary(movieId: number, options?: GetMovieRatingSummaryOptions) {
  return apiRequest<GetMovieRatingSummaryResponse>(
    `/api/rating/${movieId}/ratings/summary`,
    { method: "GET", auth: false, quiet: options?.quiet }
  )
}

export async function createMovieRating(movieId: number, request: CreateMovieRatingRequest) {
  return apiRequest<unknown>(`/api/rating/${movieId}/ratings`, {
    method: "POST",
    auth: true,
    body: request,
  })
}
