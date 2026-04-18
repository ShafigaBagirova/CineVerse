"use client"

import { Play } from "lucide-react"
import { useMemo, useState } from "react"
import { pickYoutubeTrailerEmbedUrl, type MovieVideoDto } from "@/lib/api/movies"

interface MovieTrailerProps {
  movieTitle: string
  videos?: MovieVideoDto[] | null
}

export function MovieTrailer({ movieTitle, videos }: MovieTrailerProps) {
  const [isPlaying, setIsPlaying] = useState(false)

  const embedUrl = useMemo(() => pickYoutubeTrailerEmbedUrl(videos ?? undefined), [videos])

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-xl font-semibold text-foreground mb-4">Watch Trailer</h3>
        <div
          className={[
            "relative w-full rounded-lg overflow-hidden aspect-video",
            isPlaying ? "bg-black" : "bg-surface flex items-center justify-center group cursor-pointer",
          ].join(" ")}
          onClick={() => {
            if (!isPlaying) setIsPlaying(true)
          }}
          role={!isPlaying ? "button" : undefined}
          tabIndex={!isPlaying ? 0 : undefined}
          onKeyDown={(e) => {
            if (!isPlaying && (e.key === "Enter" || e.key === " ")) {
              e.preventDefault()
              setIsPlaying(true)
            }
          }}
        >
          {isPlaying ? (
            embedUrl ? (
              <iframe
                className="absolute inset-0 h-full w-full border-0"
                src={embedUrl}
                title={movieTitle}
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
              />
            ) : (
              <div className="flex h-full min-h-[200px] w-full items-center justify-center px-4 text-center text-sm text-muted-foreground">
                No trailer is available for this movie.
              </div>
            )
          ) : (
            <>
              <div className="absolute inset-0 bg-gradient-to-r from-black/50 to-transparent" />
              <button
                type="button"
                className="relative z-10 flex items-center justify-center w-16 h-16 rounded-full bg-primary/80 hover:bg-primary transition-colors group-hover:scale-110 duration-300"
                onClick={(e) => {
                  e.stopPropagation()
                  setIsPlaying(true)
                }}
                aria-label="Play trailer"
              >
                <Play className="h-7 w-7 text-primary-foreground ml-1" />
              </button>
              {embedUrl && (
                <div className="absolute top-4 right-4 bg-primary/80 px-3 py-1 rounded text-xs font-medium text-primary-foreground pointer-events-none">
                  Trailer
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}
