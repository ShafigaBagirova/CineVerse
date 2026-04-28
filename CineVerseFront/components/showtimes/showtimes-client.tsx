"use client"

import Image from "next/image"
import Link from "next/link"
import { useCallback, useEffect, useMemo, useState } from "react"
import { MapPin, Clock, Calendar } from "lucide-react"
import { getAllScreeningsAllPages } from "@/lib/api/screenings"
import { getAllHalls } from "@/lib/api/halls"
import { getAllCinemas } from "@/lib/api/cinemas"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
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
  language?: string | null
}

export type ShowtimesRow = {
  movieId: number
  cinema: string
  price: number
  screeningId: number
  startTimeIso: string
  movieLanguage?: string | null
  screeningLanguage?: string | null
  subtitleLanguage?: string | null
  format?: string | null
}

type Grouped = {
  cinema: string
  price: number
  screenings: Array<{
    screeningId: number
    time: string
    dateKey: string
    startTimeMs: number
    movieLanguage?: string | null
    screeningLanguage?: string | null
    subtitleLanguage?: string | null
    format?: string | null
  }>
}

function normalizeLang(value: string | null | undefined): string | null {
  const t = (value ?? "").trim()
  return t.length > 0 ? t.toUpperCase() : null
}

function formatScreeningFormat(value: string | null | undefined): string | null {
  const t = (value ?? "").trim()
  if (!t) return null
  if (t === "TwoD") return "2D"
  if (t === "ThreeD") return "3D"
  if (t === "FourDX") return "4DX"
  return t
}

function formatTimeFromIso(iso: string): string {
  const t = Date.parse(iso)
  if (Number.isNaN(t)) return "—"
  return new Date(t).toLocaleTimeString("en-US", { hour: "2-digit", minute: "2-digit", hour12: true })
}

