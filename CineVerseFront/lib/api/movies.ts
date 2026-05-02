import { apiRequest } from "@/lib/api/http"
import { ApiError } from "@/lib/api/types"

export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface GetAllMoviesResponse {
  id: number
  tmdbId: number
  title: string
  slug: string
  description?: string | null
  director?: string | null
  actors?: string | null
  cast?: string[] | string | null
  posterUrl?: string | null
  backdropUrl?: string | null
  language?: string | null
  country?: string | null
  releaseYear?: number | null
  releaseDate?: string | null
  tmdbRating?: number | null
  userAverageRating?: number | null
  status?: string | null
  durationMinutes: number
}

export interface MovieGenreDto {
  genreId: number
  name: string
  isPrimary: boolean
  order: number
}

/** Mirrors backend MovieVideo / TMDB YouTube key for embed. */
export interface MovieVideoDto {
  videoKey: string
  site: string
  type: string
  name: string
}

/** Prefer TMDB-style YouTube Trailer; otherwise first YouTube video with a key. */
export function pickYoutubeTrailerEmbedUrl(videos: MovieVideoDto[] | null | undefined): string | null {
  const list = videos ?? []
  if (list.length === 0) return null

  const site = (v: MovieVideoDto) => (v.site ?? "").trim().toLowerCase()
  const typ = (v: MovieVideoDto) => (v.type ?? "").trim().toLowerCase()
  const key = (v: MovieVideoDto) => (v.videoKey ?? "").trim()

  const isYoutube = (v: MovieVideoDto) => site(v) === "youtube" && key(v) !== ""
  const trailerFirst = list.find((v) => isYoutube(v) && typ(v) === "trailer")
  const anyYt = list.find((v) => isYoutube(v))
  const chosen = trailerFirst ?? anyYt
  const k = chosen ? key(chosen) : ""
  if (!k) return null
  return `https://www.youtube.com/embed/${encodeURIComponent(k)}`
}

export interface ReviewDto {
  id: number
  userId: string
  userName?: string | null
  content: string
  isEdited: boolean
  isDeleted: boolean
  createdAt: string
  updatedAt?: string | null
  isSpoiler: boolean
}

export interface GetMovieByIdResponse {
  id: number
  title: string
  description: string
  director?: string | null
  cast?: string[] | null
  posterUrl?: string | null
  backdropUrl?: string | null
  userAverageRating?: number | null
  myRating?: number | null
  ratingCount: number
  reviews: ReviewDto[]
  genres: MovieGenreDto[]
  videos?: MovieVideoDto[] | null
}

export interface GetSuggestedMoviesResponse {
  id: number
  title: string
  posterUrl?: string | null
  imdbRating: number
  releaseDate: number
  slug: string
  genres: string[]
}

export interface GetAllMoviesQuery {
  pageNumber?: number
  pageSize?: number
  search?: string
  actorName?: string
  directorName?: string
  /** MovieGenre / Genre PK — GET /api/movie?genreId= */
  genreId?: number
  language?: string
  year?: number
  sortBy?: "title" | "year" | "tmdbRating" | "userRating" | "createdAt"
  desc?: boolean
}

function buildQuery(query: Record<string, string | number | boolean | undefined>) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === "") return
    if (key === "year" && typeof value === "number" && Number.isFinite(value)) {
      params.set(key, String(Math.trunc(value)))
      return
    }
    params.set(key, String(value))
  })
  const queryString = params.toString()
  return queryString ? `?${queryString}` : ""
}

/** Only these `sortBy` values are valid on GET /api/movie (must match backend contract exactly). */
export const BACKEND_MOVIE_LIST_SORT_BY = [
  "title",
  "year",
  "tmdbRating",
  "userRating",
  "createdAt",
] as const

export type BackendMovieListSortBy = (typeof BACKEND_MOVIE_LIST_SORT_BY)[number]

/** Normalize any input to a backend-allowed `sortBy` (avoids 400 from invalid or mistyped values). */
export function toBackendMovieListSortBy(value: string | undefined | null): BackendMovieListSortBy {
  const v = (value ?? "").trim()
  if ((BACKEND_MOVIE_LIST_SORT_BY as readonly string[]).includes(v)) {
    return v as BackendMovieListSortBy
  }
  return "createdAt"
}

const CURRENT_YEAR = () => new Date().getFullYear()

