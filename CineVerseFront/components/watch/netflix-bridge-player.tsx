"use client"

import { forwardRef, useCallback, useEffect, useImperativeHandle, useRef, useState } from "react"
import { ExternalLink } from "lucide-react"
import { NetflixExtensionMissingNotice, NetflixUnsupportedNotice } from "@/components/watch/netflix-notices"
import { PLAYER_STATE, type PlayerHandle } from "@/components/watch/player-types"
import { Button } from "@/components/ui/button"
import { useNetflixSupport } from "@/hooks/use-netflix-support"
import { listenToExtension, sendToExtension, type ExtensionMessage } from "@/lib/netflix-bridge"
import { canAutoOpen, isNetflixLoginPath } from "@/lib/netflix-auto-open"

type NetflixBridgePlayerProps = {
  contentId: string
  contentUrl: string
  onReady?: () => void
  onUnready?: () => void
  onStateChange?: (state: number) => void
  onSeek?: (seconds: number) => void
}

type ExtensionStatus = "checking" | "missing" | "installed"
type TabStatus = "unknown" | "closed" | "login" | "waiting" | "wrong-video" | "ready"

const DETECT_PING_DELAYS_MS = [0, 400, 1000, 1800]
const DETECT_TIMEOUT_MS = 2600
const MISSING_POLL_MS = 2000
const AUTO_OPEN_CHECK_MS = 3000

const PANEL_CLASS = "absolute inset-0 flex flex-col items-center justify-center gap-3 bg-zinc-950 p-6 text-center text-white"

function formatTime(totalSeconds: number) {
  const seconds = Math.max(0, Math.floor(totalSeconds))
  const h = Math.floor(seconds / 3600)
  const m = Math.floor((seconds % 3600) / 60)
  const s = seconds % 60
  const mm = h > 0 ? String(m).padStart(2, "0") : String(m)
  return `${h > 0 ? `${h}:` : ""}${mm}:${String(s).padStart(2, "0")}`
}

/**
 * Netflix cannot be embedded, so the video plays in the viewer's own Netflix tab. The CineVerse
 * extension relays play/pause/seek between that tab and this page, which exposes the same
 * PlayerHandle as the YouTube player and reuses the room's sync logic.
 */