export function ShowtimesClient({ movies, rows }: { movies: ShowtimesMovieData[]; rows: ShowtimesRow[] }) {
  const moviesById = useMemo(() => new Map(movies.map((m) => [m.id, m])), [movies])
  const [mounted, setMounted] = useState(false)
  const [rowsState, setRowsState] = useState<ShowtimesRow[]>(rows)
  const [nowMs, setNowMs] = useState<number>(() => Date.now())
  const [audioFilter, setAudioFilter] = useState("all")
  const [subtitleFilter, setSubtitleFilter] = useState("all")
  const [dubbingFilter, setDubbingFilter] = useState("all")

  useEffect(() => {
    setMounted(true)
  }, [])

  useEffect(() => {
    setNowMs(Date.now())
  }, [])

  const sortedDateKeys = useMemo(() => {
    const set = new Set<string>()
    for (const r of rowsState) {
      const k = parseStartTimeToLocalDateKey(r.startTimeIso)
      if (k) set.add(k)
    }
    return Array.from(set).sort()
  }, [rowsState])

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

  const dateFilteredRows = useMemo(() => {
    return rowsState.filter((r) => {
      const rowDateKey = parseStartTimeToLocalDateKey(r.startTimeIso)
      if (rowDateKey !== selectedDateKey) return false
      const startTimeMs = Date.parse(r.startTimeIso)
      if (Number.isNaN(startTimeMs)) return false
      if (selectedDateKey === todayKey && startTimeMs < nowMs) return false
      return true
    })
  }, [rowsState, selectedDateKey, todayKey, nowMs])

  const refreshShowtimesData = useCallback(
    async (reason: "mount" | "focus" | "date-change") => {
      try {
        console.log("SHOWTIMES_REQUEST", {
          reason,
          selectedDate: selectedDateKey,
          audio: audioFilter,
          subtitles: subtitleFilter,
          dubbing: dubbingFilter,
        })
        const [screeningsRows, hallsResponse, cinemasResponse] = await Promise.all([
          getAllScreeningsAllPages(
            {
              isActive: true,
              status: "Scheduled",
            },
            { noCache: true, cacheBust: true }
          ),
          getAllHalls(1, 100, { sortBy: "name", desc: false, quiet: true }),
          getAllCinemas(1, 100, { sortBy: "name", desc: false, quiet: true }),
        ])

        const hallsById = new Map(hallsResponse.items.map((hall) => [hall.id, hall]))
        const cinemasById = new Map(cinemasResponse.items.map((cinema) => [cinema.id, cinema]))

        const refreshedRows: ShowtimesRow[] = []
        console.log("SHOWTIMES_RAW_RESPONSE", screeningsRows)
        screeningsRows.forEach((screening) => {
          const hall = hallsById.get(screening.hallId)
          if (!hall) return
          const cinema = cinemasById.get(hall.cinemaId)
          if (!cinema) return
          if (!moviesById.has(screening.movieId)) return

          refreshedRows.push({
            movieId: screening.movieId,
            cinema: cinema.name,
            price: Number(screening.price),
            screeningId: screening.id,
            startTimeIso: screening.startTime,
            movieLanguage: moviesById.get(screening.movieId)?.language ?? null,
            screeningLanguage: screening.language ?? null,
            subtitleLanguage: screening.subtitleLanguage ?? null,
            format: screening.format ?? null,
          })
        })

        console.log("SHOWTIMES_MAPPED", refreshedRows)
        setRowsState(refreshedRows)
      } catch (error) {
        console.error("SHOWTIMES_REFRESH_ERROR", error)
      }
    },
    [audioFilter, dubbingFilter, moviesById, selectedDateKey, subtitleFilter]
  )

  useEffect(() => {
    void refreshShowtimesData("mount")
  }, [refreshShowtimesData])

  useEffect(() => {
    void refreshShowtimesData("date-change")
  }, [selectedDateKey, refreshShowtimesData])

  useEffect(() => {
    const onFocus = () => {
      void refreshShowtimesData("focus")
    }
    window.addEventListener("focus", onFocus)
    return () => window.removeEventListener("focus", onFocus)
  }, [refreshShowtimesData])

  const availableAudioOptions = useMemo(() => {
    const set = new Set<string>()
    for (const r of dateFilteredRows) {
      const lang = normalizeLang(r.screeningLanguage)
      if (lang) set.add(lang)
    }
    return Array.from(set).sort()
  }, [dateFilteredRows])

  const availableSubtitleOptions = useMemo(() => {
    const set = new Set<string>()
    for (const r of dateFilteredRows) {
      const sub = normalizeLang(r.subtitleLanguage)
      if (sub) set.add(sub)
      else set.add("None")
    }
    return Array.from(set).sort()
  }, [dateFilteredRows])

  const availableDubbingOptions = useMemo(() => {
    const set = new Set<string>()
    for (const r of dateFilteredRows) {
      const original = normalizeLang(r.movieLanguage)
      const audio = normalizeLang(r.screeningLanguage)
      if (!original || !audio) continue
      if (original === audio) set.add("No")
      else set.add(`Yes (${audio})`)
    }
    return Array.from(set).sort()
  }, [dateFilteredRows])

  useEffect(() => {
    if (audioFilter !== "all" && !availableAudioOptions.includes(audioFilter)) setAudioFilter("all")
    if (subtitleFilter !== "all" && !availableSubtitleOptions.includes(subtitleFilter)) setSubtitleFilter("all")
    if (dubbingFilter !== "all" && !availableDubbingOptions.includes(dubbingFilter)) setDubbingFilter("all")
  }, [audioFilter, subtitleFilter, dubbingFilter, availableAudioOptions, availableSubtitleOptions, availableDubbingOptions])

  const filteredRows = useMemo(() => {
    return dateFilteredRows.filter((r) => {
      const audio = normalizeLang(r.screeningLanguage)
      const subtitles = normalizeLang(r.subtitleLanguage) ?? "None"
      const original = normalizeLang(r.movieLanguage)
      const dubbing =
        original && audio ? (original === audio ? "No" : `Yes (${audio})`) : null

      if (audioFilter !== "all" && audio !== audioFilter) return false
      if (subtitleFilter !== "all" && subtitles !== subtitleFilter) return false
      if (dubbingFilter !== "all" && dubbing !== dubbingFilter) return false
      return true
    })
  }, [dateFilteredRows, audioFilter, subtitleFilter, dubbingFilter])

  useEffect(() => {
    console.log("SHOWTIMES_FILTERED", filteredRows)
  }, [filteredRows])

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
        movieLanguage: r.movieLanguage ?? null,
        screeningLanguage: r.screeningLanguage ?? null,
        subtitleLanguage: r.subtitleLanguage ?? null,
        format: r.format ?? null,
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
            {mounted ? (
              <Select value={selectedDateKey} onValueChange={setSelectedDateKey}>
                <SelectTrigger
                  id="showtimes-date"
                  className="w-full rounded-xl border border-[#81D8D0]/50 bg-[#F0FFFD]/95 py-3 pr-10 pl-9 text-slate-800 shadow-sm transition-all hover:border-[#5ECFC5] focus-visible:border-[#81D8D0] focus-visible:ring-2 focus-visible:ring-[#81D8D0]/40"
                >
                  <SelectValue placeholder="Select date" />
                </SelectTrigger>
                <SelectContent className="z-50 mt-2 overflow-hidden rounded-xl border border-[#81D8D0]/40 bg-white/95 text-slate-800 shadow-lg backdrop-blur-md">
                  {selectableDateKeys.map((key) => (
                    <SelectItem
                      key={key}
                      value={key}
                      className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]"
                    >
                      {friendlyDateLabel(key)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <div className="h-[46px] w-full animate-pulse rounded-xl border border-[#81D8D0]/40 bg-[#F0FFFD]/70" />
            )}
          </div>
          <div className="flex flex-1 flex-col gap-0.5">
            <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Showing</span>
            <span className="text-base font-semibold text-foreground">
              {selectedDateKey ? formatLocalDateLong(selectedDateKey) : "—"}
            </span>
          </div>
        </div>
        <div className="mt-4 grid gap-3 sm:grid-cols-3">
          <div className="space-y-1">
            <label htmlFor="showtimes-audio" className="text-xs font-medium text-muted-foreground">
              Audio language
            </label>
            {mounted ? (
              <Select value={audioFilter} onValueChange={setAudioFilter}>
                <SelectTrigger
                  id="showtimes-audio"
                  className="w-full rounded-xl border border-[#81D8D0]/50 bg-[#F0FFFD]/95 px-4 py-3 text-slate-800 shadow-sm transition-all hover:border-[#5ECFC5] focus-visible:border-[#81D8D0] focus-visible:ring-2 focus-visible:ring-[#81D8D0]/40"
                >
                  <SelectValue placeholder="All audio languages" />
                </SelectTrigger>
                <SelectContent className="z-50 mt-2 overflow-hidden rounded-xl border border-[#81D8D0]/40 bg-white/95 text-slate-800 shadow-lg backdrop-blur-md">
                  <SelectItem className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]" value="all">
                    All audio languages
                  </SelectItem>
                  {availableAudioOptions.map((opt) => (
                    <SelectItem
                      key={opt}
                      value={opt}
                      className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]"
                    >
                      {opt}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <div className="h-[46px] w-full animate-pulse rounded-xl border border-[#81D8D0]/40 bg-[#F0FFFD]/70" />
            )}
          </div>
          <div className="space-y-1">
            <label htmlFor="showtimes-subs" className="text-xs font-medium text-muted-foreground">
              Subtitle language
            </label>
            {mounted ? (
              <Select value={subtitleFilter} onValueChange={setSubtitleFilter}>
                <SelectTrigger
                  id="showtimes-subs"
                  className="w-full rounded-xl border border-[#81D8D0]/50 bg-[#F0FFFD]/95 px-4 py-3 text-slate-800 shadow-sm transition-all hover:border-[#5ECFC5] focus-visible:border-[#81D8D0] focus-visible:ring-2 focus-visible:ring-[#81D8D0]/40"
                >
                  <SelectValue placeholder="All subtitles" />
                </SelectTrigger>
                <SelectContent className="z-50 mt-2 overflow-hidden rounded-xl border border-[#81D8D0]/40 bg-white/95 text-slate-800 shadow-lg backdrop-blur-md">
                  <SelectItem className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]" value="all">
                    All subtitles
                  </SelectItem>
                  {availableSubtitleOptions.map((opt) => (
                    <SelectItem
                      key={opt}
                      value={opt}
                      className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]"
                    >
                      {opt}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <div className="h-[46px] w-full animate-pulse rounded-xl border border-[#81D8D0]/40 bg-[#F0FFFD]/70" />
            )}
          </div>
          <div className="space-y-1">
            <label htmlFor="showtimes-dubbing" className="text-xs font-medium text-muted-foreground">
              Dubbing
            </label>
            {mounted ? (
              <Select value={dubbingFilter} onValueChange={setDubbingFilter}>
                <SelectTrigger
                  id="showtimes-dubbing"
                  className="w-full rounded-xl border border-[#81D8D0]/50 bg-[#F0FFFD]/95 px-4 py-3 text-slate-800 shadow-sm transition-all hover:border-[#5ECFC5] focus-visible:border-[#81D8D0] focus-visible:ring-2 focus-visible:ring-[#81D8D0]/40"
                >
                  <SelectValue placeholder="All dubbing options" />
                </SelectTrigger>
                <SelectContent className="z-50 mt-2 overflow-hidden rounded-xl border border-[#81D8D0]/40 bg-white/95 text-slate-800 shadow-lg backdrop-blur-md">
                  <SelectItem className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]" value="all">
                    All dubbing options
                  </SelectItem>
                  {availableDubbingOptions.map((opt) => (
                    <SelectItem
                      key={opt}
                      value={opt}
                      className="px-4 py-3 text-sm data-[state=checked]:bg-[#DFF7F5] data-[state=checked]:font-semibold data-[state=checked]:text-[#2C7A7B] hover:bg-[#E6FFFB] hover:text-[#2C7A7B]"
                    >
                      {opt}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            ) : (
              <div className="h-[46px] w-full animate-pulse rounded-xl border border-[#81D8D0]/40 bg-[#F0FFFD]/70" />
            )}
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
                                  <span className="mt-1 flex flex-wrap gap-1">
                                    {formatScreeningFormat(screening.format) && (
                                      <span className="rounded-full border border-[#8EDBD5] bg-[#DFF7F5] px-1.5 py-0.5 text-[10px] font-medium text-[#174A47]">
                                        {formatScreeningFormat(screening.format)}
                                      </span>
                                    )}
                                    {normalizeLang(screening.screeningLanguage) && (
                                      <span className="rounded-full border border-[#8EDBD5] bg-[#DFF7F5] px-1.5 py-0.5 text-[10px] font-medium text-[#174A47]">
                                        Audio: {normalizeLang(screening.screeningLanguage)}
                                      </span>
                                    )}
                                    {normalizeLang(screening.subtitleLanguage) && (
                                      <span className="rounded-full border border-[#8EDBD5] bg-[#DFF7F5] px-1.5 py-0.5 text-[10px] font-medium text-[#174A47]">
                                        Subtitles: {normalizeLang(screening.subtitleLanguage)}
                                      </span>
                                    )}
                                    {normalizeLang(screening.movieLanguage) && (
                                      <span className="rounded-full border border-[#8EDBD5] bg-[#DFF7F5] px-1.5 py-0.5 text-[10px] font-medium text-[#174A47]">
                                        Original: {normalizeLang(screening.movieLanguage)}
                                      </span>
                                    )}
                                    {normalizeLang(screening.movieLanguage) && normalizeLang(screening.screeningLanguage) && (
                                      <span className="rounded-full border border-[#8EDBD5] bg-[#DFF7F5] px-1.5 py-0.5 text-[10px] font-medium text-[#174A47]">
                                        {normalizeLang(screening.movieLanguage) === normalizeLang(screening.screeningLanguage)
                                          ? "Dubbed: No"
                                          : `Dubbed: ${normalizeLang(screening.screeningLanguage)} dub`}
                                      </span>
                                    )}
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
