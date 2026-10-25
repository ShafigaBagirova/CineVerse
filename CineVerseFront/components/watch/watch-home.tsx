"use client"

import { useCallback, useEffect, useState, type FormEvent } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { KeyRound, Trash2, Users, Youtube } from "lucide-react"
import { toast } from "sonner"
import { useAuth } from "@/components/providers/auth-provider"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Skeleton } from "@/components/ui/skeleton"
import { useNetflixSupport } from "@/hooks/use-netflix-support"
import { userFacingApiErrorMessage } from "@/lib/api/types"
import {
  deleteWatchRoom,
  getMyWatchRooms,
  joinWatchRoom,
  type WatchRoomListItemDto,
} from "@/lib/api/watch-rooms"
import { cn } from "@/lib/utils"

type SourceTile = {
  id: "youtube" | "netflix"
  name: string
  enabled: boolean
  /** Optional official logo file placed in /public/platforms; the text fallback is used when it is missing. */
  logoSrc: string
  tileClassName: string
  fallback: React.ReactNode
}

const SOURCES: SourceTile[] = [
  {
    id: "youtube",
    name: "YouTube",
    enabled: true,
    logoSrc: "/platforms/youtube.svg",
    tileClassName: "border border-zinc-200 bg-white",
    fallback: (
      <span className="flex items-center gap-2">
        <Youtube className="h-10 w-10 text-red-600" />
        <span className="text-xl font-bold tracking-tight text-zinc-900">YouTube</span>
      </span>
    ),
  },
  {
    id: "netflix",
    name: "Netflix",
    enabled: true,
    logoSrc: "/platforms/netflix.svg",
    tileClassName: "bg-black",
    fallback: <span className="text-2xl font-black uppercase tracking-[0.2em] text-red-600">Netflix</span>,
  },
]

function SourceLogo({ source }: { source: SourceTile }) {
  const [logoFailed, setLogoFailed] = useState(false)

  if (logoFailed) return <>{source.fallback}</>

  return (
    // eslint-disable-next-line @next/next/no-img-element
    <img
      src={source.logoSrc}
      alt={source.name}
      className="max-h-14 max-w-[70%] object-contain"
      onError={() => setLogoFailed(true)}
    />
  )
}

