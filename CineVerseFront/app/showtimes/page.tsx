import { getAllMoviesAllPages } from "@/lib/api/movies"
import { getAllScreeningsAllPages } from "@/lib/api/screenings"
import { getAllHalls } from "@/lib/api/halls"
import { getAllCinemas } from "@/lib/api/cinemas"
import { ShowtimesClient } from "@/components/showtimes/showtimes-client"
import type { ShowtimesMovieData, ShowtimesRow } from "@/components/showtimes/showtimes-client"

export const dynamic = "force-dynamic"
export const revalidate = 0

export default async function ShowtimesPage() {
  const loadWithContext = async <T,>(label: string, loader: () => Promise<T>) => {
    try {
      return await loader()
    } catch (error) {
      throw new Error(`${label} request failed: ${error instanceof Error ? error.message : "unknown error"}`)
    }
  }

  const movies = await loadWithContext("movies", () =>
    getAllMoviesAllPages({ sortBy: "title", desc: false }, { quiet: true })
  )
  const screeningsRows = await loadWithContext("screenings", () =>
    getAllScreeningsAllPages(
      {
        isActive: true,
        status: "Scheduled",
      },
      { noCache: true, cacheBust: true }
    )
  )
  const hallsResponse = await loadWithContext("halls", () => getAllHalls(1, 100))
  const cinemasResponse = await loadWithContext("cinemas", () => getAllCinemas(1, 100))

  const hallsById = new Map(hallsResponse.items.map((hall) => [hall.id, hall]))
  const cinemasById = new Map(cinemasResponse.items.map((cinema) => [cinema.id, cinema]))
  const moviesById = new Map(movies.map((m) => [m.id, m]))

  const rows: ShowtimesRow[] = []
  screeningsRows.forEach((screening) => {
    const hall = hallsById.get(screening.hallId)
    if (!hall) return
    const cinema = cinemasById.get(hall.cinemaId)
    if (!cinema) return
    if (!moviesById.has(screening.movieId)) return

    rows.push({
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

  const moviesData: ShowtimesMovieData[] = movies.map((m) => ({
    id: m.id,
    title: m.title,
    posterUrl: m.posterUrl ?? null,
    durationMinutes: m.durationMinutes,
    releaseYear: m.releaseYear ?? null,
    language: m.language ?? null,
  }))

  return <ShowtimesClient movies={moviesData} rows={rows} />
}
