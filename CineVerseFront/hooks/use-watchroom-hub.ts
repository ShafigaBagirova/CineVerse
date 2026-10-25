"use client"

import { useCallback, useEffect, useMemo, useRef, useState } from "react"
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr"
import { getAccessToken } from "@/lib/api/http"
import type {
  WatchAction,
  WatchChatMessage,
  WatchHeartbeatEvent,
  WatchMemberEvent,
  WatchReaction,
  WatchRoomSnapshot,
  WatchStateChangedEvent,
} from "@/lib/api/watch-rooms"

export type WatchHubStatus = "connecting" | "connected" | "reconnecting" | "disconnected" | "failed"

export type WatchRoomHubHandlers = {
  onSnapshot?: (snapshot: WatchRoomSnapshot) => void
  onStateChanged?: (event: WatchStateChangedEvent) => void
  onHeartbeat?: (event: WatchHeartbeatEvent) => void
  onMessage?: (message: WatchChatMessage) => void
  onReaction?: (reaction: WatchReaction) => void
  onMemberJoined?: (member: WatchMemberEvent) => void
  onMemberLeft?: (member: WatchMemberEvent) => void
  onRoomClosed?: () => void
}

type UseWatchRoomHubOptions = {
  code: string
  enabled: boolean
  handlers: WatchRoomHubHandlers
}

function readHubError(error: unknown, fallback: string) {
  const raw = error instanceof Error ? error.message : ""
  const marker = "HubException: "
  const index = raw.indexOf(marker)
  if (index >= 0) return raw.slice(index + marker.length).trim() || fallback
  return raw.trim() || fallback
}

export function useWatchRoomHub({ code, enabled, handlers }: UseWatchRoomHubOptions) {
  const hubRef = useRef<HubConnection | null>(null)
  const handlersRef = useRef(handlers)
  const [status, setStatus] = useState<WatchHubStatus>("disconnected")
  const [error, setError] = useState<string | null>(null)
  const apiBaseUrl = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://127.0.0.1:5073").replace(/\/$/, "")
  const hubUrl = `${apiBaseUrl}/hubs/watchroom`

  useEffect(() => {
    handlersRef.current = handlers
  }, [handlers])

  useEffect(() => {
    if (!enabled || !code) {
      hubRef.current = null
      setStatus("disconnected")
      return
    }

    let cancelled = false
    const hub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => getAccessToken() ?? "",
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    hubRef.current = hub

    hub.on("StateChanged", (event: WatchStateChangedEvent) => handlersRef.current.onStateChanged?.(event))
    hub.on("Heartbeat", (event: WatchHeartbeatEvent) => handlersRef.current.onHeartbeat?.(event))
    hub.on("ReceiveMessage", (message: WatchChatMessage) => handlersRef.current.onMessage?.(message))
    hub.on("ReceiveReaction", (reaction: WatchReaction) => handlersRef.current.onReaction?.(reaction))
    hub.on("MemberJoined", (member: WatchMemberEvent) => handlersRef.current.onMemberJoined?.(member))
    hub.on("MemberLeft", (member: WatchMemberEvent) => handlersRef.current.onMemberLeft?.(member))
    hub.on("RoomClosed", () => handlersRef.current.onRoomClosed?.())

    const joinRoom = async () => {
      const snapshot = await hub.invoke<WatchRoomSnapshot>("JoinRoom", code)
      if (!cancelled) handlersRef.current.onSnapshot?.(snapshot)
    }

    hub.onreconnecting(() => setStatus("reconnecting"))
    hub.onreconnected(async () => {
      try {
        await joinRoom()
        setStatus("connected")
        setError(null)
      } catch (e) {
        setStatus("failed")
        setError(readHubError(e, "Failed to rejoin the watch room."))
      }
    })
    hub.onclose(() => {
      if (!cancelled) setStatus("disconnected")
    })

    const start = async () => {
      try {
        setStatus("connecting")
        setError(null)
        await hub.start()
        if (cancelled) return
        await joinRoom()
        if (!cancelled) setStatus("connected")
      } catch (e) {
        if (cancelled) return
        console.error("[watchroom] connect failed:", e)
        setStatus("failed")
        setError(readHubError(e, "Failed to connect to the watch room."))
      }
    }

    const startPromise = start()

    return () => {
      cancelled = true
      hubRef.current = null
      // Stopping while the connection is still negotiating makes SignalR log an error (React Strict Mode does this in dev).
      void startPromise.then(() => hub.stop()).catch(() => {})
    }
  }, [code, enabled, hubUrl])

  const invoke = useCallback(async (method: string, ...args: unknown[]) => {
    const hub = hubRef.current
    if (!hub || hub.state !== HubConnectionState.Connected) return
    try {
      await hub.invoke(method, ...args)
    } catch (e) {
      setError(readHubError(e, "Watch room request failed."))
    }
  }, [])

  return useMemo(
    () => ({
      status,
      error,
      isConnected: status === "connected",
      syncState: (action: WatchAction, positionSeconds: number) => invoke("SyncState", code, action, positionSeconds),
      sendHeartbeat: (positionSeconds: number) => invoke("Heartbeat", code, positionSeconds),
      sendMessage: (content: string) => invoke("SendMessage", code, content),
      sendReaction: (emoji: string) => invoke("SendReaction", code, emoji),
    }),
    [status, error, invoke, code]
  )
}
