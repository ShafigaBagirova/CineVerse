"use client"

import { useState, useMemo, useRef, useEffect } from "react"
import Image from "next/image"
import Link from "next/link"
import { Star, SlidersHorizontal, X, ChevronDown, Search, ChevronLeft, ChevronRight } from "lucide-react"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { getAllGenres } from "@/lib/api/genres"
import {
  getAllMovies,
  getMovieReleaseYear,
  getMoviesGridSortQuery,
  mapMoviesGridLanguageToApi,
  type GetAllMoviesResponse,
} from "@/lib/api/movies"
const sortOptions = [
  { value: "rating", label: "Top Rated" },
  { value: "year", label: "Newest" },
  { value: "title", label: "A-Z" },
]
const releaseYears = Array.from({ length: 30 }, (_, i) => new Date().getFullYear() - i)
const languages = ["All", "English", "Spanish", "French", "German", "Japanese", "Korean", "Russian", "Mandarin", "Arabic"]

export function MoviesGrid({
  initialSearch = "",
  initialActor = "",
  initialDirector = "",
}: {
  initialSearch?: string
  initialActor?: string
  initialDirector?: string
}) {
  const PAGE_SIZE = 32
  const [genreOptions, setGenreOptions] = useState<{ id: number; name: string }[]>([])
  const [selectedGenreId, setSelectedGenreId] = useState<number | null>(null)
  const [sortBy, setSortBy] = useState("rating")
  const [showFilters, setShowFilters] = useState(false)
  const [yearFrom, setYearFrom] = useState(1990)
  const [yearTo, setYearTo] = useState(new Date().getFullYear())
  const [selectedLanguages, setSelectedLanguages] = useState<string[]>([])
  const [languageDropdownOpen, setLanguageDropdownOpen] = useState(false)
  const [languageSearch, setLanguageSearch] = useState("")
  const languageDropdownRef = useRef<HTMLDivElement>(null)
  const [movies, setMovies] = useState<GetAllMoviesResponse[]>([])
  const [pageNumber, setPageNumber] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const actorFromQuery = initialActor.trim()
  const directorFromQuery = initialDirector.trim()
  const [search, setSearch] = useState((actorFromQuery || directorFromQuery || initialSearch).trim())
  const [activeActorFilter, setActiveActorFilter] = useState(actorFromQuery)
  const [activeDirectorFilter, setActiveDirectorFilter] = useState(directorFromQuery)

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (languageDropdownRef.current && !languageDropdownRef.current.contains(event.target as Node)) {
        setLanguageDropdownOpen(false)
      }
    }
    document.addEventListener("mousedown", handleClickOutside)
    return () => document.removeEventListener("mousedown", handleClickOutside)
  }, [])

  // Toggle language selection
  const toggleLanguage = (lang: string) => {
    setSelectedLanguages(prev =>
      prev.includes(lang)
        ? prev.filter(l => l !== lang)
        : [...prev, lang]
    )
  }

  // Filter languages based on search
  const filteredLanguages = languages.filter(lang => 
    lang.toLowerCase().includes(languageSearch.toLowerCase())
  )

  const normalizedMovies = useMemo(
    () =>
      movies.map((movie) => ({
        id: movie.id,
        title: movie.title,
        director: String(movie.director ?? "").trim(),
        actors: String(movie.actors ?? "").trim(),
        cast: movie.cast,
        poster: movie.posterUrl || "/images/movie-1.jpg",
        year: getMovieReleaseYear(movie),
        runtime: movie.durationMinutes > 0 ? `${Math.floor(movie.durationMinutes / 60)}h ${movie.durationMinutes % 60}m` : "N/A",
        rating: movie.userAverageRating ?? movie.tmdbRating ?? 0,
        genres: [] as string[],
        language: movie.language ?? "Unknown",
        // Compare using same ISO-style codes we send in ?language= (DB stores en, es, … not "English").
        languageCode: (movie.language ?? "").trim().toLowerCase(),
      })),
    [movies]
  )

  const selectedLanguageCodes = useMemo(
    () =>
      selectedLanguages
        .filter((l) => l !== "All")
        .map((l) => mapMoviesGridLanguageToApi(l) ?? l.trim().toLowerCase()),
    [selectedLanguages]
  )

  const sortedGenreOptions = useMemo(
    () => [...genreOptions].sort((a, b) => a.name.localeCompare(b.name)),
    [genreOptions]
  )

  const selectedGenreName = useMemo(
    () => (selectedGenreId == null ? null : genreOptions.find((g) => g.id === selectedGenreId)?.name ?? null),
    [genreOptions, selectedGenreId]
  )

  useEffect(() => {
    let mounted = true
    const loadGenres = async () => {
      try {
        const list = await getAllGenres()
        if (!mounted) return
        setGenreOptions(list.map((g) => ({ id: g.id, name: g.name })))
      } catch {
        if (mounted) setGenreOptions([])
      }
    }
    void loadGenres()
    return () => {
      mounted = false
    }
  }, [])

  useEffect(() => {
    setPageNumber(1)
  }, [sortBy, selectedLanguages, yearFrom, yearTo, selectedGenreId, search])

  useEffect(() => {
    const controller = new AbortController()
    const loadMovies = async () => {
      try {
        setLoading(true)
        setError(null)

        const { sortBy: sortByQuery, desc: sortDesc } = getMoviesGridSortQuery(sortBy)

        const primaryLanguage = selectedLanguages.length === 1 ? selectedLanguages[0] : undefined
        const yearQuery =
          yearFrom === yearTo ? Math.trunc(Number(yearFrom)) : undefined

        const response = await getAllMovies({
          pageNumber,
          pageSize: PAGE_SIZE,
          search: search.trim() !== "" ? search.trim() : undefined,
          actorName: activeActorFilter || undefined,
          directorName: activeDirectorFilter || undefined,
          genreId: selectedGenreId != null && selectedGenreId > 0 ? selectedGenreId : undefined,
          language:
            selectedLanguages.length === 1 ? mapMoviesGridLanguageToApi(primaryLanguage) : undefined,
          year: yearQuery !== undefined && Number.isFinite(yearQuery) ? yearQuery : undefined,
          sortBy: sortByQuery,
          desc: sortDesc,
        })

        if (!controller.signal.aborted) {
          setMovies(response.items)
          setTotalPages(Math.max(1, Number(response.totalPages ?? 1) || 1))
          setTotalCount(Math.max(0, Number(response.totalCount ?? 0) || 0))
        }
      } catch (err) {
        if (!controller.signal.aborted) {
          setError(err instanceof Error ? err.message : "Failed to load movies.")
          setMovies([])
          setTotalPages(1)
          setTotalCount(0)
        }
      } finally {
        if (!controller.signal.aborted) {
          setLoading(false)
        }
      }
    }

    void loadMovies()
    return () => controller.abort()
  }, [sortBy, selectedLanguages, yearFrom, yearTo, selectedGenreId, search, activeActorFilter, activeDirectorFilter, pageNumber])

  const filtered = useMemo(() => {
    let result = [...normalizedMovies]

    // Genre is filtered server-side via genreId on GET /api/movie (local rows had empty genres: []).
    // Unknown/missing release year must not hide rows (using 0 made every such movie fail the default year range).
    result = result.filter((m) => {
      if (m.year == null || Number.isNaN(m.year)) return true
      return m.year >= yearFrom && m.year <= yearTo
    })
    if (selectedLanguageCodes.length > 0) {
      result = result.filter((m) => selectedLanguageCodes.includes(m.languageCode))
    }
    return result
  }, [normalizedMovies, yearFrom, yearTo, selectedLanguageCodes])

  const paginationItems = useMemo(() => {
    if (totalPages <= 1) return [1]
    const pages: Array<number | string> = []
    const start = Math.max(1, pageNumber - 1)
    const end = Math.min(totalPages, pageNumber + 1)

    pages.push(1)
    if (start > 2) pages.push("left-ellipsis")
    for (let p = Math.max(2, start); p <= Math.min(totalPages - 1, end); p++) {
      pages.push(p)
    }
    if (end < totalPages - 1) pages.push("right-ellipsis")
    if (totalPages > 1) pages.push(totalPages)
    return pages
  }, [pageNumber, totalPages])

  return (
    <div className="flex flex-col gap-6 lg:flex-row">
      {/* Desktop Sidebar */}
      <aside className="hidden w-56 shrink-0 lg:block">
        <div className="sticky top-24 max-h-[calc(100vh-180px)] overflow-y-auto rounded-2xl border border-border/50 bg-card p-5 pr-3">
          <h3 className="mb-4 text-sm font-semibold text-foreground">Filters</h3>

          <div className="mb-6">
            <p className="mb-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">Genre</p>
            <div className="flex flex-col gap-1">
              <button
                type="button"
                onClick={() => setSelectedGenreId(null)}
                className={cn(
                  "rounded-lg px-3 py-1.5 text-left text-sm transition-colors",
                  selectedGenreId == null
                    ? "bg-primary/10 font-medium text-primary"
                    : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                )}
              >
                All
              </button>
              {sortedGenreOptions.map((genre) => (
                <button
                  type="button"
                  key={genre.id}
                  onClick={() => setSelectedGenreId(genre.id)}
                  className={cn(
                    "rounded-lg px-3 py-1.5 text-left text-sm transition-colors",
                    selectedGenreId === genre.id
                      ? "bg-primary/10 font-medium text-primary"
                      : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                  )}
                >
                  {genre.name}
                </button>
              ))}
            </div>
          </div>

          <div className="mb-6">
            <p className="mb-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">Sort By</p>
            <div className="flex flex-col gap-1">
              {sortOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => setSortBy(option.value)}
                  className={cn(
                    "rounded-lg px-3 py-1.5 text-left text-sm transition-colors",
                    sortBy === option.value
                      ? "bg-primary/10 font-medium text-primary"
                      : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                  )}
                >
                  {option.label}
                </button>
              ))}
            </div>
          </div>

          <div className="mb-6">
            <p className="mb-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">Release Year</p>
            <div className="flex gap-2">
              <select
                value={yearFrom}
                onChange={(e) => setYearFrom(Number(e.target.value))}
                className="flex-1 rounded-lg border border-border/50 bg-secondary px-2.5 py-1.5 text-xs text-foreground transition-colors hover:border-primary/50 hover:bg-surface focus:outline-none focus:ring-1 focus:ring-primary"
              >
                {releaseYears.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
              <select
                value={yearTo}
                onChange={(e) => setYearTo(Number(e.target.value))}
                className="flex-1 rounded-lg border border-border/50 bg-secondary px-2.5 py-1.5 text-xs text-foreground transition-colors hover:border-primary/50 hover:bg-surface focus:outline-none focus:ring-1 focus:ring-primary"
              >
                {releaseYears.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <p className="mb-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">Language</p>
            
            {/* Selected Language Tags */}
            {selectedLanguages.length > 0 && (
              <div className="mb-3 flex flex-wrap gap-2">
                {selectedLanguages.map((lang) => (
                  <button
                    key={lang}
                    onClick={() => toggleLanguage(lang)}
                    className="flex items-center gap-1.5 rounded-full bg-primary/20 px-2.5 py-1 text-xs font-medium text-primary hover:bg-primary/30 transition-colors"
                  >
                    {lang}
                    <X className="h-3 w-3" />
                  </button>
                ))}
              </div>
            )}

            {/* Language Dropdown */}
            <div ref={languageDropdownRef} className="relative">
              <button
                onClick={() => setLanguageDropdownOpen(!languageDropdownOpen)}
                className="w-full flex items-center justify-between rounded-lg border border-border/50 bg-secondary px-3 py-2 text-sm text-foreground transition-all hover:border-primary/50 hover:bg-surface focus:outline-none focus:ring-1 focus:ring-primary"
              >
                <span className="text-xs">
                  {selectedLanguages.length > 0 
                    ? `${selectedLanguages.length} selected` 
                    : "Select languages..."}
                </span>
                <ChevronDown className={cn("h-4 w-4 transition-transform", languageDropdownOpen && "rotate-180")} />
              </button>

              {/* Dropdown Menu */}
              {languageDropdownOpen && (
                <div className="absolute top-full left-0 right-0 z-10 mt-2 rounded-lg border border-border/50 bg-card shadow-lg animate-in fade-in-0 zoom-in-95 duration-200">
                  {/* Search Input */}
                  <div className="border-b border-border/30 p-2">
                    <div className="relative">
                      <Search className="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted-foreground" />
                      <input
                        type="text"
                        placeholder="Search languages..."
                        value={languageSearch}
                        onChange={(e) => setLanguageSearch(e.target.value)}
                        className="w-full rounded-lg border border-border/50 bg-secondary pl-7 pr-3 py-1.5 text-xs text-foreground placeholder-muted-foreground transition-colors hover:border-primary/50 focus:outline-none focus:ring-1 focus:ring-primary"
                        autoFocus
                      />
                    </div>
                  </div>

                  {/* Language List */}
                  <div className="max-h-64 overflow-y-auto p-2">
                    {filteredLanguages.length > 0 ? (
                      filteredLanguages.map((lang) => (
                        <button
                          key={lang}
                          onClick={() => toggleLanguage(lang)}
                          className={cn(
                            "w-full flex items-center gap-2 rounded-lg px-3 py-2 text-left text-xs transition-colors",
                            selectedLanguages.includes(lang)
                              ? "bg-primary/10 font-medium text-primary"
                              : "text-muted-foreground hover:bg-secondary hover:text-foreground"
                          )}
                        >
                          <input
                            type="checkbox"
                            checked={selectedLanguages.includes(lang)}
                            onChange={() => {}}
                            className="h-3.5 w-3.5 rounded border-border accent-primary cursor-pointer"
                          />
                          {lang}
                        </button>
                      ))
                    ) : (
                      <div className="px-3 py-4 text-center text-xs text-muted-foreground">
                        No languages found
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </aside>

      {/* Mobile Filter Toggle */}
      <div className="flex items-center gap-3 lg:hidden">
        <button
          onClick={() => setShowFilters(!showFilters)}
          className="flex items-center gap-2 rounded-lg border border-border/50 bg-card px-4 py-2 text-sm text-foreground"
        >
          <SlidersHorizontal className="h-4 w-4" />
          Filters
        </button>
        {selectedGenreId != null && selectedGenreName && (
          <button
            type="button"
            onClick={() => setSelectedGenreId(null)}
            className="flex items-center gap-1 rounded-full bg-primary/10 px-3 py-1 text-xs font-medium text-primary"
          >
            {selectedGenreName}
            <X className="h-3 w-3" />
          </button>
        )}
      </div>

      {showFilters && (
        <div className="rounded-2xl border border-border/50 bg-card p-4 lg:hidden space-y-4">
          <div>
            <p className="mb-2 text-xs font-medium text-muted-foreground">Genre</p>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => {
                  setSelectedGenreId(null)
                  setShowFilters(false)
                }}
                className={cn(
                  "rounded-full px-3 py-1 text-xs font-medium transition-colors",
                  selectedGenreId == null
                    ? "bg-primary text-primary-foreground"
                    : "bg-secondary text-muted-foreground hover:text-foreground"
                )}
              >
                All
              </button>
              {sortedGenreOptions.map((genre) => (
                <button
                  type="button"
                  key={genre.id}
                  onClick={() => {
                    setSelectedGenreId(genre.id)
                    setShowFilters(false)
                  }}
                  className={cn(
                    "rounded-full px-3 py-1 text-xs font-medium transition-colors",
                    selectedGenreId === genre.id
                      ? "bg-primary text-primary-foreground"
                      : "bg-secondary text-muted-foreground hover:text-foreground"
                  )}
                >
                  {genre.name}
                </button>
              ))}
            </div>
          </div>
          <div>
            <p className="mb-2 text-xs font-medium text-muted-foreground">Sort By</p>
            <div className="flex gap-2">
              {sortOptions.map((option) => (
                <button
                  key={option.value}
                  onClick={() => { setSortBy(option.value); setShowFilters(false) }}
                  className={cn(
                    "rounded-full px-3 py-1 text-xs font-medium transition-colors",
                    sortBy === option.value
                      ? "bg-primary text-primary-foreground"
                      : "bg-secondary text-muted-foreground hover:text-foreground"
                  )}
                >
                  {option.label}
                </button>
              ))}
            </div>
          </div>
          <div>
            <p className="mb-2 text-xs font-medium text-muted-foreground">Release Year</p>
            <div className="flex gap-2">
              <select
                value={yearFrom}
                onChange={(e) => setYearFrom(Number(e.target.value))}
                className="flex-1 rounded-lg border border-border/50 bg-secondary px-2 py-1.5 text-xs text-foreground transition-colors hover:bg-surface focus:outline-none focus:ring-1 focus:ring-primary"
              >
                {releaseYears.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
              <select
                value={yearTo}
                onChange={(e) => setYearTo(Number(e.target.value))}
                className="flex-1 rounded-lg border border-border/50 bg-secondary px-2 py-1.5 text-xs text-foreground transition-colors hover:bg-surface focus:outline-none focus:ring-1 focus:ring-primary"
              >
                {releaseYears.map((year) => (
                  <option key={year} value={year}>
                    {year}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div>
            <p className="mb-2 text-xs font-medium text-muted-foreground">Language</p>
            <div className="flex flex-wrap gap-2">
              {filteredLanguages.map((lang) => (
                <button
                  key={lang}
                  onClick={() => toggleLanguage(lang)}
                  className={cn(
                    "rounded-full px-3 py-1 text-xs font-medium transition-colors",
                    selectedLanguages.includes(lang)
                      ? "bg-primary text-primary-foreground"
                      : "bg-secondary text-muted-foreground hover:text-foreground"
                  )}
                >
                  {lang}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Grid */}
      <div className="flex-1">
        <div className="mb-4">
          <div className="flex items-center gap-2 rounded-lg border border-border/50 bg-card px-3 py-2">
            <Search className="h-4 w-4 text-muted-foreground" />
            <input
              type="text"
              value={search}
              onChange={(e) => {
                const next = e.target.value
                setSearch(next)
                if (activeActorFilter && next.trim() !== activeActorFilter) {
                  setActiveActorFilter("")
                }
                if (activeDirectorFilter && next.trim() !== activeDirectorFilter) {
                  setActiveDirectorFilter("")
                }
              }}
              placeholder="Search movies by title, director, or actor..."
              className="w-full bg-transparent text-sm text-foreground placeholder:text-muted-foreground focus:outline-none"
            />
          </div>
          {activeActorFilter && (
            <p className="mt-2 text-xs font-medium text-[#2C7A7B]">
              Results for actor: {activeActorFilter}
            </p>
          )}
          {activeDirectorFilter && (
            <p className="mt-2 text-xs font-medium text-[#2C7A7B]">
              Results for director: {activeDirectorFilter}
            </p>
          )}
        </div>
        {loading && (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <p className="text-lg font-medium text-foreground">Loading movies...</p>
          </div>
        )}
        {error && !loading && (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <p className="text-lg font-medium text-foreground">Could not load movies</p>
            <p className="mt-1 text-sm text-muted-foreground">{error}</p>
          </div>
        )}
        {!loading && !error && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:gap-6 xl:grid-cols-4">
          {filtered.map((movie) => (
            <Link
              key={movie.id}
              href={`/movies/${movie.id}`}
              className="group"
            >
              <div className="relative aspect-[2/3] overflow-hidden rounded-xl border border-border/30">
                <Image
                  src={movie.poster}
                  alt={movie.title}
                  fill
                  className="object-cover transition-transform duration-500 group-hover:scale-105"
                />
                <div className="absolute inset-0 bg-gradient-to-t from-background/80 via-transparent to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
                <div className="absolute bottom-0 left-0 right-0 translate-y-4 p-3 opacity-0 transition-all group-hover:translate-y-0 group-hover:opacity-100">
                  <div className="flex items-center gap-1">
                    <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                    <span className="text-xs font-bold text-foreground">{movie.rating.toFixed(1)}</span>
                  </div>
                  <div className="mt-1 flex flex-wrap gap-1">
                    {movie.genres.map((g) => (
                      <span key={g} className="rounded-full bg-secondary/80 px-2 py-0.5 text-[10px] font-medium text-foreground">
                        {g}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
              <h3 className="mt-3 text-sm font-semibold text-foreground transition-colors group-hover:text-primary">
                {movie.title}
              </h3>
              <p className="mt-0.5 text-xs text-muted-foreground">
                {movie.year ?? "N/A"} &middot; {movie.runtime}
              </p>
            </Link>
          ))}
        </div>
        )}
        {!loading && !error && filtered.length > 0 && (
          <div className="mt-8 space-y-2">
            <p className="text-center text-sm text-muted-foreground">
              Page {pageNumber} of {totalPages}
              {totalCount > 0 ? ` • ${totalCount} movies` : ""}
            </p>
            <div className="flex items-center justify-center gap-5">
              <button
                type="button"
                aria-label="Previous page"
                disabled={pageNumber <= 1}
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                className={cn(
                  "inline-flex h-8 w-8 items-center justify-center rounded-full text-foreground transition-colors duration-200",
                  pageNumber <= 1 ? "cursor-not-allowed text-muted-foreground/40" : "hover:text-primary"
                )}
              >
                <ChevronLeft className="h-5 w-5" />
              </button>
              <div className="flex items-center justify-center gap-3 sm:gap-4">
              {paginationItems.map((item, idx) => {
                if (typeof item !== "number") {
                  return (
                    <span key={`${item}-${idx}`} className="text-sm text-muted-foreground">
                      ...
                    </span>
                  )
                }
                const active = item === pageNumber
                return (
                  <button
                    key={item}
                    type="button"
                    className={cn(
                      "text-sm font-medium transition-all duration-200",
                      active
                        ? "text-primary underline underline-offset-4"
                        : "text-muted-foreground hover:text-foreground"
                    )}
                    onClick={() => setPageNumber(item)}
                  >
                    {item}
                  </button>
                )
              })}
              </div>
              <button
                type="button"
                aria-label="Next page"
                disabled={pageNumber >= totalPages}
                onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                className={cn(
                  "inline-flex h-8 w-8 items-center justify-center rounded-full text-foreground transition-colors duration-200",
                  pageNumber >= totalPages ? "cursor-not-allowed text-muted-foreground/40" : "hover:text-primary"
                )}
              >
                <ChevronRight className="h-5 w-5" />
              </button>
            </div>
          </div>
        )}
        {!loading && !error && filtered.length === 0 && (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <p className="text-lg font-medium text-foreground">No movies found</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Try adjusting your filters
            </p>
          </div>
        )}
      </div>
    </div>
  )
}
