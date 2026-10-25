"use client"

import { forwardRef, useEffect, useImperativeHandle, useRef } from "react"

interface YouTubePlayerApi {
  playVideo: () => void
  pauseVideo: () => void
  seekTo: (seconds: number, allowSeekAhead: boolean) => void
  getCurrentTime: () => number
  getPlayerState: () => number
  destroy: () => void
}

interface YouTubeNamespace {
  Player: new (
    element: HTMLElement,
    options: {
      videoId: string
      width?: string | number
      height?: string | number
      playerVars?: Record<string, number | string>
      events?: {
        onReady?: () => void
        onStateChange?: (event: { data: number }) => void
      }
    }
  ) => YouTubePlayerApi
}

declare global {
  interface Window {
    YT?: YouTubeNamespace
    onYouTubeIframeAPIReady?: () => void
  }
}

export const YT_STATE = {
  ENDED: 0,
  PLAYING: 1,
  PAUSED: 2,
  BUFFERING: 3,
} as const

export interface YouTubePlayerHandle {
  play: () => void
  pause: () => void
  seekTo: (seconds: number) => void
  getCurrentTime: () => number
  getState: () => number
  isPlaying: () => boolean
}

type YouTubePlayerProps = {
  videoId: string
  onReady?: () => void
  onStateChange?: (state: number) => void
}

let apiPromise: Promise<void> | null = null

function loadYouTubeApi() {
  if (typeof window === "undefined") return Promise.resolve()
  if (window.YT?.Player) return Promise.resolve()
  if (apiPromise) return apiPromise

  apiPromise = new Promise<void>((resolve) => {
    const previous = window.onYouTubeIframeAPIReady
    window.onYouTubeIframeAPIReady = () => {
      previous?.()
      resolve()
    }
    const script = document.createElement("script")
    script.src = "https://www.youtube.com/iframe_api"
    document.head.appendChild(script)
  })

  return apiPromise
}

export const YouTubePlayer = forwardRef<YouTubePlayerHandle, YouTubePlayerProps>(function YouTubePlayer(
  { videoId, onReady, onStateChange },
  ref
) {
  const containerRef = useRef<HTMLDivElement | null>(null)
  const playerRef = useRef<YouTubePlayerApi | null>(null)
  const readyRef = useRef(false)
  const onReadyRef = useRef(onReady)
  const onStateChangeRef = useRef(onStateChange)

  useEffect(() => {
    onReadyRef.current = onReady
    onStateChangeRef.current = onStateChange
  }, [onReady, onStateChange])

  useImperativeHandle(
    ref,
    () => ({
      play: () => {
        if (readyRef.current) playerRef.current?.playVideo()
      },
      pause: () => {
        if (readyRef.current) playerRef.current?.pauseVideo()
      },
      seekTo: (seconds: number) => {
        if (readyRef.current) playerRef.current?.seekTo(Math.max(0, seconds), true)
      },
      getCurrentTime: () => (readyRef.current ? (playerRef.current?.getCurrentTime() ?? 0) : 0),
      getState: () => (readyRef.current ? (playerRef.current?.getPlayerState() ?? -1) : -1),
      isPlaying: () => readyRef.current && playerRef.current?.getPlayerState() === YT_STATE.PLAYING,
    }),
    []
  )

  useEffect(() => {
    const container = containerRef.current
    if (!container || !videoId) return

    let cancelled = false
    const target = document.createElement("div")
    container.appendChild(target)

    void loadYouTubeApi().then(() => {
      if (cancelled || !window.YT) return
      playerRef.current = new window.YT.Player(target, {
        videoId,
        width: "100%",
        height: "100%",
        playerVars: { rel: 0, playsinline: 1 },
        events: {
          onReady: () => {
            readyRef.current = true
            onReadyRef.current?.()
          },
          onStateChange: (event) => onStateChangeRef.current?.(event.data),
        },
      })
    })

    return () => {
      cancelled = true
      readyRef.current = false
      try {
        playerRef.current?.destroy()
      } catch {
        // the iframe may already be gone
      }
      playerRef.current = null
      container.replaceChildren()
    }
  }, [videoId])

  return <div ref={containerRef} className="absolute inset-0 [&_iframe]:h-full [&_iframe]:w-full" />
})
