"use client"

import Image from "next/image"
import { Star, Clock, Play, Heart, Eye, Bookmark } from "lucide-react"
import { useEffect, useState } from "react"
import { cn } from "@/lib/utils"
import type { GetMovieByIdResponse } from "@/lib/api/movies"
import { useAuth } from "@/components/providers/auth-provider"
import {
  addToWatchlist,
  getMyWatchedMovies,
  getMyWatchlist,
  markAsWatched,
  removeFromWatched,
  removeFromWatchlist,
} from "@/lib/api/watch"

interface MovieHeroModel {
  title: string
  description: string
  poster: string
  backdrop: string
  rating: number
  ratingCount: number
  genres: string[]
}

export function MovieHero({ movie }: { movie: MovieHeroModel | GetMovieByIdResponse }) {
  const { status } = useAuth()
  const [watchlisted, setWatchlisted] = useState(false)
  const [watched, setWatched] = useState(false)
  const [loadingState, setLoadingState] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const normalized = {
    id: "id" in movie ? movie.id : 0,
    title: movie.title,
    description: movie.description,
    poster: "poster" in movie ? movie.poster : movie.posterUrl || "/images/movie-1.jpg",
    backdrop: "backdrop" in movie ? movie.backdrop : movie.backdropUrl || movie.posterUrl || "/images/movie-1.jpg",
    rating:
      "rating" in movie
        ? movie.rating
        : Number(movie.userAverageRating ?? 0),
    ratingCount: "ratingCount" in movie ? movie.ratingCount : 0,
    genres:
      "genres" in movie && Array.isArray(movie.genres)
        ? (movie.genres as { name?: string }[]).map((g) => (typeof g === "string" ? g : g.name || "")).filter(Boolean)
        : [],
  }

  useEffect(() => {
    const load = async () => {
      if (status !== "authenticated" || !normalized.id) {
        setWatchlisted(false)
        setWatched(false)
        return
      }
      try {
        const [watchlist, watchedList] = await Promise.all([
          getMyWatchlist(1, 50),
          getMyWatchedMovies(1, 50),
        ])
        setWatchlisted(watchlist.items.some((item) => item.movieId === normalized.id))
        setWatched(watchedList.items.some((item) => item.movieId === normalized.id))
      } catch {
        setWatchlisted(false)
        setWatched(false)
      }
    }
    void load()
  }, [normalized.id, status])

  const onToggleWatchlist = async () => {
    if (status !== "authenticated") {
      setMessage("Sign in to manage watchlist.")
      return
    }
    try {
      setLoadingState(true)
      setMessage(null)
      if (watchlisted) {
        await removeFromWatchlist(normalized.id)
        setWatchlisted(false)
      } else {
        await addToWatchlist(normalized.id)
        // Move between lists: when adding back to watchlist, remove from watched.
        if (watched) {
          await removeFromWatched(normalized.id)
          setWatched(false)
        }
        setWatchlisted(true)
      }
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Watchlist action failed.")
    } finally {
      setLoadingState(false)
    }
  }

  const onToggleWatched = async () => {
    if (status !== "authenticated") {
      setMessage("Sign in to mark as watched.")
      return
    }
    try {
      setLoadingState(true)
      setMessage(null)
      if (watched) {
        await removeFromWatched(normalized.id)
        setWatched(false)
      } else {
        await markAsWatched(normalized.id)
        // Move between lists: watched item is removed from watchlist.
        if (watchlisted) {
          await removeFromWatchlist(normalized.id)
          setWatchlisted(false)
        }
        setWatched(true)
      }
    } catch (err) {
      setMessage(err instanceof Error ? err.message : "Watched action failed.")
    } finally {
      setLoadingState(false)
    }
  }

  return (
    <section className="relative">
      <div className="relative h-[50vh] overflow-hidden md:h-[60vh]">
        <Image
          src={normalized.backdrop}
          alt=""
          fill
          className="object-cover"
          priority
        />
        <div className="absolute inset-0 bg-gradient-to-t from-background via-background/60 to-background/30" />
      </div>

      <div className="relative z-10 mx-auto -mt-40 max-w-7xl px-4 lg:px-8">
        <div className="flex flex-col gap-8 md:flex-row">
          <div className="relative aspect-[2/3] w-48 shrink-0 overflow-hidden rounded-xl border-2 border-border/50 shadow-2xl md:w-56">
            <Image
              src={normalized.poster}
              alt={normalized.title}
              fill
              className="object-cover"
            />
          </div>

          <div className="flex-1 pt-2">
            <div className="mb-2 flex flex-wrap gap-2">
              {normalized.genres.map((g) => (
                <span
                  key={g}
                  className="rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary"
                >
                  {g}
                </span>
              ))}
            </div>

            <h1 className="font-serif text-3xl font-bold text-foreground md:text-5xl">
              {normalized.title}
            </h1>

            <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
              <span className="flex items-center gap-1">
                <Star className="h-4 w-4 fill-primary text-primary" />
                <span className="font-semibold text-foreground">{normalized.rating.toFixed(1)}</span>
                <span>/10</span>
              </span>
              <span className="flex items-center gap-1">
                <Clock className="h-4 w-4" />
                {normalized.ratingCount} ratings
              </span>
            </div>

            <p className="mt-4 max-w-2xl leading-relaxed text-muted-foreground">
              {normalized.description}
            </p>

            {/* Action buttons */}
            <div className="mt-6 flex flex-wrap gap-3">
              <button className="inline-flex h-10 items-center gap-2 rounded-lg bg-primary px-5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90">
                <Play className="h-4 w-4" />
                Watch Trailer
              </button>
              <button
                onClick={onToggleWatchlist}
                disabled={loadingState}
                className={cn(
                  "inline-flex h-10 items-center gap-2 rounded-lg border px-5 text-sm font-medium transition-colors",
                  watchlisted
                    ? "border-primary bg-primary/10 text-primary"
                    : "border-border bg-secondary/50 text-foreground hover:bg-secondary",
                  loadingState && "opacity-70"
                )}
              >
                <Bookmark className={cn("h-4 w-4", watchlisted && "fill-primary")} />
                {watchlisted ? "In Watchlist" : "Add to Watchlist"}
              </button>
              <button
                onClick={onToggleWatched}
                disabled={loadingState}
                className={cn(
                  "inline-flex h-10 items-center gap-2 rounded-lg border px-5 text-sm font-medium transition-colors",
                  watched
                    ? "border-primary bg-primary/10 text-primary"
                    : "border-border bg-secondary/50 text-foreground hover:bg-secondary",
                  loadingState && "opacity-70"
                )}
              >
                <Eye className={cn("h-4 w-4", watched && "text-primary")} />
                {watched ? "Watched" : "Mark as Watched"}
              </button>
            </div>
            {message && <p className="mt-3 text-xs text-muted-foreground">{message}</p>}

          </div>
        </div>
      </div>
    </section>
  )
}