function appendMovieListQueryParams(
  params: URLSearchParams,
  query: GetAllMoviesQuery
) {
  const pageNumber = query.pageNumber ?? 1
  const pageSize = query.pageSize ?? 24
  params.set("pageNumber", String(Math.max(1, Math.trunc(Number(pageNumber)) || 1)))
  params.set("pageSize", String(Math.min(100, Math.max(1, Math.trunc(Number(pageSize)) || 24))))

  if (query.search != null && String(query.search).trim() !== "") {
    params.set("search", String(query.search).trim())
  }
  if (query.actorName != null && String(query.actorName).trim() !== "") {
    const actor = String(query.actorName).trim()
    // Backend binds ?actor= / ?cast= to the same filter as actorName (see MovieController merge).
    params.set("actor", actor)
    params.set("actorName", actor)
  }
  if (query.directorName != null && String(query.directorName).trim() !== "") {
    const director = String(query.directorName).trim()
    params.set("director", director)
    params.set("directorName", director)
  }

  if (query.language != null && String(query.language).trim() !== "") {
    const lang = String(query.language).trim().slice(0, 10)
    if (lang) params.set("language", lang)
  }

  if (query.year != null && Number.isFinite(query.year)) {
    const y = Math.trunc(query.year)
    if (y >= 1888 && y <= CURRENT_YEAR()) {
      params.set("year", String(y))
    }
  }

  if (query.genreId != null && Number.isFinite(query.genreId)) {
    const gid = Math.trunc(query.genreId)
    if (gid > 0) {
      params.set("genreId", String(gid))
    }
  }

  const sortBy = toBackendMovieListSortBy(query.sortBy)
  params.set("sortBy", sortBy)

  const desc = query.desc !== false
  params.set("desc", desc ? "true" : "false")
}

/** UI language labels (Movies grid) → API `language` query (TMDB / DB uses ISO 639-1, max 10 chars). */
export const MOVIES_GRID_LANGUAGE_UI_TO_API: Record<string, string> = {
  English: "en",
  Spanish: "es",
  French: "fr",
  German: "de",
  Japanese: "ja",
  Korean: "ko",
  Russian: "ru",
  Mandarin: "zh",
  Arabic: "ar",
}

/** Map a single UI language label to the value sent on GET /api/movie?language= */
export function mapMoviesGridLanguageToApi(uiLabel: string | undefined): string | undefined {
  if (!uiLabel || uiLabel === "All") return undefined
  const mapped = MOVIES_GRID_LANGUAGE_UI_TO_API[uiLabel]
  if (mapped) return mapped
  const t = uiLabel.trim().toLowerCase()
  return t.length <= 10 ? t : t.slice(0, 10)
}

/**
 * Maps Movies grid `sortOptions.value` (internal state only — labels like "Top Rated" are never sent).
 * Values must be only `sortBy` / `desc` allowed by GET /api/movie.
 *
 * UI (unchanged): "rating" → "Top Rated", "year" → "Newest", "title" → "A-Z".
 */
export const MOVIES_GRID_SORT_VALUE_TO_QUERY = {
  /** Top Rated → TMDB score */
  rating: { sortBy: "tmdbRating" as const, desc: true },
  /** Newest → recently added / created in system */
  year: { sortBy: "createdAt" as const, desc: true },
  /** A–Z */
  title: { sortBy: "title" as const, desc: false },
} as const

export type MoviesGridSortValue = keyof typeof MOVIES_GRID_SORT_VALUE_TO_QUERY

export function getMoviesGridSortQuery(uiSortValue: string): {
  sortBy: BackendMovieListSortBy
  desc: boolean
} {
  const key = uiSortValue as MoviesGridSortValue
  if (key in MOVIES_GRID_SORT_VALUE_TO_QUERY) {
    const raw = MOVIES_GRID_SORT_VALUE_TO_QUERY[key]
    return { sortBy: toBackendMovieListSortBy(raw.sortBy), desc: raw.desc }
  }
  return { sortBy: "tmdbRating", desc: true }
}

/** Resolve release year from API fields (handles ISO date strings from DateOnly and missing data). */
export function getMovieReleaseYear(movie: GetAllMoviesResponse): number | null {
  if (movie.releaseYear != null && Number.isFinite(movie.releaseYear)) {
    return movie.releaseYear
  }
  const raw = movie.releaseDate
  if (raw == null || raw === "") return null
  const d = new Date(typeof raw === "string" ? raw : String(raw))
  const y = d.getFullYear()
  return Number.isFinite(y) && !Number.isNaN(y) ? y : null
}

export type GetAllMoviesOptions = {
  quiet?: boolean
  /** Send bearer token when true (e.g. admin list after authenticated mutations). */
  auth?: boolean
}

/** Matches backend list validation: page size 1–100. */
export const GET_ALL_MOVIES_MAX_PAGE_SIZE = 100

/**
 * Loads every movie from `GET /api/movie` by requesting successive pages (max page size).
 * De-duplicates by `id`, then sorts by `title` (case-insensitive) for dropdowns.
 */
export async function getAllMoviesAllPages(
  query: Omit<GetAllMoviesQuery, "pageNumber" | "pageSize"> = {},
  options?: GetAllMoviesOptions
): Promise<GetAllMoviesResponse[]> {
  const pageSize = GET_ALL_MOVIES_MAX_PAGE_SIZE
  const byId = new Map<number, GetAllMoviesResponse>()
  let pageNumber = 1
  const maxPages = 100

  while (pageNumber <= maxPages) {
    const page = await getAllMovies({ ...query, pageNumber, pageSize }, options)
    for (const m of page.items) {
      const id = Number(m.id)
      if (Number.isFinite(id)) byId.set(id, m)
    }
    if (page.items.length === 0 || !page.hasNextPage) break
    pageNumber++
  }

  return Array.from(byId.values()).sort((a, b) =>
    String(a.title ?? "").localeCompare(String(b.title ?? ""), undefined, { sensitivity: "base" }),
  )
}

