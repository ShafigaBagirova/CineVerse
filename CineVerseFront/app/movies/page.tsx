"use client"

import { useMemo } from "react"
import { useSearchParams } from "next/navigation"
import { MoviesGrid } from "@/components/movies/movies-grid"

export default function MoviesPage() {
  const searchParams = useSearchParams()
  const initialSearch = useMemo(() => searchParams.get("search") ?? "", [searchParams])
  const initialActor = useMemo(() => {
    const actor = searchParams.get("actor")
    const cast = searchParams.get("cast")
    return actor ?? cast ?? ""
  }, [searchParams])
  const initialDirector = useMemo(() => {
    const director = searchParams.get("director")
    return director ?? ""
  }, [searchParams])

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">Browse</p>
        <h1 className="font-serif text-3xl font-bold text-foreground md:text-4xl">All Movies</h1>
      </div>
      <MoviesGrid initialSearch={initialSearch} initialActor={initialActor} initialDirector={initialDirector} />
    </div>
  )
}
