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

export async function getAllHalls(pageNumber = 1, pageSize = 100) {
  return apiRequest<PaginatedResponse<GetAllHallsResponse>>(
    `/api/hall?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    { method: "GET", auth: false }
  )
}
