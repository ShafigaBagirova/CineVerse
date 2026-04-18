"use client"

import { FormEvent, useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Spinner } from "@/components/ui/spinner"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { getAllMovies, type GetAllMoviesResponse } from "@/lib/api/movies"
import { getAllHalls, type GetAllHallsResponse } from "@/lib/api/halls"
import {
  createScreening,
  deleteScreening,
  getAllScreenings,
  updateScreening,
  type GetAllScreeningsResponse,
  type ScreeningFormat,
  type ScreeningStatus,
  type UpdateScreeningBody,
} from "@/lib/api/screenings"
import { ApiError } from "@/lib/api/types"

/** Same pagination shape as admin halls: pageNumber=1, pageSize=10 via query string. */
const ADMIN_PAGE = { pageNumber: 1, pageSize: 10 } as const

const FORMAT_FROM_NUM: Record<number, ScreeningFormat> = {
  1: "TwoD",
  2: "ThreeD",
  3: "IMAX",
  4: "FourDX",
}

const STATUS_FROM_NUM: Record<number, ScreeningStatus> = {
  1: "Scheduled",
  2: "Cancelled",
  3: "Completed",
}

function mapFormat(v: unknown): ScreeningFormat {
  if (typeof v === "string" && v) return v as ScreeningFormat
  if (typeof v === "number" && FORMAT_FROM_NUM[v]) return FORMAT_FROM_NUM[v]
  return "TwoD"
}

function mapStatus(v: unknown): ScreeningStatus {
  if (typeof v === "string" && v) return v as ScreeningStatus
  if (typeof v === "number" && STATUS_FROM_NUM[v]) return STATUS_FROM_NUM[v]
  return "Scheduled"
}

function pickPaginatedItems<T>(data: unknown): T[] {
  if (data == null || typeof data !== "object") return []
  const o = data as Record<string, unknown>
  const raw = o.items ?? o.Items
  return Array.isArray(raw) ? (raw as T[]) : []
}

function normalizeScreeningRow(raw: unknown): GetAllScreeningsResponse {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {}
  const id = Number(o.id ?? o.Id ?? 0)
  const movieId = Number(o.movieId ?? o.MovieId ?? 0)
  const hallId = Number(o.hallId ?? o.HallId ?? 0)
  const movieTitle = String(o.movieTitle ?? o.MovieTitle ?? "")
  const hallName = String(o.hallName ?? o.HallName ?? "")
  const startRaw = o.startTime ?? o.StartTime
  const endRaw = o.endTime ?? o.EndTime
  const startTime =
    typeof startRaw === "string"
      ? startRaw
      : startRaw instanceof Date
        ? startRaw.toISOString()
        : typeof startRaw === "number"
          ? new Date(startRaw).toISOString()
          : String(startRaw ?? "")
  const endTime =
    typeof endRaw === "string"
      ? endRaw
      : endRaw instanceof Date
        ? endRaw.toISOString()
        : typeof endRaw === "number"
          ? new Date(endRaw).toISOString()
          : String(endRaw ?? "")
  const pr = o.price ?? o.Price
  const price =
    typeof pr === "number"
      ? pr
      : typeof pr === "string" && pr.trim() !== ""
        ? Number(pr)
        : Number.NaN
  const language = String(o.language ?? o.Language ?? "")
  const sub = o.subtitleLanguage ?? o.SubtitleLanguage
  const subtitleLanguage = sub == null || sub === "" ? null : String(sub)
  return {
    id,
    movieId,
    hallId,
    movieTitle,
    hallName,
    startTime,
    endTime,
    price: Number.isFinite(price) ? price : 0,
    language,
    subtitleLanguage,
    format: mapFormat(o.format ?? o.Format),
    status: mapStatus(o.status ?? o.Status),
    isActive: Boolean(o.isActive ?? o.IsActive ?? true),
  }
}

function screeningRowsFromListPayload(data: unknown): GetAllScreeningsResponse[] {
  return pickPaginatedItems<unknown>(data).map(normalizeScreeningRow)
}

function formatScreeningStart(iso: string): string {
  if (!iso) return "—"
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return "—"
  return d.toLocaleString()
}

function getErrorMessage(e: unknown, fallback: string): string {
  if (e instanceof ApiError) return e.message
  if (e instanceof Error && e.message.trim()) return e.message
  if (typeof e === "string" && e.trim()) return e
  return fallback
}

function toIsoLocal(value: string) {
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  return d.toISOString()
}

