"use client"

import { useCallback, useEffect, useMemo, useRef, useState, type KeyboardEvent } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { format } from "date-fns"
import { Check, Copy, Play, Send, Users, Wifi, WifiOff } from "lucide-react"
import { toast } from "sonner"
import { useAuth } from "@/components/providers/auth-provider"
import { EmojiPicker } from "@/components/watch/emoji-picker"
import { NetflixBridgePlayer } from "@/components/watch/netflix-bridge-player"
import type { PlayerHandle } from "@/components/watch/player-types"
import { YouTubePlayer, YT_STATE } from "@/components/watch/youtube-player"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Skeleton } from "@/components/ui/skeleton"
import { useWatchRoomHub, type WatchRoomHubHandlers } from "@/hooks/use-watchroom-hub"
import type {
  PlaybackState,
  WatchAction,
  WatchChatMessage,
  WatchReaction,
  WatchRoomDto,
} from "@/lib/api/watch-rooms"
import { listenToExtension, sendToExtension, type OverlayChatMessage } from "@/lib/netflix-bridge"
import { cn } from "@/lib/utils"

const REACTION_EMOJIS = ["❤️", "😂", "😮", "😢", "👏", "🔥"]
const POLL_INTERVAL_MS = 250
const SEEK_JUMP_SECONDS = 1.5
const REMOTE_SUPPRESS_MS = 1200
const AUTOPLAY_CHECK_MS = 1500
const HEARTBEAT_INTERVAL_MS = 5000
const HEARTBEAT_DRIFT_SECONDS = 2.5
const REACTION_LIFETIME_MS = 4000
const MAX_MESSAGES = 200
const MAX_MESSAGE_LENGTH = 500
const OVERLAY_HISTORY_SIZE = 30

type FloatingReaction = WatchReaction & { id: number }

function toOverlayMessage(message: WatchChatMessage, userId: string | null): OverlayChatMessage {
  return {
    senderName: message.senderName?.trim() || "Someone",
    mine: message.senderId === userId,
    content: message.content,
    sentAtUtc: message.sentAtUtc,
  }
}

