"use client"

import { MapPin, Clock, AlertCircle } from "lucide-react"
import Link from "next/link"
import type { Showtime } from "@/components/movies/movie-showtimes"

interface CinemaAvailabilityProps {
  movieId: number
  showtimes: Showtime[]
}

export function CinemaAvailability({ movieId, showtimes }: CinemaAvailabilityProps) {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-foreground">Cinema Availability</h3>
        <span className="text-xs text-primary font-medium">Today</span>
      </div>

      {showtimes.length === 0 ? (
        <div className="rounded-lg border border-border/20 bg-surface p-4 text-center">
          <AlertCircle className="h-5 w-5 text-muted-foreground mx-auto mb-2" />
          <p className="text-sm text-muted-foreground">No showtimes available</p>
        </div>
      ) : (
        <div className="space-y-3">
          {showtimes.map((cinema, index) => {
            return (
              <div
                key={cinema.screenings[0]?.screeningId ?? `${cinema.cinemaId}-${cinema.screenings[0]?.time ?? "na"}-${index}`}
                className="rounded-lg border border-border/20 bg-surface p-4 space-y-3"
              >
                <div className="flex items-start justify-between">
                  <div>
                    <h4 className="font-medium text-foreground flex items-center gap-2">
                      <MapPin className="h-4 w-4 text-primary" />
                      {cinema.cinema}
                    </h4>
                    <p className="text-xs text-muted-foreground mt-1">
                      Price: {cinema.price} AZN - {cinema.format} - {cinema.language}
                    </p>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  {cinema.screenings.map((screening) => (
                    <Link
                      key={screening.screeningId}
                      href={`/seats?movie=${movieId}&cinema=${encodeURIComponent(cinema.cinema)}&time=${encodeURIComponent(screening.time)}&screeningId=${screening.screeningId}`}
                      className="flex items-center gap-1.5 rounded-lg bg-primary/10 px-3 py-1.5 text-xs font-medium text-primary border border-primary/30 hover:bg-primary/20 transition-colors"
                    >
                      <Clock className="h-3.5 w-3.5" />
                      {screening.time}
                    </Link>
                  ))}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
