"use client"

import Image from "next/image"
import Link from "next/link"
import { useEffect, useMemo, useState } from "react"
import { MapPin, Clock, Calendar } from "lucide-react"
import {
  formatLocalDateLong,
  formatLocalDateShort,
  parseStartTimeToLocalDateKey,
  toLocalDateKey,
} from "@/lib/showtimes/date-normalize"

export type ShowtimesMovieData = {
  id: number
  title: string
  posterUrl: string | null
  durationMinutes: number
  releaseYear: number | null
}

export type ShowtimesRow = {
  movieId: number
  cinema: string
  price: number
  screeningId: number
  startTimeIso: string
}

type Grouped = {
  cinema: string
  price: number
  screenings: Array<{ screeningId: number; time: string; dateKey: string; startTimeMs: number }>
}

function formatTimeFromIso(iso: string): string {
  const t = Date.parse(iso)
  if (Number.isNaN(t)) return "—"
  return new Date(t).toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit", hour12: true })
}

export function ShowtimesClient({ movies, rows }: { movies: ShowtimesMovieData[]; rows: ShowtimesRow[] }) {
  const moviesById = useMemo(() => new Map(movies.map((m) => [m.id, m])), [movies])
  const [nowMs, setNowMs] = useState<number>(() => Date.now())

  useEffect(() => {
    setNowMs(Date.now())
  }, [])

  const sortedDateKeys = useMemo(() => {
    const set = new Set<string>()
    for (const r of rows) {
      const k = parseStartTimeToLocalDateKey(r.startTimeIso)
      if (k) set.add(k)
    }
    return Array.from(set).sort()
  }, [rows])

  const todayKey = useMemo(() => toLocalDateKey(new Date(nowMs)), [nowMs])
  const selectableDateKeys = useMemo(() => {
    const out: string[] = []
    const base = new Date(nowMs)
    base.setHours(0, 0, 0, 0)
    for (let i = 0; i < 7; i += 1) {
      const d = new Date(base)
      d.setDate(base.getDate() + i)
      out.push(toLocalDateKey(d))
    }
    return out
  }, [nowMs])
  const [selectedDateKey, setSelectedDateKey] = useState(todayKey)

  useEffect(() => {
    if (!selectedDateKey || !selectableDateKeys.includes(selectedDateKey)) {
      setSelectedDateKey(todayKey)
    }
  }, [selectableDateKeys, selectedDateKey, todayKey])

  const filteredRows = useMemo(() => {
    return rows.filter((r) => {
      const rowDateKey = parseStartTimeToLocalDateKey(r.startTimeIso)
      if (rowDateKey !== selectedDateKey) return false
      const startTimeMs = Date.parse(r.startTimeIso)
      if (Number.isNaN(startTimeMs)) return false
      if (selectedDateKey === todayKey && startTimeMs < nowMs) return false
      return true
    })
  }, [rows, selectedDateKey, todayKey, nowMs])

  const groupedByMovie = useMemo(() => {
    const map = new Map<number, Map<string, Grouped>>()
    for (const r of filteredRows) {
      if (!map.has(r.movieId)) map.set(r.movieId, new Map())
      const key = `${r.cinema}-${r.price}`
      const groups = map.get(r.movieId)!
      if (!groups.has(key)) {
        groups.set(key, { cinema: r.cinema, price: r.price, screenings: [] })
      }
      const dk = parseStartTimeToLocalDateKey(r.startTimeIso) ?? selectedDateKey
      groups.get(key)!.screenings.push({
        screeningId: r.screeningId,
        time: formatTimeFromIso(r.startTimeIso),
        dateKey: dk,
        startTimeMs: Date.parse(r.startTimeIso),
      })
    }
    return map
  }, [filteredRows, selectedDateKey])

  const movieIdsOrdered = useMemo(() => {
    const ids = new Set(filteredRows.map((r) => r.movieId))
    return movies.filter((m) => ids.has(m.id)).map((m) => m.id)
  }, [filteredRows, movies])

  const friendlyDateLabel = (key: string) => {
    if (!key) return "Select date"
    const [y, m, d] = key.split("-").map(Number)
    if (!y || !m || !d) return key
    const date = new Date(y, m - 1, d)
    if (key === todayKey) return "Today"
    const tomorrow = new Date()
    tomorrow.setHours(0, 0, 0, 0)
    tomorrow.setDate(tomorrow.getDate() + 1)
    if (key === toLocalDateKey(tomorrow)) return "Tomorrow"
    return new Intl.DateTimeFormat("en-US", { weekday: "short", month: "short", day: "numeric" }).format(date)
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">Now Showing</p>
        <h1 className="font-serif text-3xl font-bold text-foreground md:text-4xl">Showtimes</h1>
        <p className="mt-2 text-sm text-muted-foreground">Pick a day to see what&apos;s playing.</p>
      </div>

      <div className="mb-8 rounded-2xl border border-border/50 bg-card p-4 sm:p-5">
        <label htmlFor="showtimes-date" className="mb-2 flex items-center gap-2 text-sm font-medium text-foreground">
          <Calendar className="h-4 w-4 text-primary" />
          Select date
        </label>
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:gap-6">
          <div className="relative w-full max-w-xs">
            <Calendar className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-primary" />
            <select
              id="showtimes-date"
              value={selectedDateKey}
              onChange={(e) => setSelectedDateKey(e.target.value)}
              className="w-full rounded-lg border border-border/50 bg-background py-2 pl-9 pr-3 text-sm text-foreground"
            >
              {selectableDateKeys.map((key) => (
                <option key={key} value={key}>
                  {friendlyDateLabel(key)}
                </option>
              ))}
            </select>
          </div>
          <div className="flex flex-1 flex-col gap-0.5">
            <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Showing</span>
            <span className="text-base font-semibold text-foreground">
              {selectedDateKey ? formatLocalDateLong(selectedDateKey) : "—"}
            </span>
          </div>
        </div>
      </div>

      {movieIdsOrdered.length === 0 ? (
        <div className="rounded-2xl border border-border/50 bg-card p-10 text-center">
          <p className="font-medium text-foreground">No showtimes for this date</p>
          <p className="mt-2 text-sm text-muted-foreground">
            No upcoming sessions for this date.
          </p>
        </div>
      ) : (
        <div className="flex flex-col gap-6">
          {movieIdsOrdered.map((movieId) => {
            const movie = moviesById.get(movieId)
            if (!movie) return null
            const groups = groupedByMovie.get(movieId)
            if (!groups) return null
            const movieShowtimes = Array.from(groups.values())

            return (
              <div
                key={movie.id}
                className="rounded-2xl border border-border/50 bg-card p-5 transition-colors hover:border-border"
              >
                <div className="flex gap-5">
                  <Link
                    href={`/movies/${movie.id}`}
                    className="relative aspect-[2/3] w-20 shrink-0 overflow-hidden rounded-lg"
                  >
                    <Image
                      src={movie.posterUrl || "/images/movie-1.jpg"}
                      alt={movie.title}
                      fill
                      className="object-cover"
                    />
                  </Link>
                  <div className="flex-1">
                    <Link
                      href={`/movies/${movie.id}`}
                      className="font-serif text-lg font-bold text-foreground transition-colors hover:text-primary"
                    >
                      {movie.title}
                    </Link>
                    <div className="mt-1 flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                      <span className="flex items-center gap-1">
                        <Clock className="h-3 w-3" />
                        {movie.durationMinutes} min
                      </span>
                      <span>{movie.releaseYear ?? "N/A"}</span>
                      <span className="flex items-center gap-1 text-foreground/90">
                        <Calendar className="h-3 w-3 text-primary" />
                        {formatLocalDateShort(selectedDateKey)}
                      </span>
                    </div>

                    <div className="mt-4 flex flex-col gap-3">
                      {movieShowtimes.map((st) => (
                        <div key={`${st.cinema}-${st.price}`}>
                          <div className="mb-1.5 flex flex-wrap items-center gap-1.5">
                            <MapPin className="h-3.5 w-3.5 shrink-0 text-primary" />
                            <span className="text-sm font-medium text-foreground">{st.cinema}</span>
                            <span className="text-xs text-muted-foreground">&middot; {st.price} AZN</span>
                          </div>
                          <div className="flex flex-wrap gap-2">
                            {st.screenings
                              .filter((screening) => Number.isFinite(screening.startTimeMs) && screening.startTimeMs >= nowMs)
                              .map((screening) => (
                                <Link
                                  key={screening.screeningId}
                                  href={`/seats?movie=${movie.id}&cinema=${encodeURIComponent(st.cinema)}&time=${encodeURIComponent(screening.time)}&screeningId=${screening.screeningId}`}
                                  className="flex min-w-[8rem] flex-col gap-0.5 rounded-lg border border-border/50 bg-secondary/50 px-3 py-1.5 text-xs font-medium text-foreground transition-all hover:border-primary/50 hover:bg-primary/10 hover:text-primary"
                                >
                                  <span className="text-[10px] uppercase tracking-wide text-muted-foreground">
                                    {formatLocalDateShort(screening.dateKey)}
                                  </span>
                                  <span className="flex items-center gap-1">
                                    <Clock className="h-3 w-3" />
                                    {screening.time}
                                  </span>
                                </Link>
                              ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
