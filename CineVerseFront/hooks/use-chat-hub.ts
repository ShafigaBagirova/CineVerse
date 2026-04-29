"use client"

import { useEffect, useMemo, useRef, useState } from "react"
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr"
import { getAccessToken } from "@/lib/api/http"
import type { MessageDto } from "@/lib/api/chats"

type UseChatHubOptions = {
  enabled: boolean
  activeChatId: number | null
  onReceiveMessage?: (message: MessageDto) => void
}

export function useChatHub({ enabled, activeChatId, onReceiveMessage }: UseChatHubOptions) {
  const connectionRef = useRef<HubConnection | null>(null)
  const [connectionError, setConnectionError] = useState<string | null>(null)
  const [connectionStatus, setConnectionStatus] = useState<
    "connecting" | "connected" | "reconnecting" | "disconnected" | "failed"
  >("disconnected")
  const apiBaseUrl = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "").replace(/\/$/, "")
  const hubUrl = apiBaseUrl ? `${apiBaseUrl}/chatHub` : ""

  useEffect(() => {
    if (!enabled) {
      connectionRef.current = null
      setConnectionStatus("disconnected")
      setConnectionError(null)
      return
    }

    const token = getAccessToken() ?? ""
    console.log("[chat] hubUrl:", hubUrl)
    console.log("[chat] token exists:", Boolean(token))

    if (!hubUrl) {
      setConnectionStatus("failed")
      setConnectionError("Missing NEXT_PUBLIC_API_BASE_URL for chat hub.")
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

    connectionRef.current = hub

    hub.on("ReceiveMessage", (message: MessageDto) => {
      onReceiveMessage?.(message)
    })

    const attachLifecycle = (activeHub: HubConnection) => {
      activeHub.onreconnecting(() => {
        setConnectionStatus("reconnecting")
        setConnectionError("Reconnecting to chat...")
      })

      activeHub.onreconnected(() => {
        setConnectionStatus("connected")
        setConnectionError(null)
      })

      activeHub.onclose((error) => {
        setConnectionStatus("disconnected")
        if (error) setConnectionError("Chat connection closed.")
      })
    }

    attachLifecycle(hub)

    const startPromise = (async () => {
      try {
        setConnectionStatus("connecting")
        setConnectionError(null)
        await hub.start()
        if (cancelled) {
          if (
            hub.state === HubConnectionState.Connected ||
            hub.state === HubConnectionState.Connecting ||
            hub.state === HubConnectionState.Reconnecting
          ) {
            await hub.stop()
          }
          return
        }
        setConnectionStatus("connected")
      } catch (error) {
        if (!cancelled) {
          console.error("[chat] SignalR connect failed:", error)
          setConnectionStatus("failed")
          setConnectionError("Failed to connect to chat.")
        }
      }
    })()

    return () => {
      cancelled = true
      void (async () => {
        try {
          await startPromise
          if (
            hub.state === HubConnectionState.Connected ||
            hub.state === HubConnectionState.Connecting ||
            hub.state === HubConnectionState.Reconnecting
          ) {
            await hub.stop()
          }
        } catch (error) {
          console.warn("[chat] SignalR stop failed:", error)
        }
      })()
    }
  }, [enabled, hubUrl, onReceiveMessage])

  useEffect(() => {
    const hub = connectionRef.current
    if (!hub || hub.state !== HubConnectionState.Connected || !activeChatId) return

    void hub.invoke("JoinChat", activeChatId).catch((error) => {
      console.warn("[chat] JoinChat failed:", error)
      setConnectionError("Failed to join chat.")
    })

    return () => {
      const currentHub = connectionRef.current
      if (currentHub && currentHub.state === HubConnectionState.Connected) {
        void currentHub.invoke("LeaveChat", activeChatId).catch(() => {})
      }
    }
  }, [activeChatId, connectionStatus])

  const api = useMemo(
    () => ({
      isConnected: connectionStatus === "connected",
      connectionStatus,
      connectionError,
      async sendMessageToChat(chatId: number, content: string) {
        const hub = connectionRef.current
        if (!hub || hub.state !== HubConnectionState.Connected) {
          throw new Error("Chat connection is not connected.")
        }
        await hub.invoke("SendMessageToChat", chatId, content)
      },
    }),
    [connectionStatus, connectionError]
  )

  return api
}
