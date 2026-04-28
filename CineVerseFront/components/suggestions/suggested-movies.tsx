"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import Image from "next/image"
import { Star } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { getAllMovies, getSuggestedMovies, type GetAllMoviesResponse, type GetSuggestedMoviesResponse } from "@/lib/api/movies"
import { resolveMoviePosterUrl } from "@/lib/movie-poster"

interface SuggestedMoviesProps {
  excludeIds?: number[]
  title?: string
}

function getSuggestedPosterValue(movie: GetSuggestedMoviesResponse): string | null | undefined {
  const candidate = movie as GetSuggestedMoviesResponse & {
    posterPath?: string | null
    imageUrl?: string | null
    poster?: string | null
    backdropUrl?: string | null
  }
  return candidate.posterUrl ?? candidate.posterPath ?? candidate.imageUrl ?? candidate.poster ?? candidate.backdropUrl
}

export function SuggestedMovies({ excludeIds = [], title = "Recommended For You" }: SuggestedMoviesProps) {
  const { status, user } = useAuth()
  const isVip = Boolean(user?.roles?.includes("VIP"))
  const [suggested, setSuggested] = useState<GetSuggestedMoviesResponse[]>([])
  const [fallback, setFallback] = useState<GetAllMoviesResponse[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let mounted = true
    const load = async () => {
      try {
        setLoading(true)
        const response = await getSuggestedMovies(1, 10)
        if (mounted) {
          const list = (response.items ?? []).filter((m) => !excludeIds.includes(m.id)).slice(0, 6)
          setSuggested(list)
          setFallback([])
        }
      } catch {
        try {
          const response = await getAllMovies({ pageNumber: 1, pageSize: 10, sortBy: "userRating", desc: true })
          if (mounted) {
            setFallback(response.items.filter((m) => !excludeIds.includes(m.id)).slice(0, 6))
          }
        } catch {
          if (mounted) {
            setSuggested([])
            setFallback([])
          }
        }
      } finally {
        if (mounted) setLoading(false)
      }
    }
    void load()
    return () => {
      mounted = false
    }
  }, [excludeIds, isVip, status])

  const items = suggested.length > 0
    ? suggested.map((m) => ({
        id: m.id,
        title: m.title,
        poster: resolveMoviePosterUrl(getSuggestedPosterValue(m)),
        rating: m.imdbRating,
        year: m.releaseDate,
        genres: m.genres,
      }))
    : fallback.map((m) => ({
        id: m.id,
        title: m.title,
        poster: resolveMoviePosterUrl(m.posterUrl),
        rating: Number(m.userAverageRating ?? m.tmdbRating ?? 0),
        year: m.releaseYear ?? 0,
        genres: [] as string[],
      }))

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-foreground">{title}</h3>
        <Link href="/movies" className="text-sm text-primary hover:text-primary/80 transition-colors">
          View All
        </Link>
      </div>

      {loading ? (
        <div className="rounded-lg border border-border/20 bg-surface p-4 text-sm text-muted-foreground">
          Loading suggestions...
        </div>
      ) : items.length === 0 ? (
        <div className="rounded-lg border border-border/20 bg-surface p-4 text-sm text-muted-foreground">
          No recommendations yet. Watch and rate more movies to improve suggestions.
        </div>
      ) : (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {items.map(movie => (
          <Link
            key={movie.id}
            href={`/movies/${movie.id}`}
            className="group rounded-lg border border-border/20 bg-surface overflow-hidden hover:bg-surface-hover transition-all hover:border-primary/30"
          >
            <div className="relative aspect-video overflow-hidden bg-surface-hover">
              <Image
                src={movie.poster}
                alt={movie.title}
                fill
                className="object-cover group-hover:scale-105 transition-transform duration-300"
              />
              <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/20 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex items-end p-3">
                <p className="text-sm font-semibold text-white">{movie.title}</p>
              </div>
            </div>
            <div className="p-3">
              <div className="flex items-center justify-between mb-2">
                <h4 className="text-sm font-semibold text-foreground line-clamp-1">{movie.title}</h4>
              </div>
              <div className="flex items-center gap-2 mb-2">
                <div className="flex items-center gap-1">
                  <Star className="h-4 w-4 fill-primary text-primary" />
                  <span className="text-sm font-semibold text-foreground">{Number(movie.rating).toFixed(1)}</span>
                </div>
                <span className="text-xs text-muted-foreground">{movie.year}</span>
              </div>
              <div className="flex gap-1 flex-wrap">
                {movie.genres.slice(0, 2).map(genre => (
                  <span key={genre} className="text-xs rounded bg-primary/10 px-2 py-1 text-primary">
                    {genre}
                  </span>
                ))}
              </div>
            </div>
          </Link>
        ))}
      </div>
      )}
    </div>
  )
}