function normalizeMovieListItem(m: GetAllMoviesResponse): GetAllMoviesResponse {
  const raw = m as unknown as Record<string, unknown>
  const id = Math.trunc(Number(raw.id ?? raw.Id ?? m.id ?? 0))
  const normalizedDirector = (() => {
    const v = raw.director ?? raw.Director ?? m.director
    return typeof v === "string" ? v : m.director
  })()
  const normalizedActors = (() => {
    const v = raw.actors ?? raw.Actors ?? m.actors
    return typeof v === "string" ? v : m.actors
  })()
  const normalizedCast = (raw.cast ?? raw.Cast ?? m.cast) as GetAllMoviesResponse["cast"]
  return {
    ...m,
    ...(Number.isFinite(id) && id > 0 ? { id } : {}),
    director: normalizedDirector ?? null,
    actors: normalizedActors ?? null,
    cast: normalizedCast ?? null,
  }
}

export async function getAllMovies(query: GetAllMoviesQuery = {}, options?: GetAllMoviesOptions) {
  const params = new URLSearchParams()
  appendMovieListQueryParams(params, query)
  // Unique URL every request so browser / Next rewrite proxy cannot reuse a stale GET (same path+query was cached).
  params.set("_cb", String(Date.now()))

  const qs = params.toString() ? `?${params.toString()}` : ""
  const url = `/api/movie${qs}`
  console.log("MOVIE API URL:", url)
  console.log("FILTERS:", {
    actor: query.actorName ?? "",
    director: query.directorName ?? "",
    search: query.search ?? "",
  })

  const data = await apiRequest<PaginatedResponse<GetAllMoviesResponse>>(url, {
    method: "GET",
    auth: options?.auth ?? false,
    quiet: options?.quiet,
    cache: "no-store",
    headers: {
      "Cache-Control": "no-cache",
      Pragma: "no-cache",
    },
  })

  // Defensive: always return an array (handles null items or PascalCase from non-camel JSON)
  const items =
    data.items ??
    (data as unknown as { Items?: GetAllMoviesResponse[] | null }).Items ??
    []
  const list = Array.isArray(items) ? items.map((row) => normalizeMovieListItem(row as GetAllMoviesResponse)) : []
  return {
    ...data,
    items: list,
  }
}

export async function getMovieById(id: number) {
  return apiRequest<GetMovieByIdResponse>(`/api/movie/${id}`, {
    method: "GET",
    auth: false,
    rawResponse: true,
  })
}

export async function getSuggestedMovies(pageNumber = 1, pageSize = 6) {
  const qs = buildQuery({ pageNumber, pageSize })
  const data = await apiRequest<PaginatedResponse<GetSuggestedMoviesResponse>>(`/api/movie/suggested${qs}`, {
    method: "GET",
    auth: true,
  })
  const items =
    data.items ??
    (data as unknown as { Items?: GetSuggestedMoviesResponse[] | null }).Items ??
    []
  return {
    ...data,
    items: Array.isArray(items) ? items : [],
  }
}

/** POST /api/movie — body matches `CreateMovieCommand` / nested `createMovieRequest`. */
export type CreateMovieBody = {
  title: string
  description: string
  country: string
  ageRating: string
  tagline: string
  releaseDate: string
  director: string
  durationMinutes: number
  language: string
  tmdbId: number
  genreIds?: number[]
}

export type MovieStatus = "Released" | "Upcoming" | "Cancelled" | "PostProduction"

export type UpdateMovieBody = {
  title?: string
  description?: string
  country?: string
  ageRating?: string
  tagline?: string
  releaseDate?: string
  director?: string
  durationMinutes?: number
  language?: string
  status?: MovieStatus
}

export async function createMovie(body: CreateMovieBody) {
  return apiRequest<unknown>("/api/movie", {
    method: "POST",
    auth: true,
    body: { createMovieRequest: body },
  })
}

export async function updateMovie(id: number, body: UpdateMovieBody) {
  return apiRequest<unknown>(`/api/movie/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteMovie(id: number | string) {
  const n = Math.trunc(Number(id))
  if (!Number.isFinite(n) || n <= 0) {
    throw new ApiError("Invalid movie id.", 400)
  }
  return apiRequest<unknown>(`/api/movie/${n}`, {
    method: "DELETE",
    auth: true,
    headers: {
      Accept: "application/json",
    },
  })
}

export async function syncMoviesFromTmdb(page: number) {
  return apiRequest<unknown>(`/api/movie/sync-tmdb?page=${page}`, {
    method: "POST",
    auth: true,
  })
}
