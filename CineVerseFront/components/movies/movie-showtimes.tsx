import Link from "next/link"
import { MapPin, Clock, AlertCircle } from "lucide-react"

export interface Showtime {
  cinemaId: number
  cinema: string
  address?: string
  screenings: Array<{
    screeningId: number
    time: string
  }>
  price: number
  language: string
  format: string
}

export function MovieShowtimes({ movieId, showtimes }: { movieId: number; showtimes: Showtime[] }) {
  if (showtimes.length === 0) {
    return (
      <div className="rounded-2xl border border-border/20 bg-card p-6">
        <h2 className="mb-3 font-serif text-xl font-bold text-foreground">Showtimes</h2>
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <AlertCircle className="h-4 w-4" />
          <p>No showtimes available at this time.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="rounded-2xl border border-border/20 bg-card p-6">
      <h2 className="mb-5 font-serif text-xl font-bold text-foreground">Showtimes & Booking</h2>
      <div className="flex flex-col gap-5">
        {showtimes.map((st) => {
          return (
            <div key={`${st.cinemaId}-${st.price}-${st.language}-${st.format}`} className="rounded-lg border border-border/20 bg-surface p-4">
              <div className="mb-3 flex items-start justify-between">
                <div>
                  <h3 className="text-sm font-semibold text-foreground flex items-center gap-2">
                    <MapPin className="h-4 w-4 text-primary" />
                    {st.cinema}
                  </h3>
                  <p className="text-xs text-muted-foreground mt-1 ml-6">
                    {st.address || "Cinema address unavailable"} - {st.format} - {st.language}
                  </p>
                </div>
                <span className="text-xs font-medium text-primary bg-primary/10 px-2 py-1 rounded">{st.price} AZN</span>
              </div>
              <div className="flex flex-wrap gap-2">
                {st.screenings.map((screening) => (
                  <Link
                    key={screening.screeningId}
                    href={`/seats?movie=${movieId}&cinema=${encodeURIComponent(st.cinema)}&time=${encodeURIComponent(screening.time)}&screeningId=${screening.screeningId}`}
                    className="flex items-center gap-1.5 rounded-lg border border-primary/30 bg-primary/10 px-3 py-2 text-xs font-medium text-primary hover:bg-primary/20 hover:border-primary/50 transition-all"
                  >
                    <Clock className="h-3 w-3" />
                    {screening.time}
                  </Link>
                ))}
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
