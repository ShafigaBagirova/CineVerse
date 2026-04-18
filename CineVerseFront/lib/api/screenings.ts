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

/** Backend validation: page size must be 1–100 (see GetAllScreeningsQueryValidator). */
function clampScreeningPagination(pageNumber: number | undefined, pageSize: number | undefined) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const ps = Math.min(100, Math.max(1, Math.trunc(Number(pageSize ?? 10)) || 10))
  return { pageNumber: pn, pageSize: ps }
}

export type GetAllScreeningsOptions = { quiet?: boolean }

export async function getAllScreenings(query: GetAllScreeningsRequest = {}, options?: GetAllScreeningsOptions) {
  const { pageNumber: pn, pageSize: ps } = clampScreeningPagination(query.pageNumber, query.pageSize)
  const qs = buildQuery({
    movieId: query.movieId,
    hallId: query.hallId,
    status: query.status,
    format: query.format,
    isActive: query.isActive,
    dateFrom: query.dateFrom,
    dateTo: query.dateTo,
    pageNumber: pn,
    pageSize: ps,
  })
  return apiRequest<PaginatedResponse<GetAllScreeningsResponse>>(`/api/screening${qs}`, {
    method: "GET",
    auth: false,
    quiet: options?.quiet,
  })
}

export type CreateScreeningBody = {
  movieId: number
  hallId: number
  startTime: string
  endTime: string
  price: number
  language: string
  subtitleLanguage?: string | null
  format: ScreeningFormat
}

export type UpdateScreeningBody = {
  movieId?: number
  hallId?: number
  startTime?: string
  endTime?: string
  price?: number
  language?: string
  subtitleLanguage?: string | null
  format?: ScreeningFormat
  status?: ScreeningStatus
  isActive?: boolean
}

export async function createScreening(body: CreateScreeningBody) {
  return apiRequest<unknown>("/api/screening", {
    method: "POST",
    auth: true,
    body,
  })
}

export async function updateScreening(id: number, body: UpdateScreeningBody) {
  return apiRequest<unknown>(`/api/screening/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteScreening(id: number) {
  return apiRequest<unknown>(`/api/screening/${id}`, {
    method: "DELETE",
    auth: true,
  })
}

export async function getScreeningById(id: number) {
  return apiRequest<GetAllScreeningsResponse>(`/api/screening/${id}`, {
    method: "GET",
    auth: false,
  })
}
