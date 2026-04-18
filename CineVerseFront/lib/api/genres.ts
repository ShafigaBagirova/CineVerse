import { apiRequest } from "@/lib/api/http"

/** Matches GET /api/genre (bare JSON array, not BaseResponse-wrapped). */
export interface GetAllGenresResponse {
  id: number
  name: string
}

export async function getAllGenres() {
  return apiRequest<GetAllGenresResponse[]>("/api/genre", {
    method: "GET",
    auth: false,
    rawResponse: true,
  })
}
