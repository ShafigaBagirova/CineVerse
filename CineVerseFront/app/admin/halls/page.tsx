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
import { getAllCinemas } from "@/lib/api/cinemas"
import {
  createHall,
  deleteHall,
  getAllHalls,
  updateHall,
  type GetAllHallsResponse,
  type UpdateHallBody,
} from "@/lib/api/halls"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

export default function AdminHallsPage() {
  const [rows, setRows] = useState<GetAllHallsResponse[]>([])
  const [cinemas, setCinemas] = useState<{ id: number; name: string }[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [form, setForm] = useState({ name: "", cinemaId: 0, capacity: 100 })
  const [editRow, setEditRow] = useState<GetAllHallsResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateHallBody>({})

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) {
      setLoading(true)
    }
    setError(null)
    try {
      const [h, c] = await Promise.all([
        getAllHalls(1, 100, {
          sortBy: "createdAt",
          desc: true,
          quiet: true,
        }),
        getAllCinemas(1, 50, { quiet: true }),
      ])
      const items = h.items ?? []
      // Backend "delete" is soft (isActive=false); hide removed halls from this list.
      setRows(items.filter((x) => x.isActive))
      setCinemas(c.items.map((x) => ({ id: x.id, name: x.name })))
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load halls.")
    } finally {
      if (!opts?.silent) {
        setLoading(false)
      }
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  useEffect(() => {
    if (cinemas.length > 0) {
      setForm((f) => (f.cinemaId === 0 ? { ...f, cinemaId: cinemas[0].id } : f))
    }
  }, [cinemas])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    const cinemaId = Math.trunc(Number(form.cinemaId))
    if (!form.name.trim()) {
      setError("Name is required.")
      setBusy(false)
      return
    }
    if (cinemaId <= 0) {
      setError("Select a cinema.")
      setBusy(false)
      return
    }
    try {
      await createHall({
        name: form.name.trim(),
        cinemaId,
        capacity: Math.max(1, Math.trunc(Number(form.capacity)) || 1),
      })
      setSuccessMessage("Hall created.")
      setCreateOpen(false)
      setForm({ name: "", cinemaId: cinemas[0]?.id ?? 0, capacity: 100 })
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await updateHall(editRow.id, editForm)
      setEditRow(null)
      setSuccessMessage("Hall updated.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this hall?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteHall(id)
      setRows((prev) => prev.filter((x) => x.id !== id))
      setSuccessMessage("Hall removed.")
      await load({ silent: true })
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) {
        setError("This hall was already removed. Refreshing the list.")
        setRows((prev) => prev.filter((x) => x.id !== id))
        await load({ silent: true })
      } else {
        setError(userFacingApiErrorMessage(err, "Delete failed."))
      }
    } finally {
      setBusy(false)
    }
  }

  const cinemaName = (id: number) => cinemas.find((c) => c.id === id)?.name ?? `#${id}`

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Halls</h1>
          <p className="text-sm text-muted-foreground">/api/hall — ManageCinemas.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New hall</Button>
          </DialogTrigger>
          <DialogContent>
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create hall</DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                <div className="space-y-1">
                  <Label htmlFor="h-name">Name</Label>
                  <Input
                    id="h-name"
                    required
                    value={form.name}
                    onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="h-cinema">Cinema</Label>
                  <select
                    id="h-cinema"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={form.cinemaId}
                    onChange={(e) => setForm((p) => ({ ...p, cinemaId: Number(e.target.value) }))}
                  >
                    {cinemas.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1">
                  <Label htmlFor="h-cap">Capacity</Label>
                  <Input
                    id="h-cap"
                    type="number"
                    min={1}
                    required
                    value={form.capacity}
                    onChange={(e) => setForm((p) => ({ ...p, capacity: Number(e.target.value) }))}
                  />
                </div>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={busy || cinemas.length === 0}>
                  Create
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

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
                  <TableHead>Name</TableHead>
                  <TableHead>Cinema</TableHead>
                  <TableHead>Capacity</TableHead>
                  <TableHead>Active</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((h) => (
                  <TableRow key={h.id}>
                    <TableCell className="font-mono text-xs">{h.id}</TableCell>
                    <TableCell className="font-medium">{h.name}</TableCell>
                    <TableCell>{cinemaName(h.cinemaId)}</TableCell>
                    <TableCell>{h.capacity}</TableCell>
                    <TableCell>{h.isActive ? "yes" : "no"}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        type="button"
                        size="sm"
                        variant="secondary"
                        onClick={() => {
                          setEditRow(h)
                          setEditForm({
                            name: h.name,
                            cinemaId: h.cinemaId,
                            capacity: h.capacity,
                          })
                        }}
                      >
                        Edit
                      </Button>{" "}
                      <Button
                        type="button"
                        size="sm"
                        variant="destructive"
                        disabled={busy}
                        onClick={() => void onDelete(h.id)}
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
        <DialogContent>
          <form onSubmit={onSaveEdit}>
            <DialogHeader>
              <DialogTitle>Edit hall</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label htmlFor="he-name">Name</Label>
                <Input
                  id="he-name"
                  value={editForm.name ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, name: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="he-cinema">Cinema</Label>
                <select
                  id="he-cinema"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  value={editForm.cinemaId ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({ ...p, cinemaId: Number(e.target.value) }))
                  }
                >
                  {cinemas.map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-1">
                <Label htmlFor="he-cap">Capacity</Label>
                <Input
                  id="he-cap"
                  type="number"
                  min={1}
                  value={editForm.capacity ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({ ...p, capacity: Number(e.target.value) }))
                  }
                />
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
