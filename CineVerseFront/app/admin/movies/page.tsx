"use client"

import { FormEvent, useCallback, useEffect, useState } from "react"
import { useRouter } from "next/navigation"
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
import { getAllGenres, type GetAllGenresResponse } from "@/lib/api/genres"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

function adminDeleteErrorMessage(err: unknown): string {
  if (err instanceof ApiError) {
    const primary = err.message.trim()
    const fromErrors = err.errors?.filter((s) => s.trim()).join(" · ") ?? ""
    const combined = [primary, fromErrors].filter(Boolean).join(" · ").trim()
    if (combined) return combined
    return `Request failed (HTTP ${err.status}).`
  }
  return userFacingApiErrorMessage(err, "Delete failed.")
}

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
  const router = useRouter()
  const [rows, setRows] = useState<GetAllMoviesResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [search, setSearch] = useState("")
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [genreOptions, setGenreOptions] = useState<GetAllGenresResponse[]>([])
  const [selectedGenreIds, setSelectedGenreIds] = useState<number[]>([])
  const [editRow, setEditRow] = useState<GetAllMoviesResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateMovieBody>({})
  const [syncPage, setSyncPage] = useState(1)
  const [syncing, setSyncing] = useState(false)
  const [deletingId, setDeletingId] = useState<number | null>(null)

  const loadMovies = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) {
      setLoading(true)
    }
    setError(null)
    try {
      const data = await getAllMovies(
        {
          pageNumber: 1,
          pageSize: 100,
          search: search.trim() || undefined,
          sortBy: "createdAt",
          desc: true,
        },
        { quiet: opts?.silent, auth: true },
      )
      setRows([...data.items])
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load movies.")
    } finally {
      if (!opts?.silent) {
        setLoading(false)
      }
    }
  }, [search])

  function scheduleRouterRefresh() {
    setTimeout(() => {
      router.refresh()
    }, 0)
  }

  useEffect(() => {
    void loadMovies()
  }, [loadMovies])

  useEffect(() => {
    async function loadGenres() {
      try {
        const data = await getAllGenres()
        setGenreOptions(Array.isArray(data) ? data : [])
      } catch {
        setGenreOptions([])
      }
    }
    void loadGenres()
  }, [])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    try {
      await createMovie({
        ...createForm,
        tmdbId: Number(createForm.tmdbId) || 0,
        durationMinutes: Math.max(1, Math.trunc(Number(createForm.durationMinutes)) || 120),
        genreIds: selectedGenreIds,
      })
      setCreateOpen(false)
      setCreateForm(emptyCreate)
      setSelectedGenreIds([])
      await loadMovies({ silent: true })
      scheduleRouterRefresh()
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
      await loadMovies({ silent: true })
      scheduleRouterRefresh()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Update failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number | string) {
    const movieId = Math.trunc(Number(id))
    if (!Number.isFinite(movieId) || movieId <= 0) {
      setError("Invalid movie id. Refresh the page and try again.")
      return
    }
    if (!window.confirm("Delete this movie?")) return
    setBusy(true)
    setDeletingId(movieId)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteMovie(movieId)
      setSuccessMessage("Movie deleted.")
      setRows((prev) => prev.filter((movie) => movie.id !== movieId))
      await loadMovies()
      scheduleRouterRefresh()
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setError(adminDeleteErrorMessage(err))
        await loadMovies({ silent: true })
        scheduleRouterRefresh()
      } else {
        setError(adminDeleteErrorMessage(err))
      }
    } finally {
      setDeletingId(null)
      setBusy(false)
    }
  }

  async function onSync() {
    setSyncing(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await syncMoviesFromTmdb(Math.max(1, syncPage))
      setSuccessMessage("Movies synced from TMDB.")
      await loadMovies({ silent: true })
      scheduleRouterRefresh()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Sync failed.")
    } finally {
      setSyncing(false)
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

  function toggleCreateGenre(genreId: number) {
    setSelectedGenreIds((prev) =>
      prev.includes(genreId) ? prev.filter((id) => id !== genreId) : [...prev, genreId],
    )
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
          <Button type="button" variant="secondary" disabled={busy} onClick={() => void loadMovies()}>
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
                  <div className="space-y-2">
                    <Label>Genres</Label>
                    <div className="max-h-32 space-y-2 overflow-y-auto rounded-md border border-input p-2">
                      {genreOptions.length === 0 ? (
                        <p className="text-xs text-muted-foreground">No genres available.</p>
                      ) : (
                        genreOptions.map((g) => (
                          <label key={g.id} className="flex items-center gap-2 text-sm">
                            <input
                              type="checkbox"
                              checked={selectedGenreIds.includes(g.id)}
                              onChange={() => toggleCreateGenre(g.id)}
                              disabled={busy}
                            />
                            <span>{g.name}</span>
                          </label>
                        ))
                      )}
                    </div>
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
              disabled={syncing}
              onChange={(e) => setSyncPage(Math.max(1, Number(e.target.value) || 1))}
            />
            <Button type="button" size="sm" variant="outline" disabled={syncing} onClick={() => void onSync()}>
              {syncing ? "Syncing..." : "Sync page"}
            </Button>
          </div>
        </CardHeader>
      </Card>

      {error && <p className="text-sm text-destructive">{error}</p>}
      {successMessage && <p className="text-sm text-green-600 dark:text-green-500">{successMessage}</p>}

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
                        disabled={busy || deletingId === m.id}
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
