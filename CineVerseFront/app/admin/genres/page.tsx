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
import {
  createGenre,
  deleteGenre,
  getAllGenres,
  updateGenre,
  type GetAllGenresResponse,
} from "@/lib/api/genres"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

export default function AdminGenresPage() {
  const [rows, setRows] = useState<GetAllGenresResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createName, setCreateName] = useState("")
  const [editRow, setEditRow] = useState<GetAllGenresResponse | null>(null)
  const [editName, setEditName] = useState("")

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const data = await getAllGenres()
      setRows(data)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load genres.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    const name = createName.trim()
    if (!name) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await createGenre({ name })
      setCreateOpen(false)
      setCreateName("")
      setSuccessMessage("Genre created.")
      await load()
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  function openEdit(g: GetAllGenresResponse) {
    setEditRow(g)
    setEditName(g.name)
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    const name = editName.trim()
    if (!name) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await updateGenre(editRow.id, { name })
      setEditRow(null)
      setSuccessMessage("Genre updated.")
      await load({ silent: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Update failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this genre?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteGenre(id)
      setRows((prev) => prev.filter((g) => g.id !== id))
      setSuccessMessage("Genre deleted.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Delete failed."))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Genres</h1>
          <p className="text-sm text-muted-foreground">
            {`GET /api/genre — list. POST /api/genre body { name }. PUT /api/genre/{id}, DELETE /api/genre/{id} (ManageMovies).`}
          </p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New genre</Button>
          </DialogTrigger>
          <DialogContent className="sm:max-w-md">
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create genre</DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                <div className="space-y-1">
                  <Label htmlFor="g-name">Name</Label>
                  <Input
                    id="g-name"
                    required
                    value={createName}
                    onChange={(e) => setCreateName(e.target.value)}
                    placeholder="e.g. Sci‑Fi"
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

      {successMessage ? (
        <Card className="border-primary/30 bg-primary/5">
          <CardContent className="py-3 text-sm text-foreground">{successMessage}</CardContent>
        </Card>
      ) : null}

      {error ? (
        <Card className="border-destructive/40 bg-destructive/5">
          <CardContent className="py-3 text-sm text-destructive">{error}</CardContent>
        </Card>
      ) : null}

      <Card>
        <CardContent className="p-0">
          {loading ? (
            <div className="flex justify-center py-16">
              <Spinner className="h-10 w-10 text-primary" />
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[88px]">ID</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead className="w-[200px] text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={3} className="text-center text-muted-foreground">
                      No genres yet.
                    </TableCell>
                  </TableRow>
                ) : (
                  rows.map((g) => (
                    <TableRow key={g.id}>
                      <TableCell className="font-mono text-xs">{g.id}</TableCell>
                      <TableCell>{g.name}</TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-2">
                          <Button type="button" variant="outline" size="sm" onClick={() => openEdit(g)}>
                            Edit
                          </Button>
                          <Button
                            type="button"
                            variant="destructive"
                            size="sm"
                            disabled={busy}
                            onClick={() => void onDelete(g.id)}
                          >
                            Delete
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={editRow !== null} onOpenChange={(open) => !open && setEditRow(null)}>
        <DialogContent className="sm:max-w-md">
          <form onSubmit={onSaveEdit}>
            <DialogHeader>
              <DialogTitle>Edit genre</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label htmlFor="g-edit-name">Name</Label>
                <Input
                  id="g-edit-name"
                  required
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                />
              </div>
            </div>
            <DialogFooter>
              <Button type="submit" disabled={busy}>
                {busy ? "Saving…" : "Save"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
