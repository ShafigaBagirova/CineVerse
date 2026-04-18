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

export type GetAllCinemasOptions = { quiet?: boolean }

export async function getAllCinemas(pageNumber = 1, pageSize = 50, options?: GetAllCinemasOptions) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber)) || 1)
  const ps = Math.min(100, Math.max(1, Math.trunc(Number(pageSize)) || 10))
  return apiRequest<PaginatedResponse<GetAllCinemasResponse>>(
    `/api/cinema?pageNumber=${pn}&pageSize=${ps}`,
    { method: "GET", auth: false, quiet: options?.quiet }
  )
}

export async function getCinemaById(id: number) {
  return apiRequest<GetCinemaByIdResponse>(`/api/cinema/${id}`, {
    method: "GET",
    auth: false,
  })
}
