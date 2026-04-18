import { MoviesGrid } from "@/components/movies/movies-grid"

export default async function MoviesPage({
  searchParams,
}: {
  searchParams?: Promise<Record<string, string | string[] | undefined>>
}) {
  const resolved = searchParams ? await searchParams : {}
  const rawSearch = resolved?.search
  const initialSearch = Array.isArray(rawSearch) ? rawSearch[0] ?? "" : rawSearch ?? ""

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
          Browse
        </p>
        <h1 className="font-serif text-3xl font-bold text-foreground md:text-4xl">
          All Movies
        </h1>
      </div>
      <MoviesGrid initialSearch={initialSearch} />
    </div>
  )
}
