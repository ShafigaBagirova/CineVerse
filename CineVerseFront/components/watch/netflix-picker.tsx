"use client"

import { useEffect, useRef, useState } from "react"
import Link from "next/link"
import { useRouter, useSearchParams } from "next/navigation"
import { ArrowLeft, ExternalLink } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { NetflixExtensionMissingNotice, NetflixUnsupportedNotice } from "@/components/watch/netflix-notices"
import { useNetflixExtension } from "@/hooks/use-netflix-extension"
import { useNetflixSupport } from "@/hooks/use-netflix-support"
import { userFacingApiErrorMessage } from "@/lib/api/types"
import { createWatchRoom } from "@/lib/api/watch-rooms"

const NETFLIX_BROWSE_URL = "https://www.netflix.com/browse"
const CREATE_DELAY_PLAYING_MS = 1500
const CREATE_DELAY_PAUSED_MS = 4500

type Phase = "waiting" | "creating" | "failed"

/**
 * Opens Netflix in its own window through the extension. Once the user starts any video there,
 * the room is created for that video and the user is taken into it.
 */
export function NetflixPicker() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { status: authStatus } = useAuth()
  const title = searchParams.get("title")?.trim() ?? ""

  const { status, tab, commandError, open } = useNetflixExtension()
  const support = useNetflixSupport()
  const unsupported = support === "mobile" || support === "unsupported-browser"
  const [phase, setPhase] = useState<Phase>("waiting")
  const [error, setError] = useState<string | null>(null)
  const [armed, setArmed] = useState(false)

  const openedRef = useRef(false)
  const creatingRef = useRef(false)
  const tabRef = useRef(tab)

  useEffect(() => {
    tabRef.current = tab
  }, [tab])

  useEffect(() => {
    if (authStatus !== "authenticated" || !title || status !== "installed" || openedRef.current) return
    openedRef.current = true
    open(NETFLIX_BROWSE_URL)
  }, [authStatus, title, status, open])

  // A video that was already playing before this page opened must not become the room; wait until Netflix is browsing.
  useEffect(() => {
    if (tab.known && tab.open && tab.watchId === null) setArmed(true)
  }, [tab.known, tab.open, tab.watchId])

  const candidateId = armed && tab.open && tab.videoReady ? tab.watchId : null

  useEffect(() => {
    if (phase !== "waiting" || !candidateId) return

    const timer = window.setTimeout(
      () => void createRoom(),
      tabRef.current.playing ? CREATE_DELAY_PLAYING_MS : CREATE_DELAY_PAUSED_MS
    )
    return () => window.clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [phase, candidateId, tab.playing])

  const createRoom = async () => {
    const current = tabRef.current
    if (creatingRef.current || !current.watchId) return

    creatingRef.current = true
    setPhase("creating")
    setError(null)

    try {
      const room = await createWatchRoom({
        contentUrl: `https://www.netflix.com/watch/${current.watchId}`,
        title,
        startPositionSeconds: current.time,
        startPlaying: current.playing,
      })
      router.push(`/watch/${room.code}`)
    } catch (e) {
      setError(userFacingApiErrorMessage(e, "Could not create the watch room."))
      setPhase("failed")
      creatingRef.current = false
    }
  }

  if (authStatus === "guest") {
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
          <p className="mb-4 text-sm text-muted-foreground">Name your room first, then pick something to watch.</p>
          <Button asChild>
            <Link href="/watch">Back to watch rooms</Link>
          </Button>
        </Card>
      </div>
    )
  }

  const windowOpen = tab.known && tab.open

  return (
    <div className="mx-auto max-w-xl px-4 py-8">
      <Link href="/watch" className="mb-4 inline-flex items-center gap-1 text-sm text-muted-foreground hover:underline">
        <ArrowLeft className="h-4 w-4" />
        Back
      </Link>
      <h1 className="text-xl font-semibold">Pick something on Netflix</h1>
      <p className="mb-5 text-sm text-muted-foreground">Room: {title}</p>

      <Card className="flex flex-col items-center gap-3 p-8 text-center">
        {unsupported ? <NetflixUnsupportedNotice support={support} tone="light" /> : null}

        {!unsupported && status === "checking" ? <p className="text-sm text-muted-foreground">Looking for the CineVerse extension...</p> : null}

        {!unsupported && status === "missing" ? <NetflixExtensionMissingNotice tone="light" /> : null}

        {!unsupported && status === "installed" && phase === "creating" ? <p className="font-medium">Creating your room...</p> : null}

        {!unsupported && status === "installed" && phase === "failed" ? (
          <>
            <p className="text-sm text-destructive">{error}</p>
            <Button size="sm" onClick={() => setPhase("waiting")}>
              Try again
            </Button>
          </>
        ) : null}

        {!unsupported && status === "installed" && phase === "waiting" ? (
          <>
            {candidateId ? (
              <p className="font-medium">Got it, setting up your room...</p>
            ) : windowOpen ? (
              <>
                <p className="font-medium">Netflix is open in a new window</p>
                <p className="max-w-md text-sm text-muted-foreground">
                  Sign in to your Netflix account there, find a film or episode and start playing it. Your room is created
                  automatically.
                </p>
              </>
            ) : (
              <>
                <p className="font-medium">Opening Netflix...</p>
                <p className="max-w-md text-sm text-muted-foreground">
                  If no window appeared, press the button below. Netflix will ask you to sign in with your own account.
                </p>
              </>
            )}

            {!candidateId ? (
              <Button variant="outline" size="sm" onClick={() => open(NETFLIX_BROWSE_URL)}>
                <ExternalLink className="mr-1 h-4 w-4" />
                {windowOpen ? "Show Netflix window" : "Open Netflix"}
              </Button>
            ) : null}
          </>
        ) : null}

        {commandError ? <p className="max-w-md text-xs text-amber-600">{commandError}</p> : null}
      </Card>

      <p className="mt-4 text-xs text-muted-foreground">
        Everyone in the room needs their own Netflix subscription and the CineVerse extension. CineVerse is not affiliated
        with Netflix and never sees your Netflix login.
      </p>
    </div>
  )
}
