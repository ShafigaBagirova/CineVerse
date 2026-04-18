"use client"

import { useEffect, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { Star, Bookmark, Heart } from "lucide-react"
import { getMyWatchlist, removeFromWatchlist, type WatchlistMovieDto } from "@/lib/api/watch"
import { resolveMoviePosterUrl } from "@/lib/movie-poster"
import { useAuth } from "@/components/providers/auth-provider"

export default function WatchlistPage() {
  const { status } = useAuth()
  const [watchlistMovies, setWatchlistMovies] = useState<WatchlistMovieDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadWatchlist = async () => {
    if (status !== "authenticated") {
      setWatchlistMovies([])
      setLoading(false)
      return
    }
    try {
      setLoading(true)
      setError(null)
      const data = await getMyWatchlist(1, 50)
      setWatchlistMovies(data.items)
    } catch (err) {
      setWatchlistMovies([])
      setError(err instanceof Error ? err.message : "Failed to load watchlist.")
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadWatchlist()
  }, [status])

  const onRemove = async (movieId: number) => {
    try {
      await removeFromWatchlist(movieId)
      await loadWatchlist()
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to remove movie from watchlist.")
    }
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
          Your Collection
        </p>
        <h1 className="font-serif text-3xl font-bold text-foreground md:text-4xl">
          Watchlist
        </h1>
      </div>

      {status !== "authenticated" && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <Bookmark className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <p className="text-lg font-medium text-foreground">Sign in required</p>
          <p className="mt-1 text-sm text-muted-foreground">Please sign in to view your watchlist.</p>
        </div>
      )}
      {loading && status === "authenticated" && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <p className="text-lg font-medium text-foreground">Loading watchlist...</p>
        </div>
      )}
      {error && status === "authenticated" && !loading && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <p className="text-lg font-medium text-foreground">Could not load watchlist</p>
          <p className="mt-1 text-sm text-muted-foreground">{error}</p>
        </div>
      )}
      {!loading && !error && status === "authenticated" && (
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:gap-6 lg:grid-cols-5">
        {watchlistMovies.map((movie) => (
          <div key={movie.movieId} className="group relative">
            <Link href={`/movies/${movie.movieId}`}>
              <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                <Image
                  src={resolveMoviePosterUrl(movie.posterUrl)}
                  alt={movie.title}
                  fill
                  className="object-cover transition-transform duration-500 group-hover:scale-105"
                />
                <div className="absolute right-2 top-2 rounded-full bg-primary/90 p-1.5">
                  <Bookmark className="h-3.5 w-3.5 fill-primary-foreground text-primary-foreground" />
                </div>
                <div className="absolute inset-0 bg-gradient-to-t from-background/80 via-transparent to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
              </div>
            </Link>
            <h3 className="mt-3 text-sm font-semibold text-foreground">{movie.title}</h3>
            <div className="mt-0.5 flex items-center gap-1 text-xs text-muted-foreground">
              <Star className="h-3 w-3 fill-primary text-primary" />
              <span>{Number(movie.userAverageRating ?? 0).toFixed(1)}</span>
            </div>
            <button
              onClick={() => onRemove(movie.movieId)}
              className="mt-2 rounded-md border border-border/40 px-2 py-1 text-xs text-muted-foreground hover:bg-secondary"
            >
              Remove
            </button>
          </div>
        ))}
      </div>
      )}

      {!loading && !error && status === "authenticated" && watchlistMovies.length === 0 && (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <Bookmark className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <p className="text-lg font-medium text-foreground">Your watchlist is empty</p>
          <p className="mt-1 text-sm text-muted-foreground">Start adding movies you want to watch!</p>
          <Link
            href="/movies"
            className="mt-4 inline-flex h-10 items-center rounded-lg bg-primary px-6 text-sm font-semibold text-primary-foreground"
          >
            Browse Movies
          </Link>
        </div>
      )}
    </div>
  )
}
