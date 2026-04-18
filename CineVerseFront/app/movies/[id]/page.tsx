import { notFound } from "next/navigation"
import { MovieHero } from "@/components/movies/movie-hero"
import { MovieShowtimes } from "@/components/movies/movie-showtimes"
import { MovieReviews } from "@/components/movies/movie-reviews"
import { MovieTrailer } from "@/components/movies/movie-trailer"
import { MovieAnalysisSection } from "@/components/movies/movie-analysis"
import { SuggestedMovies } from "@/components/suggestions/suggested-movies"
import { CinemaAvailability } from "@/components/cinema/cinema-availability"
import { getMovieById } from "@/lib/api/movies"
import { getAllScreenings } from "@/lib/api/screenings"
import { getAllHalls } from "@/lib/api/halls"
import { getAllCinemas } from "@/lib/api/cinemas"

export default async function MovieDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  const numericId = Number(id)
  if (Number.isNaN(numericId)) notFound()

  let movie = null
  try {
    movie = await getMovieById(numericId)
  } catch {
    notFound()
  }
  if (!movie) notFound()

  const loadWithContext = async <T,>(label: string, loader: () => Promise<T>) => {
    try {
      return await loader()
    } catch (error) {
      throw new Error(`${label} request failed: ${error instanceof Error ? error.message : "unknown error"}`)
    }
  }

  const screeningsResponse = await loadWithContext("screenings", () =>
    getAllScreenings({ movieId: movie.id, isActive: true, status: "Scheduled", pageNumber: 1, pageSize: 100 })
  )
  const hallsResponse = await loadWithContext("halls", () => getAllHalls(1, 100))
  const cinemasResponse = await loadWithContext("cinemas", () => getAllCinemas(1, 100))

  const hallsById = new Map(hallsResponse.items.map((hall) => [hall.id, hall]))
  const cinemasById = new Map(cinemasResponse.items.map((cinema) => [cinema.id, cinema]))

  const showtimeGroups = new Map<
    string,
    {
      cinemaId: number
      cinema: string
      address?: string
      screenings: Array<{ screeningId: number; time: string }>
      price: number
      language: string
      format: string
    }
  >()

  screeningsResponse.items.forEach((screening) => {
    const hall = hallsById.get(screening.hallId)
    if (!hall) return
    const cinema = cinemasById.get(hall.cinemaId)
    if (!cinema) return
    const key = `${cinema.id}-${screening.price}-${screening.language}-${screening.format}`
    const existing = showtimeGroups.get(key)
    const time = new Date(screening.startTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })
    if (existing) {
      existing.screenings.push({ screeningId: screening.id, time })
      return
    }

    showtimeGroups.set(key, {
      cinemaId: cinema.id,
      cinema: cinema.name,
      address: cinema.address,
      screenings: [{ screeningId: screening.id, time }],
      price: Number(screening.price),
      language: screening.language,
      format: screening.format,
    })
  })

  const showtimes = Array.from(showtimeGroups.values())

  return (
    <>
      <MovieHero movie={movie} />
      <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
        <div className="grid gap-10 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-8">
            <MovieTrailer movieTitle={movie.title} videos={movie.videos ?? undefined} />
            <MovieReviews movieId={movie.id} />
            <MovieAnalysisSection movieId={movie.id} />
          </div>
          <div className="space-y-6">
            <MovieShowtimes movieId={movie.id} showtimes={showtimes} />
            <CinemaAvailability movieId={movie.id} showtimes={showtimes} />
          </div>
        </div>
      </div>
      <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
        <SuggestedMovies excludeIds={[movie.id]} title="You Might Also Like" />
      </div>
    </>
  )
}