export function WatchRoom({ code }: { code: string }) {
  const router = useRouter()
  const { status: authStatus, user } = useAuth()
  const [room, setRoom] = useState<WatchRoomDto | null>(null)
  const [onlineUserIds, setOnlineUserIds] = useState<Set<string>>(new Set())
  const [messages, setMessages] = useState<WatchChatMessage[]>([])
  const [reactions, setReactions] = useState<FloatingReaction[]>([])
  const [draft, setDraft] = useState("")
  const [playerReady, setPlayerReady] = useState(false)
  const [needsInteraction, setNeedsInteraction] = useState(false)
  const [copyState, setCopyState] = useState<"idle" | "copied" | "failed">("idle")

  const playerRef = useRef<PlayerHandle>(null)
  const playerReadyRef = useRef(false)
  const pendingPlaybackRef = useRef<PlaybackState | null>(null)
  const suppressUntilRef = useRef(0)
  const lastKnownRef = useRef({ time: 0, wall: Date.now(), playing: false })
  const desiredRef = useRef({ position: 0, wall: Date.now(), playing: false })
  const autoplayCheckRef = useRef<number | null>(null)
  const copyResetRef = useRef<number | null>(null)
  const reactionIdRef = useRef(0)
  const messagesEndRef = useRef<HTMLDivElement | null>(null)
  const draftInputRef = useRef<HTMLInputElement | null>(null)
  const autoplayPromptRef = useRef(true)
  const isNetflixRef = useRef(false)
  const userIdRef = useRef<string | null>(null)
  const messagesRef = useRef<WatchChatMessage[]>([])

  const isNetflix = room?.platform === "Netflix"

  const scheduleAutoplayCheck = useCallback(() => {
    if (autoplayCheckRef.current !== null) window.clearTimeout(autoplayCheckRef.current)
    autoplayCheckRef.current = window.setTimeout(() => {
      autoplayCheckRef.current = null
      const player = playerRef.current
      if (!autoplayPromptRef.current || !player || !desiredRef.current.playing) return
      const state = player.getState()
      if (state !== YT_STATE.PLAYING && state !== YT_STATE.BUFFERING) setNeedsInteraction(true)
    }, AUTOPLAY_CHECK_MS)
  }, [])

  const applyRemote = useCallback(
    (state: PlaybackState, action?: WatchAction) => {
      const player = playerRef.current
      if (!player || !playerReadyRef.current) {
        pendingPlaybackRef.current = state
        desiredRef.current = { position: state.positionSeconds, wall: Date.now(), playing: state.isPlaying }
        return
      }

      suppressUntilRef.current = Date.now() + REMOTE_SUPPRESS_MS
      const target = state.positionSeconds
      const now = Date.now()

      if (action === "seek") {
        player.seekTo(target)
        desiredRef.current = { position: target, wall: now, playing: desiredRef.current.playing }
        lastKnownRef.current = { time: target, wall: now, playing: lastKnownRef.current.playing }
        return
      }

      if (Math.abs(player.getCurrentTime() - target) > 1.5) player.seekTo(target)
      if (state.isPlaying) player.play()
      else player.pause()

      desiredRef.current = { position: target, wall: now, playing: state.isPlaying }
      lastKnownRef.current = { time: target, wall: now, playing: state.isPlaying }
      if (state.isPlaying) scheduleAutoplayCheck()
      else setNeedsInteraction(false)
    },
    [scheduleAutoplayCheck]
  )

  const handlers = useMemo<WatchRoomHubHandlers>(
    () => ({
      onSnapshot: (snapshot) => {
        setRoom(snapshot.room)
        setOnlineUserIds(new Set(snapshot.onlineUserIds))
        applyRemote(snapshot.playback)
      },
      onStateChanged: (event) => applyRemote(event.playback, event.action),
      onHeartbeat: (event) => {
        const player = playerRef.current
        if (!player || !playerReadyRef.current || !player.isPlaying()) return

        const target = event.playback.positionSeconds
        const now = Date.now()
        desiredRef.current = { position: target, wall: now, playing: true }

        if (Math.abs(player.getCurrentTime() - target) > HEARTBEAT_DRIFT_SECONDS) {
          suppressUntilRef.current = now + REMOTE_SUPPRESS_MS
          player.seekTo(target)
          lastKnownRef.current = { time: target, wall: now, playing: true }
        }
      },
      onMessage: (message) => {
        setMessages((prev) => [...prev, message].slice(-MAX_MESSAGES))
        if (isNetflixRef.current) {
          sendToExtension({ type: "CHAT_MESSAGE", payload: toOverlayMessage(message, userIdRef.current) })
        }
      },
      onReaction: (reaction) => {
        const id = ++reactionIdRef.current
        setReactions((prev) => [...prev, { ...reaction, id }])
        window.setTimeout(() => setReactions((prev) => prev.filter((r) => r.id !== id)), REACTION_LIFETIME_MS)
        if (isNetflixRef.current) {
          sendToExtension({
            type: "REACTION",
            payload: { emoji: reaction.emoji, senderName: reaction.senderName?.trim() || "" },
          })
        }
      },
      onMemberJoined: (member) => {
        setOnlineUserIds((prev) => new Set(prev).add(member.userId))
        setRoom((prev) =>
          prev && !prev.members.some((m) => m.userId === member.userId)
            ? {
                ...prev,
                members: [
                  ...prev.members,
                  { userId: member.userId, userName: member.userName, joinedAt: new Date().toISOString() },
                ],
              }
            : prev
        )
      },
      onMemberLeft: (member) =>
        setOnlineUserIds((prev) => {
          const next = new Set(prev)
          next.delete(member.userId)
          return next
        }),
      onRoomClosed: () => {
        toast.info("The host closed this watch room.")
        router.push("/watch")
      },
    }),
    [applyRemote, router]
  )

  const hub = useWatchRoomHub({ code, enabled: authStatus === "authenticated", handlers })
  const hubActionsRef = useRef({
    syncState: hub.syncState,
    sendHeartbeat: hub.sendHeartbeat,
    sendMessage: hub.sendMessage,
    sendReaction: hub.sendReaction,
  })

  useEffect(() => {
    hubActionsRef.current = {
      syncState: hub.syncState,
      sendHeartbeat: hub.sendHeartbeat,
      sendMessage: hub.sendMessage,
      sendReaction: hub.sendReaction,
    }
  }, [hub.syncState, hub.sendHeartbeat, hub.sendMessage, hub.sendReaction])

  const isHost = !!room && !!user && room.hostUserId === user.userId

  useEffect(() => {
    autoplayPromptRef.current = !isNetflix
    isNetflixRef.current = isNetflix
  }, [isNetflix])

  useEffect(() => {
    userIdRef.current = user?.userId ?? null
  }, [user?.userId])

  useEffect(() => {
    messagesRef.current = messages
  }, [messages])

  // In a Netflix room the chat also lives in a panel inside the Netflix window. The panel gets the room's
  // details here, and what is typed or reacted there is sent to the room like it was typed on this page.
  const overlayTitle = room?.title?.trim() || "Watch room"

  useEffect(() => {
    if (!isNetflix) return

    const sendRoomDetails = () => {
      sendToExtension({ type: "ROOM_INFO", payload: { title: overlayTitle, code } })
      sendToExtension({
        type: "CHAT_HISTORY",
        payload: {
          messages: messagesRef.current.slice(-OVERLAY_HISTORY_SIZE).map((m) => toOverlayMessage(m, userIdRef.current)),
        },
      })
    }

    sendRoomDetails()

    return listenToExtension((message) => {
      if (message.type === "OVERLAY_READY") {
        sendRoomDetails()
      } else if (message.type === "SEND_CHAT" && typeof message.content === "string") {
        const content = message.content.trim().slice(0, MAX_MESSAGE_LENGTH)
        if (content) void hubActionsRef.current.sendMessage(content)
      } else if (message.type === "SEND_REACTION" && REACTION_EMOJIS.includes(message.emoji)) {
        void hubActionsRef.current.sendReaction(message.emoji)
      }
    })
  }, [isNetflix, overlayTitle, code])

  const handlePlayerReady = useCallback(() => {
    playerReadyRef.current = true
    setPlayerReady(true)
    if (pendingPlaybackRef.current) {
      const pending = pendingPlaybackRef.current
      pendingPlaybackRef.current = null

      // The room kept playing while the player was loading, so catch up to where it is now.
      const desired = desiredRef.current
      const elapsed = desired.playing ? (Date.now() - desired.wall) / 1000 : 0
      applyRemote({ ...pending, positionSeconds: desired.position + elapsed })
    }
  }, [applyRemote])

  const handlePlayerUnready = useCallback(() => {
    playerReadyRef.current = false
    setPlayerReady(false)
  }, [])

  const handlePlayerSeek = useCallback((time: number) => {
    if (Date.now() < suppressUntilRef.current) return

    const now = Date.now()
    const playing = playerRef.current?.isPlaying() ?? lastKnownRef.current.playing
    lastKnownRef.current = { time, wall: now, playing }
    desiredRef.current = { position: time, wall: now, playing }
    void hubActionsRef.current.syncState("seek", time)
  }, [])

  const handlePlayerStateChange = useCallback((state: number) => {
    if (state === YT_STATE.PLAYING) setNeedsInteraction(false)
    if (Date.now() < suppressUntilRef.current) return

    const player = playerRef.current
    if (!player) return

    const time = player.getCurrentTime()
    if (state === YT_STATE.PLAYING) {
      lastKnownRef.current = { time, wall: Date.now(), playing: true }
      desiredRef.current = { position: time, wall: Date.now(), playing: true }
      void hubActionsRef.current.syncState("play", time)
    } else if (state === YT_STATE.PAUSED) {
      lastKnownRef.current = { time, wall: Date.now(), playing: false }
      desiredRef.current = { position: time, wall: Date.now(), playing: false }
      void hubActionsRef.current.syncState("pause", time)
    }
  }, [])

  const handleSyncClick = () => {
    const player = playerRef.current
    if (!player) return

    const desired = desiredRef.current
    const now = Date.now()
    const target = desired.playing ? desired.position + (now - desired.wall) / 1000 : desired.position

    suppressUntilRef.current = now + REMOTE_SUPPRESS_MS
    player.seekTo(target)
    if (desired.playing) player.play()
    else player.pause()
    lastKnownRef.current = { time: target, wall: now, playing: desired.playing }
    setNeedsInteraction(false)
  }

  // YouTube has no "seeked" event, so jumps while paused are found by comparing the playhead with where it should be.
  useEffect(() => {
    if (!playerReady) return

    const intervalId = window.setInterval(() => {
      const player = playerRef.current
      if (!player || Date.now() < suppressUntilRef.current) return

      const now = Date.now()
      const last = lastKnownRef.current
      const actual = player.getCurrentTime()

      if (player.getState() === YT_STATE.BUFFERING) {
        lastKnownRef.current = { time: actual, wall: now, playing: false }
        return
      }

      const expected = last.playing ? last.time + (now - last.wall) / 1000 : last.time
      if (Math.abs(actual - expected) > SEEK_JUMP_SECONDS) {
        void hubActionsRef.current.syncState("seek", actual)
      }
      lastKnownRef.current = { time: actual, wall: now, playing: player.isPlaying() }
    }, POLL_INTERVAL_MS)

    return () => window.clearInterval(intervalId)
  }, [playerReady])

  // The host periodically shares its position so everyone else can correct drift (for example after an ad).
  useEffect(() => {
    if (!playerReady || !isHost || !hub.isConnected) return

    const intervalId = window.setInterval(() => {
      const player = playerRef.current
      if (!player || !player.isPlaying() || Date.now() < suppressUntilRef.current) return
      void hubActionsRef.current.sendHeartbeat(player.getCurrentTime())
    }, HEARTBEAT_INTERVAL_MS)

    return () => window.clearInterval(intervalId)
  }, [playerReady, isHost, hub.isConnected])

  useEffect(() => {
    return () => {
      if (autoplayCheckRef.current !== null) window.clearTimeout(autoplayCheckRef.current)
      if (copyResetRef.current !== null) window.clearTimeout(copyResetRef.current)
    }
  }, [])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth", block: "end" })
  }, [messages])

  const submitMessage = async () => {
    const text = draft.trim()
    if (!text) return
    setDraft("")
    await hub.sendMessage(text)
  }

  const handleDraftKeyDown = (event: KeyboardEvent<HTMLInputElement>) => {
    if (event.key === "Enter") {
      event.preventDefault()
      void submitMessage()
    }
  }

  const insertEmoji = (emoji: string) => {
    const input = draftInputRef.current
    const start = input?.selectionStart ?? draft.length
    const end = input?.selectionEnd ?? draft.length
    const next = draft.slice(0, start) + emoji + draft.slice(end)
    if (next.length > MAX_MESSAGE_LENGTH) return

    setDraft(next)
    const caret = start + emoji.length
    window.requestAnimationFrame(() => {
      input?.focus()
      input?.setSelectionRange(caret, caret)
    })
  }

  const copyCode = async () => {
    try {
      await navigator.clipboard.writeText(code)
      setCopyState("copied")
    } catch {
      setCopyState("failed")
    }

    if (copyResetRef.current !== null) window.clearTimeout(copyResetRef.current)
    copyResetRef.current = window.setTimeout(() => setCopyState("idle"), 2000)
  }

  if (authStatus === "loading") {
    return (
      <div className="mx-auto max-w-7xl px-4 py-6">
        <Skeleton className="aspect-video w-full" />
      </div>
    )
  }

  if (authStatus !== "authenticated") {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <Card className="p-8">
          <p className="mb-4 text-sm text-muted-foreground">Sign in to join this watch room.</p>
          <Button asChild>
            <Link href="/auth">Sign in</Link>
          </Button>
        </Card>
      </div>
    )
  }

  if (!room && (hub.status === "failed" || hub.status === "disconnected") && hub.error) {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <Card className="p-8">
          <p className="mb-4 text-sm text-destructive">{hub.error}</p>
          <Button asChild variant="outline">
            <Link href="/watch">Back to watch rooms</Link>
          </Button>
        </Card>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-6">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold">{room?.title?.trim() || "Watch room"}</h1>
          <p className="text-xs text-muted-foreground">Code: {code}</p>
        </div>
        <div className="flex items-center gap-2">
          <span
            className={cn(
              "inline-flex items-center gap-1 rounded-full border px-2 py-1 text-xs",
              hub.isConnected ? "text-emerald-600" : "text-amber-600"
            )}
          >
            {hub.isConnected ? <Wifi className="h-3 w-3" /> : <WifiOff className="h-3 w-3" />}
            {hub.status}
          </span>
          <Button
            size="sm"
            variant="outline"
            onClick={copyCode}
            className={cn(
              copyState === "copied" && "border-emerald-500 text-emerald-600",
              copyState === "failed" && "border-destructive text-destructive"
            )}
          >
            {copyState === "copied" ? <Check className="mr-1 h-4 w-4" /> : <Copy className="mr-1 h-4 w-4" />}
            {copyState === "copied" ? "Copied!" : copyState === "failed" ? "Copy failed" : "Copy code"}
          </Button>
        </div>
      </div>

      {hub.error && room ? <p className="mb-3 text-sm text-destructive">{hub.error}</p> : null}

      <div className="grid gap-4 lg:grid-cols-[1fr_340px]">
        <div>
          <div className="relative aspect-video overflow-hidden rounded-xl bg-black">
            {room ? (
              isNetflix ? (
                <NetflixBridgePlayer
                  contentId={room.contentId}
                  contentUrl={room.contentUrl}
                  onReady={handlePlayerReady}
                  onUnready={handlePlayerUnready}
                  onStateChange={handlePlayerStateChange}
                  onSeek={handlePlayerSeek}
                  ref={playerRef}
                />
              ) : (
                <YouTubePlayer
                  videoId={room.contentId}
                  onReady={handlePlayerReady}
                  onStateChange={handlePlayerStateChange}
                  ref={playerRef}
                />
              )
            ) : (
              <Skeleton className="absolute inset-0 rounded-none" />
            )}

            {needsInteraction ? (
              <div className="absolute inset-0 z-10 flex items-center justify-center bg-black/70">
                <Button size="lg" onClick={handleSyncClick}>
                  <Play className="mr-2 h-5 w-5" />
                  Click to sync
                </Button>
              </div>
            ) : null}

            <div className="pointer-events-none absolute bottom-3 right-3 z-20 flex flex-col items-end gap-1">
              {reactions.map((reaction) => (
                <span
                  key={reaction.id}
                  className="rounded-full bg-black/60 px-3 py-1 text-sm text-white backdrop-blur"
                >
                  {reaction.emoji} {reaction.senderName ?? ""}
                </span>
              ))}
            </div>
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-2">
            {REACTION_EMOJIS.map((emoji) => (
              <Button
                key={emoji}
                type="button"
                variant="outline"
                size="sm"
                disabled={!hub.isConnected}
                onClick={() => void hub.sendReaction(emoji)}
              >
                {emoji}
              </Button>
            ))}
          </div>
          <p className="mt-2 text-xs text-muted-foreground">
            {isNetflix
              ? "Play, pause and seeking in your Netflix window are shared with everyone in the room. The chat button inside the Netflix window opens this same chat. Keep this page open."
              : "Play, pause and seeking are shared with everyone in the room. If the browser blocks autoplay, press \"Click to sync\" once."}
          </p>
        </div>

        <aside className="flex flex-col gap-4">
          <Card className="p-4">
            <div className="mb-2 flex items-center gap-2 text-sm font-medium">
              <Users className="h-4 w-4" />
              Members ({room?.members.length ?? 0})
            </div>
            <ul className="space-y-1 text-sm">
              {(room?.members ?? []).map((member) => (
                <li key={member.userId} className="flex items-center gap-2">
                  <span
                    className={cn(
                      "h-2 w-2 rounded-full",
                      onlineUserIds.has(member.userId) ? "bg-emerald-500" : "bg-muted-foreground/40"
                    )}
                  />
                  <span className="truncate">
                    {member.userName?.trim() || member.userId}
                    {member.userId === user?.userId ? " (you)" : ""}
                    {member.userId === room?.hostUserId ? " · host" : ""}
                  </span>
                </li>
              ))}
            </ul>
          </Card>

          <Card className="flex h-[360px] flex-col p-0">
            <ScrollArea className="min-h-0 flex-1 px-4 py-3">
              {messages.length === 0 ? (
                <p className="text-center text-xs text-muted-foreground">No messages yet.</p>
              ) : (
                <div className="space-y-2">
                  {messages.map((message, index) => {
                    const mine = message.senderId === user?.userId
                    return (
                      <div key={`${message.sentAtUtc}-${index}`} className={cn("flex flex-col", mine && "items-end")}>
                        <span className="text-[11px] text-muted-foreground">
                          {mine ? "You" : message.senderName?.trim() || message.senderId} ·{" "}
                          {format(new Date(message.sentAtUtc), "HH:mm")}
                        </span>
                        <span
                          className={cn(
                            "max-w-[85%] break-words rounded-lg px-3 py-1.5 text-sm",
                            mine ? "bg-primary text-primary-foreground" : "bg-muted"
                          )}
                        >
                          {message.content}
                        </span>
                      </div>
                    )
                  })}
                  <div ref={messagesEndRef} />
                </div>
              )}
            </ScrollArea>
            <div className="flex items-center gap-2 border-t p-3">
              <EmojiPicker onSelect={insertEmoji} disabled={!hub.isConnected} />
              <Input
                ref={draftInputRef}
                value={draft}
                onChange={(event) => setDraft(event.target.value)}
                onKeyDown={handleDraftKeyDown}
                placeholder="Write a message"
                maxLength={MAX_MESSAGE_LENGTH}
                disabled={!hub.isConnected}
              />
              <Button size="icon" onClick={() => void submitMessage()} disabled={!hub.isConnected || !draft.trim()}>
                <Send className="h-4 w-4" />
              </Button>
            </div>
          </Card>
        </aside>
      </div>
    </div>
  )
}
