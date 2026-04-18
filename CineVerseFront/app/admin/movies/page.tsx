"use client"

import { FormEvent, useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
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
import { Textarea } from "@/components/ui/textarea"
import {
  createMovie,
  deleteMovie,
  getAllMovies,
  syncMoviesFromTmdb,
  updateMovie,
  type GetAllMoviesResponse,
  type MovieStatus,
  type UpdateMovieBody,
} from "@/lib/api/movies"
import { ApiError } from "@/lib/api/types"

const emptyCreate = {
  title: "",
  description: "",
  country: "",
  ageRating: "PG-13",
  tagline: "",
  releaseDate: new Date().toISOString().slice(0, 10),
  director: "",
  durationMinutes: 120,
  language: "en",
  tmdbId: 0,
}

export default function AdminMoviesPage() {
  const [rows, setRows] = useState<GetAllMoviesResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [search, setSearch] = useState("")
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [editRow, setEditRow] = useState<GetAllMoviesResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateMovieBody>({})
  const [syncPage, setSyncPage] = useState(1)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getAllMovies({
        pageNumber: 1,
        pageSize: 100,
        search: search.trim() || undefined,
        sortBy: "createdAt",
        desc: true,
      })
      setRows(data.items)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load movies.")
    } finally {
      setLoading(false)
    }
  }, [search])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await createMovie({
        ...createForm,
        tmdbId: Number(createForm.tmdbId) || 0,
        durationMinutes: Math.max(1, Math.trunc(Number(createForm.durationMinutes)) || 120),
      })
      setCreateOpen(false)
      setCreateForm(emptyCreate)
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Create failed.")
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
      await updateMovie(editRow.id, editForm)
      setEditRow(null)
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Update failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this movie?")) return
    setBusy(true)
    setError(null)
    try {
      await deleteMovie(id)
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Delete failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onSync() {
    setBusy(true)
    setError(null)
    try {
      await syncMoviesFromTmdb(Math.max(1, syncPage))
      await load()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Sync failed.")
    } finally {
      setBusy(false)
    }
  }

  function openEdit(m: GetAllMoviesResponse) {
    setEditRow(m)
    setEditForm({
      title: m.title,
      description: m.description ?? "",
      country: m.country ?? "",
      durationMinutes: m.durationMinutes,
      language: m.language ?? "",
      status: (m.status as MovieStatus) ?? undefined,
      releaseDate: m.releaseDate ? String(m.releaseDate).slice(0, 10) : undefined,
    })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Movies</h1>
          <p className="text-sm text-muted-foreground">CRUD via /api/movie (ManageMovies).</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Input
            placeholder="Search…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-48"
          />
          <Button type="button" variant="secondary" disabled={busy} onClick={() => void load()}>
            Search
          </Button>
          <Dialog open={createOpen} onOpenChange={setCreateOpen}>
            <DialogTrigger asChild>
              <Button type="button">New movie</Button>
            </DialogTrigger>
            <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
              <form onSubmit={onCreate}>
                <DialogHeader>
                  <DialogTitle>Create movie</DialogTitle>
                </DialogHeader>
                <div className="grid gap-3 py-4">
                  <div className="space-y-1">
                    <Label htmlFor="m-title">Title</Label>
                    <Input
                      id="m-title"
                      required
                      value={createForm.title}
                      onChange={(e) => setCreateForm((p) => ({ ...p, title: e.target.value }))}
                    />
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="m-desc">Description</Label>
                    <Textarea
                      id="m-desc"
                      required
                      value={createForm.description}
                      onChange={(e) => setCreateForm((p) => ({ ...p, description: e.target.value }))}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div className="space-y-1">
                      <Label htmlFor="m-country">Country</Label>
                      <Input
                        id="m-country"
                        required
                        value={createForm.country}
                        onChange={(e) => setCreateForm((p) => ({ ...p, country: e.target.value }))}
                      />
                    </div>
                    <div className="space-y-1">
                      <Label htmlFor="m-lang">Language</Label>
                      <Input
                        id="m-lang"
                        required
                        value={createForm.language}
                        onChange={(e) => setCreateForm((p) => ({ ...p, language: e.target.value }))}
                      />
                    </div>
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div className="space-y-1">
                      <Label htmlFor="m-age">Age rating</Label>
                      <Input
                        id="m-age"
                        required
                        value={createForm.ageRating}
                        onChange={(e) => setCreateForm((p) => ({ ...p, ageRating: e.target.value }))}
                      />
                    </div>
                    <div className="space-y-1">
                      <Label htmlFor="m-tmdb">TMDB id</Label>
                      <Input
                        id="m-tmdb"
                        type="number"
                        required
                        value={createForm.tmdbId || ""}
                        onChange={(e) => setCreateForm((p) => ({ ...p, tmdbId: Number(e.target.value) }))}
                      />
                    </div>
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="m-tag">Tagline</Label>
                    <Input
                      id="m-tag"
                      required
                      value={createForm.tagline}
                      onChange={(e) => setCreateForm((p) => ({ ...p, tagline: e.target.value }))}
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div className="space-y-1">
                      <Label htmlFor="m-rel">Release date</Label>
                      <Input
                        id="m-rel"
                        type="date"
                        required
                        value={createForm.releaseDate}
                        onChange={(e) => setCreateForm((p) => ({ ...p, releaseDate: e.target.value }))}
                      />
                    </div>
                    <div className="space-y-1">
                      <Label htmlFor="m-dur">Duration (min)</Label>
                      <Input
                        id="m-dur"
                        type="number"
                        required
                        min={1}
                        value={createForm.durationMinutes}
                        onChange={(e) =>
                          setCreateForm((p) => ({ ...p, durationMinutes: Number(e.target.value) }))
                        }
                      />
                    </div>
                  </div>
                  <div className="space-y-1">
                    <Label htmlFor="m-dir">Director</Label>
                    <Input
                      id="m-dir"
                      required
                      value={createForm.director}
                      onChange={(e) => setCreateForm((p) => ({ ...p, director: e.target.value }))}
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button type="submit" disabled={busy}>
                    {busy ? "Saving…" : "Create"}
                  </Button>
                </DialogFooter>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </div>

      <Card className="border-border/60 bg-card/50">
        <CardHeader className="flex flex-row flex-wrap items-center justify-between gap-2">
          <CardTitle className="text-base">TMDB sync</CardTitle>
          <div className="flex items-center gap-2">
            <Input
              type="number"
              min={1}
              className="w-20"
              value={syncPage}
              onChange={(e) => setSyncPage(Math.max(1, Number(e.target.value) || 1))}
            />
            <Button type="button" size="sm" variant="outline" disabled={busy} onClick={() => void onSync()}>
              Sync page
            </Button>
          </div>
        </CardHeader>
      </Card>

      {error && <p className="text-sm text-destructive">{error}</p>}

      <Card className="border-border/60 bg-card/50">
        <CardContent className="p-0">
          {loading ? (
            <div className="flex justify-center py-16">
              <Spinner className="h-8 w-8 text-primary" />
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-14">ID</TableHead>
                  <TableHead>Title</TableHead>
                  <TableHead>Year</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((m) => (
                  <TableRow key={m.id}>
                    <TableCell className="font-mono text-xs">{m.id}</TableCell>
                    <TableCell className="font-medium">{m.title}</TableCell>
                    <TableCell>{m.releaseYear ?? "—"}</TableCell>
                    <TableCell>{m.status ?? "—"}</TableCell>
                    <TableCell className="text-right">
                      <Button type="button" size="sm" variant="secondary" onClick={() => openEdit(m)}>
                        Edit
                      </Button>{" "}
                      <Button
                        type="button"
                        size="sm"
                        variant="destructive"
                        disabled={busy}
                        onClick={() => void onDelete(m.id)}
                      >
                        Delete
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!editRow} onOpenChange={(o) => !o && setEditRow(null)}>
        <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
          <form onSubmit={onSaveEdit}>
            <DialogHeader>
              <DialogTitle>Edit movie</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label htmlFor="e-title">Title</Label>
                <Input
                  id="e-title"
                  value={editForm.title ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, title: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="e-desc">Description</Label>
                <Textarea
                  id="e-desc"
                  value={editForm.description ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, description: e.target.value }))}
                />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1">
                  <Label htmlFor="e-country">Country</Label>
                  <Input
                    id="e-country"
                    value={editForm.country ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, country: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="e-lang">Language</Label>
                  <Input
                    id="e-lang"
                    value={editForm.language ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, language: e.target.value }))}
                  />
                </div>
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1">
                  <Label htmlFor="e-rel">Release</Label>
                  <Input
                    id="e-rel"
                    type="date"
                    value={editForm.releaseDate ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, releaseDate: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="e-dur">Duration</Label>
                  <Input
                    id="e-dur"
                    type="number"
                    min={1}
                    value={editForm.durationMinutes ?? ""}
                    onChange={(e) =>
                      setEditForm((p) => ({ ...p, durationMinutes: Number(e.target.value) }))
                    }
                  />
                </div>
              </div>
              <div className="space-y-1">
                <Label htmlFor="e-status">Status</Label>
                <select
                  id="e-status"
                  className="border-input flex h-10 w-full rounded-md border bg-background px-3 text-sm"
                  value={editForm.status ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({
                      ...p,
                      status: (e.target.value || undefined) as MovieStatus | undefined,
                    }))
                  }
                >
                  <option value="">(unchanged)</option>
                  <option value="Released">Released</option>
                  <option value="Upcoming">Upcoming</option>
                  <option value="Cancelled">Cancelled</option>
                  <option value="PostProduction">PostProduction</option>
                </select>
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
