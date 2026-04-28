import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export interface GetAllCinemasResponse {
  id: number
  name: string
  address: string
  phone?: string | null
  country: string
  city: string
}

export interface GetCinemaByIdResponse {
  id: number
  name: string
  description?: string | null
  address: string
  city: string
  country: string
  phone?: string | null
  email?: string | null
  isActive: boolean
}

export interface CreateCinemaRequest {
  name: string
  description?: string
  address: string
  city: string
  country: string
  phone?: string
  email?: string
}

export interface UpdateCinemaRequest {
  name?: string
  description?: string
  address?: string
  phone?: string
  email?: string
}

export type GetAllCinemasOptions = {
  quiet?: boolean
  auth?: boolean
  cacheBust?: boolean
}

export async function getAllCinemas(pageNumber = 1, pageSize = 50, options?: GetAllCinemasOptions) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber)) || 1)
  const ps = Math.min(100, Math.max(1, Math.trunc(Number(pageSize)) || 10))
  const cb = options?.cacheBust ? `&_cb=${Date.now()}` : ""
  return apiRequest<PaginatedResponse<GetAllCinemasResponse>>(
    `/api/cinema?pageNumber=${pn}&pageSize=${ps}${cb}`,
    {
      method: "GET",
      auth: options?.auth ?? false,
      quiet: options?.quiet,
      cache: "no-store",
      headers: {
        "Cache-Control": "no-cache",
        Pragma: "no-cache",
      },
    }
  )
}

export async function getCinemaById(id: number) {
  return apiRequest<GetCinemaByIdResponse>(`/api/cinema/${id}`, {
    method: "GET",
    auth: false,
  })
}

export async function createCinema(body: CreateCinemaRequest) {
  return apiRequest<unknown>("/api/cinema", {
    method: "POST",
    auth: true,
    body,
  })
}

export async function updateCinema(id: number, body: UpdateCinemaRequest) {
  return apiRequest<unknown>(`/api/cinema/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteCinema(id: number) {
  return apiRequest<unknown>(`/api/cinema/${id}`, {
    method: "DELETE",
    auth: true,
  })
}
