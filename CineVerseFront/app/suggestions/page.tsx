"use client"

import Link from "next/link"
import Image from "next/image"
import { useEffect, useMemo, useState } from "react"
import { Crown, Star } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { FollowButton } from "@/components/follow/follow-button"
import {
  getSuggestedMovies,
  type GetSuggestedMoviesResponse,
} from "@/lib/api/movies"
import {
  getSuggestedUsers,
  suggestedUserTasteSimilarityPercent,
  type SuggestedUserItemDto,
} from "@/lib/api/follow"
import { resolveMoviePosterUrl } from "@/lib/movie-poster"

type SuggestedMovieExtra = GetSuggestedMoviesResponse & {
  posterPath?: string | null
  imageUrl?: string | null
  poster?: string | null
  backdropUrl?: string | null
  reason?: string | null
  score?: number | null
  similarityScore?: number | null
}

function moviePoster(movie: SuggestedMovieExtra): string {
  return resolveMoviePosterUrl(
    movie.posterUrl ??
      movie.posterPath ??
      movie.imageUrl ??
      movie.poster ??
      movie.backdropUrl
  )
}

export default function SuggestionsPage() {
  const { status, user } = useAuth()
  const roles = Array.isArray(user?.roles) ? user.roles : []
  const isVip = roles.some((role) => String(role ?? "").trim().toLowerCase() === "vip")

  const [movies, setMovies] = useState<SuggestedMovieExtra[]>([])
  const [users, setUsers] = useState<SuggestedUserItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true

    const load = async () => {
      if (status === "loading") {
        if (mounted) {
          setLoading(true)
        }
        return
      }
      if (status !== "authenticated") {
        if (mounted) {
          setLoading(false)
          setMovies([])
          setUsers([])
        }
        return
      }
      if (!isVip) {
        if (mounted) {
          setLoading(false)
          setMovies([])
          setUsers([])
        }
        return
      }

      try {
        setLoading(true)
        setError(null)
        const [movieRes, userRes] = await Promise.all([
          getSuggestedMovies(1, 18),
          getSuggestedUsers(1, 12),
        ])
        if (!mounted) return
        setMovies((movieRes.items ?? []) as SuggestedMovieExtra[])
        setUsers((userRes.items ?? []).filter((item) => item.userId !== user?.userId))
      } catch (err) {
        if (!mounted) return
        setMovies([])
        setUsers([])
        setError(err instanceof Error ? err.message : "Failed to load suggestions.")
      } finally {
        if (mounted) setLoading(false)
      }
    }

    void load()
    return () => {
      mounted = false
    }
  }, [isVip, status, user?.userId])

  const hasAny = useMemo(() => movies.length > 0 || users.length > 0, [movies.length, users.length])

  if (status === "loading") {
    return (
      <section className="mx-auto w-full max-w-7xl px-4 py-8 lg:px-8">
        <div className="rounded-xl border border-border/20 bg-surface p-6 text-sm text-muted-foreground">
          Loading suggestions...
        </div>
      </section>
    )
  }

  if (status !== "authenticated") {
    return (
      <section className="mx-auto w-full max-w-7xl px-4 py-8 lg:px-8">
        <div className="rounded-xl border border-border/20 bg-surface p-6 text-sm text-muted-foreground">
          Sign in to view your personalized suggestions.
        </div>
      </section>
    )
  }

  if (!isVip) {
    return (
      <section className="mx-auto w-full max-w-7xl px-4 py-8 lg:px-8">
        <div className="rounded-xl border border-border/20 bg-surface p-6">
          <h1 className="text-xl font-semibold text-foreground">VIP Suggestions</h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Suggestions are available for VIP members.
          </p>
          <Link
            href="/vip"
            className="mt-4 inline-flex rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Upgrade on VIP page
          </Link>
        </div>
      </section>
    )
  }

  return (
    <section className="mx-auto w-full max-w-7xl space-y-8 px-4 py-8 lg:px-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">Suggestions</h1>
      </div>

      {loading ? (
        <div className="rounded-xl border border-border/20 bg-surface p-6 text-sm text-muted-foreground">
          Loading suggestions...
        </div>
      ) : error ? (
        <div className="rounded-xl border border-border/20 bg-surface p-6 text-sm text-muted-foreground">
          {error}
        </div>
      ) : !hasAny ? (
        <div className="rounded-xl border border-border/20 bg-surface p-6 text-sm text-muted-foreground">
          No recommendations yet. Watch and rate more movies to improve suggestions.
        </div>
      ) : (
        <>
          <div className="space-y-4">
            <h2 className="text-lg font-semibold text-foreground">Recommended Movies</h2>
            {movies.length === 0 ? (
              <p className="rounded-lg border border-border/20 bg-surface p-4 text-sm text-muted-foreground">
                No movie suggestions right now.
              </p>
            ) : (
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
                {movies.map((movie) => {
                  const score = typeof movie.similarityScore === "number"
                    ? movie.similarityScore
                    : typeof movie.score === "number"
                      ? movie.score
                      : null
                  return (
                    <Link
                      key={movie.id}
                      href={`/movies/${movie.id}`}
                      className="group rounded-lg border border-border/20 bg-surface overflow-hidden hover:bg-surface-hover transition-all hover:border-primary/30"
                    >
                      <div className="relative aspect-[2/3] overflow-hidden bg-surface-hover">
                        <Image
                          src={moviePoster(movie)}
                          alt={movie.title}
                          fill
                          className="object-cover group-hover:scale-105 transition-transform duration-300"
                        />
                      </div>
                      <div className="p-3">
                        <h3 className="line-clamp-1 text-sm font-semibold text-foreground">{movie.title}</h3>
                        <div className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">
                          <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                          <span>{Number(movie.imdbRating ?? 0).toFixed(1)}</span>
                        </div>
                        {(movie.reason || score != null) && (
                          <p className="mt-2 line-clamp-2 text-xs text-muted-foreground">
                            {movie.reason ? movie.reason : `Score: ${Number(score).toFixed(1)}`}
                          </p>
                        )}
                      </div>
                    </Link>
                  )
                })}
              </div>
            )}
          </div>

          <div className="space-y-4">
            <h2 className="text-lg font-semibold text-foreground">Suggested Similar Users</h2>
            {users.length === 0 ? (
              <p className="rounded-lg border border-border/20 bg-surface p-4 text-sm text-muted-foreground">
                No user suggestions right now.
              </p>
            ) : (
              <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {users.map((person) => (
                  <div
                    key={person.userId}
                    className="rounded-lg border border-border/20 bg-surface p-4 hover:bg-surface-hover transition-colors"
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex items-center gap-3">
                        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground font-bold text-sm">
                          {(person.fullName || person.userName).slice(0, 2).toUpperCase()}
                        </div>
                        <div>
                          <h3 className="text-sm font-semibold text-foreground">{person.fullName || person.userName}</h3>
                          <p className="text-xs text-muted-foreground">@{person.userName}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-1 text-xs text-primary">
                        <Crown className="h-3.5 w-3.5" />
                        <span>{suggestedUserTasteSimilarityPercent(person)}%</span>
                      </div>
                    </div>
                    <div className="mt-3 flex items-center justify-between text-xs text-muted-foreground">
                      <span>Common movies: {person.commonMoviesCount}</span>
                      <span>Score: {Number(person.tasteScore ?? 0).toFixed(1)}</span>
                    </div>
                    <div className="mt-4">
                      <FollowButton
                        userId={person.userId}
                        displayLabel={person.userName ? `@${person.userName}` : person.fullName ?? undefined}
                        variant="full"
                      />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </section>
  )
}
