import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export interface GetHallByIdResponse {
  id: number
  name: string
  cinemaId: number
}

export interface GetAllHallsResponse {
  id: number
  name: string
  cinemaId: number
  capacity: number
  isActive: boolean
}

export async function getHallById(id: number) {
  return apiRequest<GetHallByIdResponse>(`/api/hall/${id}`, {
    method: "GET",
    auth: false,
  })
}

/** Backend validation: page size must be 1–100 (see GetAllHallsQueryValidator). */
function clampHallPagination(pageNumber: number, pageSize: number) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber)) || 1)
  const ps = Math.min(100, Math.max(1, Math.trunc(Number(pageSize)) || 10))
  return { pageNumber: pn, pageSize: ps }
}

export type GetAllHallsOptions = {
  quiet?: boolean
  /** Backend allows: name, capacity, createdAt (see GetAllHallsQueryValidator). */
  sortBy?: string
  desc?: boolean
  cinemaId?: number
  search?: string
}

export async function getAllHalls(pageNumber = 1, pageSize = 100, options?: GetAllHallsOptions) {
  const { pageNumber: pn, pageSize: ps } = clampHallPagination(pageNumber, pageSize)
  const params = new URLSearchParams()
  params.set("pageNumber", String(pn))
  params.set("pageSize", String(ps))
  if (options?.cinemaId != null && Number.isFinite(options.cinemaId)) {
    params.set("cinemaId", String(Math.trunc(options.cinemaId)))
  }
  if (options?.search != null && String(options.search).trim() !== "") {
    params.set("search", String(options.search).trim())
  }
  if (options?.sortBy != null && String(options.sortBy).trim() !== "") {
    params.set("sortBy", String(options.sortBy).trim())
  }
  if (options?.desc !== undefined) {
    params.set("desc", options.desc ? "true" : "false")
  }
  const qs = params.toString()
  return apiRequest<PaginatedResponse<GetAllHallsResponse>>(`/api/hall?${qs}`, {
    method: "GET",
    auth: false,
    quiet: options?.quiet,
  })
}

export type CreateHallBody = {
  name: string
  cinemaId: number
  capacity: number
}

export type UpdateHallBody = {
  name?: string
  cinemaId?: number
  capacity?: number
}

export async function createHall(body: CreateHallBody) {
  return apiRequest<unknown>("/api/hall", {
    method: "POST",
    auth: true,
    body,
  })
}

export async function updateHall(id: number, body: UpdateHallBody) {
  return apiRequest<unknown>(`/api/hall/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteHall(id: number) {
  return apiRequest<unknown>(`/api/hall/${id}`, {
    method: "DELETE",
    auth: true,
    quiet: true,
  })
}