export const NetflixBridgePlayer = forwardRef<PlayerHandle, NetflixBridgePlayerProps>(function NetflixBridgePlayer(
  { contentId, contentUrl, onReady, onUnready, onStateChange, onSeek },
  ref
) {
  const support = useNetflixSupport()
  const [extensionStatus, setExtensionStatus] = useState<ExtensionStatus>("checking")
  const [tabStatus, setTabStatus] = useState<TabStatus>("unknown")
  const [displayTime, setDisplayTime] = useState(0)
  const [displayPlaying, setDisplayPlaying] = useState(false)
  const [commandError, setCommandError] = useState<string | null>(null)

  const cacheRef = useRef({ time: 0, wall: Date.now(), playing: false })
  const readyRef = useRef(false)
  const extensionSeenRef = useRef(false)
  const autoOpenedRef = useRef(false)
  const autoOpenAttemptsRef = useRef(0)
  const lastAutoOpenRef = useRef(0)
  const contentUrlRef = useRef(contentUrl)
  const callbacksRef = useRef({ onReady, onUnready, onStateChange, onSeek })

  useEffect(() => {
    callbacksRef.current = { onReady, onUnready, onStateChange, onSeek }
  }, [onReady, onUnready, onStateChange, onSeek])

  useEffect(() => {
    contentUrlRef.current = contentUrl
  }, [contentUrl])

  const currentTime = useCallback(() => {
    const cache = cacheRef.current
    return cache.playing ? cache.time + (Date.now() - cache.wall) / 1000 : cache.time
  }, [])

  const updateCache = useCallback((time: number, playing: boolean) => {
    cacheRef.current = { time, wall: Date.now(), playing }
    setDisplayTime(time)
    setDisplayPlaying(playing)
  }, [])

  const markReady = useCallback((ready: boolean) => {
    if (ready && !readyRef.current) {
      readyRef.current = true
      callbacksRef.current.onReady?.()
    } else if (!ready && readyRef.current) {
      readyRef.current = false
      callbacksRef.current.onUnready?.()
    }
  }, [])

  // The Netflix window is signed in but not on this room's video (for example on Netflix's home page after the
  // login), so the room opens its video there. It stops after a few tries, then the button below is the way.
  const tryAutoOpenRoomVideo = useCallback(() => {
    const now = Date.now()
    if (!canAutoOpen(autoOpenAttemptsRef.current, lastAutoOpenRef.current, now)) return

    autoOpenAttemptsRef.current += 1
    lastAutoOpenRef.current = now
    sendToExtension({ type: "OPEN", url: contentUrlRef.current })
  }, [])

  useImperativeHandle(
    ref,
    () => ({
      play: () => {
        if (readyRef.current) sendToExtension({ type: "PLAY" })
      },
      pause: () => {
        if (readyRef.current) sendToExtension({ type: "PAUSE" })
      },
      seekTo: (seconds: number) => {
        if (readyRef.current) sendToExtension({ type: "SEEK", seconds: Math.max(0, seconds) })
      },
      getCurrentTime: () => currentTime(),
      getState: () => {
        if (!readyRef.current) return -1
        return cacheRef.current.playing ? PLAYER_STATE.PLAYING : PLAYER_STATE.PAUSED
      },
      isPlaying: () => readyRef.current && cacheRef.current.playing,
    }),
    [currentTime]
  )

  useEffect(() => {
    return listenToExtension((message: ExtensionMessage) => {
      switch (message.type) {
        case "READY":
          extensionSeenRef.current = true
          setExtensionStatus("installed")
          sendToExtension({ type: "REQUEST_STATE" })
          break

        case "TAB_STATE":
          extensionSeenRef.current = true
          setExtensionStatus("installed")
          if (!message.open) {
            setTabStatus("closed")
            markReady(false)
          } else if (message.watchId && message.watchId !== contentId) {
            setTabStatus("wrong-video")
            markReady(false)
          } else if (message.videoReady) {
            updateCache(message.time, message.playing)
            setTabStatus("ready")
            markReady(true)
            autoOpenAttemptsRef.current = 0
          } else if (isNetflixLoginPath(message.path)) {
            setTabStatus("login")
            markReady(false)
            autoOpenAttemptsRef.current = 0
          } else {
            setTabStatus("waiting")
            markReady(false)
            tryAutoOpenRoomVideo()
          }
          break

        case "EVENT":
          if (!readyRef.current) break
          updateCache(message.time, message.playing)
          if (message.action === "seek") callbacksRef.current.onSeek?.(message.time)
          else
            callbacksRef.current.onStateChange?.(
              message.action === "play" ? PLAYER_STATE.PLAYING : PLAYER_STATE.PAUSED
            )
          break

        case "TIME":
          if (readyRef.current) updateCache(message.time, message.playing)
          break

        case "TAB_CLOSED":
          setTabStatus("closed")
          markReady(false)
          break

        case "COMMAND_ERROR":
          setCommandError(message.message)
          break
      }
    })
  }, [contentId, markReady, updateCache, tryAutoOpenRoomVideo])

  useEffect(() => {
    extensionSeenRef.current = false
    setExtensionStatus("checking")

    const timers = DETECT_PING_DELAYS_MS.map((delay) => window.setTimeout(() => sendToExtension({ type: "PING" }), delay))
    let pollId: number | null = null

    const timeout = window.setTimeout(() => {
      if (extensionSeenRef.current) return

      setExtensionStatus("missing")
      // The extension can be installed while this page is open and then connects to it by itself, so keep asking.
      pollId = window.setInterval(() => {
        if (extensionSeenRef.current) {
          if (pollId !== null) window.clearInterval(pollId)
          pollId = null
          return
        }
        sendToExtension({ type: "PING" })
      }, MISSING_POLL_MS)
    }, DETECT_TIMEOUT_MS)

    return () => {
      timers.forEach((timer) => window.clearTimeout(timer))
      window.clearTimeout(timeout)
      if (pollId !== null) window.clearInterval(pollId)
    }
  }, [])

  // Opens the room's video once per visit, so people who join by code land straight on Netflix.
  useEffect(() => {
    if (extensionStatus !== "installed" || tabStatus !== "closed" || autoOpenedRef.current) return
    autoOpenedRef.current = true
    sendToExtension({ type: "OPEN", url: contentUrl })
  }, [extensionStatus, tabStatus, contentUrl])

  // While the Netflix window is signed in but elsewhere, keep trying (spaced out) even if no new page event arrives,
  // for example right after the user has chosen a profile.
  useEffect(() => {
    if (tabStatus !== "waiting") return

    const intervalId = window.setInterval(tryAutoOpenRoomVideo, AUTO_OPEN_CHECK_MS)
    return () => window.clearInterval(intervalId)
  }, [tabStatus, tryAutoOpenRoomVideo])

  useEffect(() => {
    return () => {
      readyRef.current = false
    }
  }, [])

  const openNetflix = () => {
    setCommandError(null)
    sendToExtension({ type: "OPEN", url: contentUrl })
  }

  const focusNetflix = () => {
    setCommandError(null)
    sendToExtension({ type: "FOCUS" })
  }

  if (support === "mobile" || support === "unsupported-browser") {
    return (
      <div className={PANEL_CLASS}>
        <NetflixUnsupportedNotice support={support} tone="dark" />
      </div>
    )
  }

  return (
    <div className={PANEL_CLASS}>
      {extensionStatus === "checking" ? (
        <p className="text-sm text-zinc-300">Looking for the CineVerse extension...</p>
      ) : null}

      {extensionStatus === "missing" ? <NetflixExtensionMissingNotice tone="dark" /> : null}

      {extensionStatus === "installed" ? (
        <>
          <p className="text-xs uppercase tracking-widest text-zinc-500">Netflix</p>

          {tabStatus === "unknown" ? <p className="text-sm text-zinc-300">Checking your Netflix window...</p> : null}

          {tabStatus === "closed" ? (
            <>
              <p className="text-base font-medium">Open this video on Netflix</p>
              <p className="max-w-md text-sm text-zinc-400">
                It opens in a new window. Everyone in the room needs their own Netflix subscription. Keep this room
                visible next to it for chat and reactions.
              </p>
            </>
          ) : null}

          {tabStatus === "login" ? (
            <>
              <p className="text-base font-medium">Sign in to Netflix</p>
              <p className="max-w-md text-sm text-zinc-400">
                Sign in in the Netflix window that opened. This room&apos;s video opens there by itself once you are
                signed in.
              </p>
            </>
          ) : null}

          {tabStatus === "waiting" ? (
            <p className="max-w-md text-sm text-zinc-300">
              Opening this room&apos;s video in your Netflix window... If a profile is asked for, choose it there.
            </p>
          ) : null}

          {tabStatus === "wrong-video" ? (
            <p className="max-w-md text-sm text-amber-400">
              The Netflix window is showing a different video than this room.
            </p>
          ) : null}

          {tabStatus === "ready" ? (
            <p className="text-sm text-emerald-400">
              Netflix connected · {displayPlaying ? "Playing" : "Paused"} · {formatTime(displayTime)}
            </p>
          ) : null}

          {tabStatus === "closed" || tabStatus === "waiting" || tabStatus === "wrong-video" ? (
            <Button variant="secondary" size="sm" onClick={openNetflix}>
              <ExternalLink className="mr-1 h-4 w-4" />
              {tabStatus === "closed" ? "Open on Netflix" : "Open the room's video"}
            </Button>
          ) : null}

          {tabStatus === "login" || tabStatus === "ready" ? (
            <Button variant="outline" size="sm" onClick={focusNetflix}>
              <ExternalLink className="mr-1 h-4 w-4" />
              Show Netflix window
            </Button>
          ) : null}
        </>
      ) : null}

      {commandError ? <p className="max-w-md text-xs text-amber-400">{commandError}</p> : null}
    </div>
  )
})