export function WatchHome() {
  const router = useRouter()
  const { status, user } = useAuth()
  const netflixSupport = useNetflixSupport()
  const [rooms, setRooms] = useState<WatchRoomListItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [joinOpen, setJoinOpen] = useState(false)
  const [joinCode, setJoinCode] = useState("")
  const [joining, setJoining] = useState(false)
  const [selectedSource, setSelectedSource] = useState<SourceTile["id"] | null>(null)
  const [roomTitle, setRoomTitle] = useState("")

  const loadRooms = useCallback(async () => {
    try {
      setLoading(true)
      const response = await getMyWatchRooms(1, 50)
      setRooms(response.items)
    } catch (e) {
      toast.error(userFacingApiErrorMessage(e, "Could not load your watch rooms."))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (status === "authenticated") void loadRooms()
    else if (status === "guest") setLoading(false)
  }, [status, loadRooms])

  const handleJoin = async (event: FormEvent) => {
    event.preventDefault()
    const code = joinCode.trim().toUpperCase()
    if (!code) return
    try {
      setJoining(true)
      const room = await joinWatchRoom(code)
      setJoinOpen(false)
      router.push(`/watch/${room.code}`)
    } catch (e) {
      toast.error(userFacingApiErrorMessage(e, "Could not join the watch room."))
    } finally {
      setJoining(false)
    }
  }

  const handleTitleContinue = (event: FormEvent) => {
    event.preventDefault()
    const title = roomTitle.trim()
    if (!title) return
    if (!selectedSource) return
    setSelectedSource(null)
    router.push(`/watch/${selectedSource}?title=${encodeURIComponent(title)}`)
  }

  const handleDelete = async (room: WatchRoomListItemDto) => {
    if (!window.confirm("Delete this watch room?")) return
    try {
      await deleteWatchRoom(room.id)
      setRooms((prev) => prev.filter((r) => r.id !== room.id))
      toast.success("Watch room deleted.")
    } catch (e) {
      toast.error(userFacingApiErrorMessage(e, "Could not delete the watch room."))
    }
  }

  if (status === "guest") {
    return (
      <div className="mx-auto max-w-md px-4 py-16 text-center">
        <Card className="p-8">
          <p className="mb-4 text-sm text-muted-foreground">Sign in to create or join a watch room.</p>
          <Button asChild>
            <Link href="/auth">Sign in</Link>
          </Button>
        </Card>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10">
      <h1 className="mb-1 text-2xl font-semibold">Watch together</h1>
      <p className="mb-6 text-sm text-muted-foreground">
        Pick where to watch, create a room and share its code. Everyone stays in sync with chat and reactions.
      </p>

      <Button
        type="button"
        variant="outline"
        size="lg"
        className="h-14 w-full justify-center gap-2 text-base"
        onClick={() => setJoinOpen(true)}
      >
        <KeyRound className="h-5 w-5" />
        Join with a code
      </Button>

      <h2 className="mb-3 mt-8 text-sm font-medium">Create a room</h2>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
        {SOURCES.map((source) => {
          // Netflix rooms need the browser extension, which phones, Firefox and Safari cannot have.
          const browserUnsupported =
            source.id === "netflix" && (netflixSupport === "mobile" || netflixSupport === "unsupported-browser")
          const available = source.enabled && !browserUnsupported

          return (
            <button
              key={source.id}
              type="button"
              disabled={!available}
              title={browserUnsupported ? "Netflix rooms need Chrome or Edge on a computer" : undefined}
              onClick={() => setSelectedSource(source.id)}
              className={cn(
                "relative flex aspect-[4/3] items-center justify-center rounded-2xl shadow-sm transition",
                source.tileClassName,
                available ? "hover:scale-[1.02] hover:shadow-md" : "cursor-not-allowed opacity-60"
              )}
            >
              <SourceLogo source={source} />
              {!source.enabled || browserUnsupported ? (
                <span className="absolute right-2 top-2 rounded-full bg-zinc-500/40 px-2 py-0.5 text-[10px] font-medium uppercase text-white">
                  {browserUnsupported ? "Chrome or Edge" : "Soon"}
                </span>
              ) : null}
            </button>
          )
        })}
      </div>

      <h2 className="mb-3 mt-8 text-sm font-medium">Your rooms</h2>
      {loading ? (
        <div className="space-y-2">
          <Skeleton className="h-14 w-full" />
          <Skeleton className="h-14 w-full" />
        </div>
      ) : rooms.length === 0 ? (
        <p className="text-sm text-muted-foreground">You are not in any watch room yet.</p>
      ) : (
        <ul className="space-y-2">
          {rooms.map((room) => (
            <li key={room.id}>
              <Card className="flex items-center justify-between gap-3 p-4">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{room.title?.trim() || "Untitled room"}</p>
                  <p className="flex items-center gap-2 text-xs text-muted-foreground">
                    <span>{room.code}</span>
                    <span className="inline-flex items-center gap-1">
                      <Users className="h-3 w-3" />
                      {room.memberCount}
                    </span>
                  </p>
                </div>
                <div className="flex items-center gap-2">
                  <Button asChild size="sm">
                    <Link href={`/watch/${room.code}`}>Open</Link>
                  </Button>
                  {room.hostUserId === user?.userId ? (
                    <Button size="icon" variant="outline" onClick={() => void handleDelete(room)}>
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  ) : null}
                </div>
              </Card>
            </li>
          ))}
        </ul>
      )}

      <Dialog open={joinOpen} onOpenChange={setJoinOpen}>
        <DialogContent>
          <form onSubmit={handleJoin} className="space-y-4">
            <DialogHeader>
              <DialogTitle>Join with a code</DialogTitle>
              <DialogDescription>Enter the room code your friend shared with you.</DialogDescription>
            </DialogHeader>
            <Input
              autoFocus
              value={joinCode}
              onChange={(event) => setJoinCode(event.target.value)}
              placeholder="Room code"
              maxLength={16}
            />
            <DialogFooter>
              <Button type="submit" disabled={joining || !joinCode.trim()}>
                {joining ? "Joining..." : "Join room"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={selectedSource !== null} onOpenChange={(open) => !open && setSelectedSource(null)}>
        <DialogContent>
          <form onSubmit={handleTitleContinue} className="space-y-4">
            <DialogHeader>
              <DialogTitle>Name your room</DialogTitle>
              <DialogDescription>
                Friends will see this name.{" "}
                {selectedSource === "netflix" ? "Next Netflix opens so you can pick something to watch." : "Next you will pick a YouTube video."}
              </DialogDescription>
            </DialogHeader>
            <Input
              autoFocus
              value={roomTitle}
              onChange={(event) => setRoomTitle(event.target.value)}
              placeholder="Room title"
              maxLength={200}
            />
            <DialogFooter>
              <Button type="submit" disabled={!roomTitle.trim()}>
                Continue
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
