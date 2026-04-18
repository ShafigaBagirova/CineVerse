import { getAllMovies } from "@/lib/api/movies"
import { getAllScreenings } from "@/lib/api/screenings"
import { getAllHalls } from "@/lib/api/halls"
import { getAllCinemas } from "@/lib/api/cinemas"
import { ShowtimesClient } from "@/components/showtimes/showtimes-client"
import type { ShowtimesMovieData, ShowtimesRow } from "@/components/showtimes/showtimes-client"

export default async function ShowtimesPage() {
  const loadWithContext = async <T,>(label: string, loader: () => Promise<T>) => {
    try {
      return await loader()
    } catch (error) {
      throw new Error(`${label} request failed: ${error instanceof Error ? error.message : "unknown error"}`)
    }
  }

  const moviesResponse = await loadWithContext("movies", () =>
    getAllMovies({ pageNumber: 1, pageSize: 100, sortBy: "title", desc: true })
  )
  const screeningsResponse = await loadWithContext("screenings", () =>
    getAllScreenings({
      pageNumber: 1,
      pageSize: 100,
      isActive: true,
      status: "Scheduled",
    })
  )
  const hallsResponse = await loadWithContext("halls", () => getAllHalls(1, 100))
  const cinemasResponse = await loadWithContext("cinemas", () => getAllCinemas(1, 100))

  const movies = moviesResponse.items
  const hallsById = new Map(hallsResponse.items.map((hall) => [hall.id, hall]))
  const cinemasById = new Map(cinemasResponse.items.map((cinema) => [cinema.id, cinema]))
  const moviesById = new Map(movies.map((m) => [m.id, m]))

  const rows: ShowtimesRow[] = []
  screeningsResponse.items.forEach((screening) => {
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
    })
  })

  const moviesData: ShowtimesMovieData[] = movies.map((m) => ({
    id: m.id,
    title: m.title,
    posterUrl: m.posterUrl ?? null,
    durationMinutes: m.durationMinutes,
    releaseYear: m.releaseYear ?? null,
  }))

  return <ShowtimesClient movies={moviesData} rows={rows} />
}
