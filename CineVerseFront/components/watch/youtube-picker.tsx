"use client"

import { useState, type FormEvent } from "react"
import Link from "next/link"
import { useRouter, useSearchParams } from "next/navigation"
import { ArrowLeft, Link2, Search } from "lucide-react"
import { toast } from "sonner"
import { useAuth } from "@/components/providers/auth-provider"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { userFacingApiErrorMessage } from "@/lib/api/types"
import { createWatchRoom, searchYouTubeVideos, type YouTubeVideoDto } from "@/lib/api/watch-rooms"

const NOT_CONFIGURED_MARKER = "not configured"

export function YouTubePicker() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { status } = useAuth()
  const title = searchParams.get("title")?.trim() ?? ""

  const [query, setQuery] = useState("")
  const [results, setResults] = useState<YouTubeVideoDto[] | null>(null)
  const [searching, setSearching] = useState(false)
  const [searchError, setSearchError] = useState<string | null>(null)
  const [creatingKey, setCreatingKey] = useState<string | null>(null)
  const [pastedUrl, setPastedUrl] = useState("")

  const createRoom = async (contentUrl: string, key: string) => {
    try {
      setCreatingKey(key)
      const room = await createWatchRoom({ contentUrl, title })
      router.push(`/watch/${room.code}`)
    } catch (e) {
      toast.error(userFacingApiErrorMessage(e, "Could not create the watch room."))
      setCreatingKey(null)
    }
  }

  const handleSearch = async (event: FormEvent) => {
    event.preventDefault()
    const text = query.trim()
    if (!text) return
    try {
      setSearching(true)
      setSearchError(null)
      setResults(await searchYouTubeVideos(text))
    } catch (e) {
      setResults(null)
      setSearchError(userFacingApiErrorMessage(e, "Search failed. Please try again."))
    } finally {
      setSearching(false)
    }
  }

  const handlePaste = (event: FormEvent) => {
    event.preventDefault()
    const url = pastedUrl.trim()
    if (!url) return
    void createRoom(url, "paste")
  }

  if (status === "guest") {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <Card className="p-8">
          <p className="mb-4 text-sm text-muted-foreground">Sign in to create a watch room.</p>
          <Button asChild>
            <Link href="/auth">Sign in</Link>
          </Button>
        </Card>
      </div>
    )
  }

  if (!title) {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <Card className="p-8">
          <p className="mb-4 text-sm text-muted-foreground">Name your room first, then pick a video.</p>
          <Button asChild>
            <Link href="/watch">Back to watch rooms</Link>
          </Button>
        </Card>
      </div>
    )
  }

  const notConfigured = searchError?.toLowerCase().includes(NOT_CONFIGURED_MARKER) ?? false

  return (
    <div className="mx-auto max-w-5xl px-4 py-8">
      <Link href="/watch" className="mb-4 inline-flex items-center gap-1 text-sm text-muted-foreground hover:underline">
        <ArrowLeft className="h-4 w-4" />
        Back
      </Link>
      <h1 className="text-xl font-semibold">Pick a YouTube video</h1>
      <p className="mb-5 text-sm text-muted-foreground">Room: {title}</p>

      <form onSubmit={handleSearch} className="flex gap-2">
        <Input
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Search YouTube"
          maxLength={100}
          autoFocus
        />
        <Button type="submit" disabled={searching || !query.trim()}>
          <Search className="mr-1 h-4 w-4" />
          Search
        </Button>
      </form>

      {searchError ? (
        <p className="mt-3 text-sm text-destructive">
          {notConfigured
            ? "Search is not set up on the server yet. You can still paste a video link below."
            : searchError}
        </p>
      ) : null}

      {searching ? (
        <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, index) => (
            <Skeleton key={index} className="aspect-video w-full" />
          ))}
        </div>
      ) : results ? (
        results.length === 0 ? (
          <p className="mt-5 text-sm text-muted-foreground">No videos found.</p>
        ) : (
          <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {results.map((video) => (
              <button
                key={video.videoId}
                type="button"
                disabled={creatingKey !== null}
                onClick={() => void createRoom(`https://www.youtube.com/watch?v=${video.videoId}`, video.videoId)}
                className="overflow-hidden rounded-xl border bg-card text-left transition hover:shadow-md disabled:opacity-60"
              >
                <div className="relative aspect-video bg-muted">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={video.thumbnailUrl} alt="" className="h-full w-full object-cover" />
                  {creatingKey === video.videoId ? (
                    <div className="absolute inset-0 flex items-center justify-center bg-black/50 text-sm text-white">
                      Creating room...
                    </div>
                  ) : null}
                </div>
                <div className="p-3">
                  <p className="line-clamp-2 text-sm font-medium">{video.title}</p>
                  <p className="mt-1 text-xs text-muted-foreground">{video.channelTitle}</p>
                </div>
              </button>
            ))}
          </div>
        )
      ) : null}

      <div className="mt-8 border-t pt-6">
        <h2 className="mb-2 flex items-center gap-2 text-sm font-medium">
          <Link2 className="h-4 w-4" />
          Or paste a video link
        </h2>
        <form onSubmit={handlePaste} className="flex gap-2">
          <Input
            value={pastedUrl}
            onChange={(event) => setPastedUrl(event.target.value)}
            placeholder="https://www.youtube.com/watch?v=..."
          />
          <Button type="submit" variant="outline" disabled={creatingKey !== null || !pastedUrl.trim()}>
            {creatingKey === "paste" ? "Creating..." : "Create room"}
          </Button>
        </form>
      </div>
    </div>
  )
}