export default function AdminShowtimesPage() {
  const [rows, setRows] = useState<GetAllScreeningsResponse[]>([])
  const [movies, setMovies] = useState<{ id: number; title: string }[]>([])
  const [halls, setHalls] = useState<{ id: number; name: string }[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState({
    movieId: 0,
    hallId: 0,
    startLocal: "",
    endLocal: "",
    price: 12.5,
    language: "en",
    subtitleLanguage: "",
    format: "TwoD" as ScreeningFormat,
  })
  const [editRow, setEditRow] = useState<GetAllScreeningsResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateScreeningBody>({})

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [s, m, h] = await Promise.all([
        getAllScreenings({ ...ADMIN_PAGE }, { quiet: true }),
        getAllMovies({ ...ADMIN_PAGE, sortBy: "title", desc: false }, { quiet: true }),
        getAllHalls(ADMIN_PAGE.pageNumber, ADMIN_PAGE.pageSize, { quiet: true }),
      ])
      setRows(screeningRowsFromListPayload(s))
      const movieRows = pickPaginatedItems<GetAllMoviesResponse>(m)
      setMovies(
        movieRows.map((x) => ({
          id: Number(x.id ?? (x as { Id?: number }).Id ?? 0),
          title: String(x.title ?? (x as { Title?: string }).Title ?? ""),
        })),
      )
      const hallRows = pickPaginatedItems<GetAllHallsResponse>(h)
      setHalls(
        hallRows.map((x) => ({
          id: Number(x.id ?? (x as { Id?: number }).Id ?? 0),
          name: String(x.name ?? (x as { Name?: string }).Name ?? ""),
        })),
      )
    } catch (e) {
      setError(getErrorMessage(e, "Failed to load showtimes."))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (movies.length && createForm.movieId === 0) {
      setCreateForm((f) => ({ ...f, movieId: movies[0].id }))
    }
    if (halls.length && createForm.hallId === 0) {
      setCreateForm((f) => ({ ...f, hallId: halls[0].id }))
    }
  }, [movies, halls, createForm.movieId, createForm.hallId])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await createScreening({
        movieId: createForm.movieId,
        hallId: createForm.hallId,
        startTime: toIsoLocal(createForm.startLocal),
        endTime: toIsoLocal(createForm.endLocal),
        price: Number(createForm.price),
        language: createForm.language.trim(),
        subtitleLanguage: createForm.subtitleLanguage.trim() || undefined,
        format: createForm.format,
      })
      setCreateOpen(false)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    setBusy(true)
    setError(null)
    try {
      const payload: UpdateScreeningBody = { ...editForm }
      if (editForm.startTime) payload.startTime = toIsoLocal(editForm.startTime as string)
      if (editForm.endTime) payload.endTime = toIsoLocal(editForm.endTime as string)
      await updateScreening(editRow.id, payload)
      setEditRow(null)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this screening?")) return
    setBusy(true)
    try {
      await deleteScreening(id)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, "Delete failed."))
    } finally {
      setBusy(false)
    }
  }

  function openEdit(s: GetAllScreeningsResponse) {
    setEditRow(s)
    const start = s.startTime ? new Date(s.startTime).toISOString().slice(0, 16) : ""
    const end = s.endTime ? new Date(s.endTime).toISOString().slice(0, 16) : ""
    setEditForm({
      status: s.status,
      isActive: s.isActive,
      price: Number(s.price),
      language: s.language,
      subtitleLanguage: s.subtitleLanguage ?? undefined,
      format: s.format,
      startTime: start,
      endTime: end,
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Showtimes</h1>
          <p className="text-sm text-muted-foreground">/api/screening — ManageScreenings.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New screening</Button>
          </DialogTrigger>
          <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-md">
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create screening</DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                <div className="space-y-1">
                  <Label>Movie</Label>
                  <select
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={createForm.movieId}
                    onChange={(e) =>
                      setCreateForm((p) => ({ ...p, movieId: Number(e.target.value) }))
                    }
                  >
                    {movies.map((m) => (
                      <option key={m.id} value={m.id}>
                        {m.title}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1">
                  <Label>Hall</Label>
                  <select
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={createForm.hallId}
                    onChange={(e) =>
                      setCreateForm((p) => ({ ...p, hallId: Number(e.target.value) }))
                    }
                  >
                    {halls.map((h) => (
                      <option key={h.id} value={h.id}>
                        {h.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label htmlFor="ss-start">Start</Label>
                    <Input
                      id="ss-start"
                      type="datetime-local"
                      required
                      value={createForm.startLocal}
                      onChange={(e) =>
                        setCreateForm((p) => ({ ...p, startLocal: e.target.value }))
                      }
                    />
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="ss-end">End</Label>
                    <Input
                      id="ss-end"
                      type="datetime-local"
                      required
                      value={createForm.endLocal}
                      onChange={(e) => setCreateForm((p) => ({ ...p, endLocal: e.target.value }))}
                    />
                  </div>
                </div>
                <div className="space-y-1">
                  <Label htmlFor="ss-price">Price</Label>
                  <Input
                    id="ss-price"
                    type="number"
                    step="0.01"
                    min={0}
                    value={createForm.price}
                    onChange={(e) =>
                      setCreateForm((p) => ({ ...p, price: Number(e.target.value) }))
                    }
                  />
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label htmlFor="ss-lang">Language</Label>
                    <Input
                      id="ss-lang"
                      value={createForm.language}
                      onChange={(e) =>
                        setCreateForm((p) => ({ ...p, language: e.target.value }))
                      }
                    />
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="ss-sub">Subtitles</Label>
                    <Input
                      id="ss-sub"
                      value={createForm.subtitleLanguage}
                      onChange={(e) =>
                        setCreateForm((p) => ({ ...p, subtitleLanguage: e.target.value }))
                      }
                    />
                  </div>
                </div>
                <div className="space-y-1">
                  <Label htmlFor="ss-fmt">Format</Label>
                  <select
                    id="ss-fmt"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={createForm.format}
                    onChange={(e) =>
                      setCreateForm((p) => ({ ...p, format: e.target.value as ScreeningFormat }))
                    }
                  >
                    <option value="TwoD">2D</option>
                    <option value="ThreeD">3D</option>
                    <option value="IMAX">IMAX</option>
                    <option value="FourDX">4DX</option>
                  </select>
                </div>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={busy}>
                  Create
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <Card className="border-border/60 bg-card/50">
        <CardContent className="p-0">
          {loading ? (
            <div className="flex justify-center py-16">
              <Spinner className="h-8 w-8 text-primary" />
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-12">ID</TableHead>
                    <TableHead>Movie</TableHead>
                    <TableHead>Hall</TableHead>
                    <TableHead>Start</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {rows.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell className="font-mono text-xs">{s.id}</TableCell>
                      <TableCell className="max-w-[140px] truncate">{s.movieTitle}</TableCell>
                      <TableCell className="max-w-[100px] truncate">{s.hallName}</TableCell>
                      <TableCell className="whitespace-nowrap text-xs">{formatScreeningStart(s.startTime)}</TableCell>
                      <TableCell>{String(s.status)}</TableCell>
                      <TableCell className="text-right">
                        <Button type="button" size="sm" variant="secondary" onClick={() => openEdit(s)}>
                          Edit
                        </Button>{" "}
                        <Button
                          type="button"
                          size="sm"
                          variant="destructive"
                          disabled={busy}
                          onClick={() => void onDelete(s.id)}
                        >
                          Delete
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!editRow} onOpenChange={(o) => !o && setEditRow(null)}>
        <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-md">
          <form onSubmit={onSaveEdit}>
            <DialogHeader>
              <DialogTitle>Edit screening</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1">
                  <Label>Start</Label>
                  <Input
                    type="datetime-local"
                    value={(editForm.startTime as string) ?? ""}
                    onChange={(e) =>
                      setEditForm((p) => ({ ...p, startTime: e.target.value }))
                    }
                  />
                </div>
                <div className="space-y-1">
                  <Label>End</Label>
                  <Input
                    type="datetime-local"
                    value={(editForm.endTime as string) ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, endTime: e.target.value }))}
                  />
                </div>
              </div>
              <div className="space-y-1">
                <Label>Price</Label>
                <Input
                  type="number"
                  step="0.01"
                  value={editForm.price ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({ ...p, price: Number(e.target.value) }))
                  }
                />
              </div>
              <div className="space-y-1">
                <Label>Status</Label>
                <select
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  value={editForm.status ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({
                      ...p,
                      status: (e.target.value || undefined) as ScreeningStatus | undefined,
                    }))
                  }
                >
                  <option value="Scheduled">Scheduled</option>
                  <option value="Cancelled">Cancelled</option>
                  <option value="Completed">Completed</option>
                </select>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="ss-active"
                  checked={editForm.isActive ?? true}
                  onChange={(e) =>
                    setEditForm((p) => ({ ...p, isActive: e.target.checked }))
                  }
                />
                <Label htmlFor="ss-active">Active</Label>
              </div>
            </div>
            <DialogFooter>
              <Button type="submit" disabled={busy}>
                Save
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
