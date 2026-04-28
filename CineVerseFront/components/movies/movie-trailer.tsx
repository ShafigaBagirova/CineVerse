"use client"

import { useMemo } from "react"
import { pickYoutubeTrailerEmbedUrl, type MovieVideoDto } from "@/lib/api/movies"

interface MovieTrailerProps {
  movieTitle: string
  videos?: MovieVideoDto[] | null
}

export function MovieTrailer({ movieTitle, videos }: MovieTrailerProps) {
  const embedUrl = useMemo(() => pickYoutubeTrailerEmbedUrl(videos ?? undefined), [videos])

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold text-foreground mb-4">Watch Trailer</h3>
        <div className="relative w-full overflow-hidden rounded-lg aspect-video bg-black">
          {embedUrl ? (
            <iframe
              className="absolute inset-0 h-full w-full border-0"
              src={embedUrl}
              title={movieTitle}
              allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
              allowFullScreen
            />
          ) : (
            <div className="flex h-full min-h-[200px] w-full items-center justify-center bg-surface px-4 text-center text-sm text-muted-foreground">
              Trailer not available
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
