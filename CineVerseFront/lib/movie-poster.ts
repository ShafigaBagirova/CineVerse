/** TMDB poster base — matches Application/Common/Mappings/MovieProfile.cs */
const TMDB_POSTER_BASE = "https://image.tmdb.org/t/p/w500"

const DEFAULT_POSTER = "/images/movie-1.jpg"

/**
 * Watchlist DTO exposes `posterUrl` but the API stores the raw TMDB path.
 * Watched DTO uses `posterPath`. Movie list endpoints return full HTTPS URLs.
 */
export function resolveMoviePosterUrl(
  posterPathOrUrl: string | null | undefined,
  fallback: string = DEFAULT_POSTER
): string {
  if (posterPathOrUrl == null) return fallback
  const raw = posterPathOrUrl.trim()
  if (!raw) return fallback
  if (raw.startsWith("http://") || raw.startsWith("https://")) return raw
  return `${TMDB_POSTER_BASE}${raw.startsWith("/") ? raw : `/${raw}`}`
}
