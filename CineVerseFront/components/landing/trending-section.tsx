"use client"

import { useEffect, useState } from "react"
import Image from "next/image"
import Link from "next/link"
import { Star } from "lucide-react"
import { getAllMovies, type GetAllMoviesResponse } from "@/lib/api/movies"

export function TrendingSection() {
  const [movies, setMovies] = useState<GetAllMoviesResponse[]>([])

  useEffect(() => {
    let mounted = true
    const load = async () => {
      try {
        const response = await getAllMovies({ pageNumber: 1, pageSize: 6, sortBy: "userRating", desc: true })
        if (mounted) setMovies(response.items)
      } catch {
        if (mounted) setMovies([])
      }
    }
    void load()
    return () => {
      mounted = false
    }
  }, [])

  return (
    <section className="py-20">
      <div className="mx-auto max-w-7xl px-4 lg:px-8">
        <div className="mb-10 flex items-end justify-between">
          <div>
            <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
              Now Showing
            </p>
            <h2 className="font-serif text-3xl font-bold text-foreground md:text-4xl">
              Trending Movies
            </h2>
          </div>
          <Link
            href="/movies"
            className="text-sm font-medium text-muted-foreground transition-colors hover:text-primary"
          >
            View All
          </Link>
        </div>

        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:gap-6 lg:grid-cols-6">
          {movies.map((movie) => (
            <Link
              key={movie.id}
              href={`/movies/${movie.id}`}
              className="group relative"
            >
              <div className="relative aspect-[2/3] overflow-hidden rounded-xl">
                <Image
                  src={movie.posterUrl || "/images/movie-1.jpg"}
                  alt={movie.title}
                  fill
                  className="object-cover transition-transform duration-500 group-hover:scale-105"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-background/80 via-transparent to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
                <div className="absolute bottom-0 left-0 right-0 translate-y-4 p-3 opacity-0 transition-all duration-300 group-hover:translate-y-0 group-hover:opacity-100">
                  <div className="flex items-center gap-1">
                    <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                    <span className="text-xs font-semibold text-foreground">
                      {Number(movie.userAverageRating ?? movie.tmdbRating ?? 0).toFixed(1)}
                    </span>
                  </div>
                </div>
              </div>
              <div className="mt-3">
                <h3 className="text-sm font-semibold text-foreground transition-colors group-hover:text-primary">
                  {movie.title}
                </h3>
                <p className="mt-0.5 text-xs text-muted-foreground">{movie.releaseYear ?? "N/A"}</p>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </section>
  )
}
