"use client"

import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import { listenToExtension, sendToExtension } from "@/lib/netflix-bridge"

export type NetflixExtensionStatus = "checking" | "missing" | "installed"

export type NetflixTabInfo = {
  /** False until the extension has reported about the Netflix window at least once. */
  known: boolean
  open: boolean
  watchId: string | null
  videoReady: boolean
  playing: boolean
  time: number
}

const UNKNOWN_TAB: NetflixTabInfo = {
  known: false,
  open: false,
  watchId: null,
  videoReady: false,
  playing: false,
  time: 0,
}

const DETECT_PING_DELAYS_MS = [0, 400, 1000, 1800]
const DETECT_TIMEOUT_MS = 2600
const MISSING_POLL_MS = 2000

/** Detects the CineVerse extension and follows the state of the Netflix window it manages. */
export function useNetflixExtension() {
  const [status, setStatus] = useState<NetflixExtensionStatus>("checking")
  const [tab, setTab] = useState<NetflixTabInfo>(UNKNOWN_TAB)
  const [commandError, setCommandError] = useState<string | null>(null)
  const [detectionRun, setDetectionRun] = useState(0)
  const seenRef = useRef(false)

  useEffect(() => {
    return listenToExtension((message) => {
      switch (message.type) {
        case "READY":
          seenRef.current = true
          setStatus("installed")
          sendToExtension({ type: "REQUEST_STATE" })
          break

        case "TAB_STATE":
          seenRef.current = true
          setStatus("installed")
          setTab({
            known: true,
            open: message.open,
            watchId: message.watchId,
            videoReady: message.videoReady,
            playing: message.playing,
            time: message.time,
          })
          break

        case "EVENT":
        case "TIME":
          setTab((prev) => (prev.open ? { ...prev, time: message.time, playing: message.playing } : prev))
          break

        case "TAB_CLOSED":
          setTab({ ...UNKNOWN_TAB, known: true })
          break

        case "COMMAND_ERROR":
          setCommandError(message.message)
          break
      }
    })
  }, [])

  useEffect(() => {
    seenRef.current = false
    setStatus("checking")

    const timers = DETECT_PING_DELAYS_MS.map((delay) => window.setTimeout(() => sendToExtension({ type: "PING" }), delay))
    let pollId: number | null = null

    const timeout = window.setTimeout(() => {
      if (seenRef.current) return

      setStatus("missing")
      // The extension can be installed while this page is open and then connects to it by itself, so keep asking.
      pollId = window.setInterval(() => {
        if (seenRef.current) {
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
  }, [detectionRun])

  const open = useCallback((url: string) => {
    setCommandError(null)
    sendToExtension({ type: "OPEN", url })
  }, [])

  const recheck = useCallback(() => setDetectionRun((value) => value + 1), [])

  return useMemo(() => ({ status, tab, commandError, open, recheck }), [status, tab, commandError, open, recheck])
}
