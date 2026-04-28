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
    /** Avoid stale lists after POST/PUT/DELETE — browsers may cache GET /api/genre otherwise. */
    cache: "no-store",
  })
}

/** Mirrors Application.Genres.Dtos.CreateGenreRequest — JSON: { "name": string } (ASP.NET camelCase). */
export interface CreateGenreRequest {
  name: string
}

/** Matches UpdateGenreRequest. */
export interface UpdateGenreRequest {
  name: string
}

interface OptionalFetchOptions {
  quiet?: boolean
}

/** POST /api/genre — plain JSON body (not wrapped). ManageMovies. */
export async function createGenre(body: CreateGenreRequest, options?: OptionalFetchOptions) {
  const name = typeof body.name === "string" ? body.name.trim() : String(body.name ?? "").trim()
  return apiRequest<unknown>("/api/genre", {
    method: "POST",
    auth: true,
    body: { name },
    quiet: options?.quiet ?? true,
  })
}

export async function updateGenre(id: number, body: UpdateGenreRequest, options?: OptionalFetchOptions) {
  return apiRequest<unknown>(`/api/genre/${id}`, {
    method: "PUT",
    auth: true,
    body,
    quiet: options?.quiet,
  })
}

export async function deleteGenre(id: number, options?: OptionalFetchOptions) {
  return apiRequest<unknown>(`/api/genre/${id}`, {
    method: "DELETE",
    auth: true,
    quiet: options?.quiet,
  })
}
