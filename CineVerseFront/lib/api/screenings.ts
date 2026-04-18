import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export type ScreeningFormat = "TwoD" | "ThreeD" | "IMAX" | "FourDX"
export type ScreeningStatus = "Scheduled" | "Cancelled" | "Completed"

export interface GetAllScreeningsResponse {
  id: number
  movieId: number
  movieTitle: string
  hallId: number
  hallName: string
  startTime: string
  endTime: string
  price: number
  language: string
  subtitleLanguage?: string | null
  format: ScreeningFormat
  status: ScreeningStatus
  isActive: boolean
}

export interface GetAllScreeningsRequest {
  movieId?: number
  hallId?: number
  status?: ScreeningStatus
  format?: ScreeningFormat
  isActive?: boolean
  dateFrom?: string
  dateTo?: string
  pageNumber?: number
  pageSize?: number
}

function buildQuery(query: Record<string, string | number | boolean | undefined>) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === "") return
    params.set(key, String(value))
  })
  const queryString = params.toString()
  return queryString ? `?${queryString}` : ""
}

export async function getAllScreenings(query: GetAllScreeningsRequest = {}) {
  const qs = buildQuery({
    movieId: query.movieId,
    hallId: query.hallId,
    status: query.status ?? "Scheduled",
    format: query.format,
    isActive: query.isActive ?? true,
    dateFrom: query.dateFrom,
    dateTo: query.dateTo,
    pageNumber: query.pageNumber ?? 1,
    pageSize: query.pageSize ?? 20,
  })
  return apiRequest<PaginatedResponse<GetAllScreeningsResponse>>(`/api/screening${qs}`, {
    method: "GET",
    auth: false,
  })
}

export async function getScreeningById(id: number) {
  return apiRequest<GetAllScreeningsResponse>(`/api/screening/${id}`, {
    method: "GET",
    auth: false,
  })
}
