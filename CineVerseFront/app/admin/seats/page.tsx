"use client"

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react"
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
import { getAllHalls } from "@/lib/api/halls"
import {
  createSeat,
  deleteSeat,
  getAllSeats,
  updateSeat,
  type GetAllSeatsResponse,
  type SeatType,
  type UpdateSeatRequest,
} from "@/lib/api/seats"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

const seatTypes: SeatType[] = ["Standard", "VIP", "Couple"]

function normalizeSeatNumberInput(value: string): number | undefined {
  let next = value
  if (next.startsWith("0")) {
    next = next.replace(/^0+/, "")
  }
  const cleaned = parseInt(next, 10)
  return Number.isNaN(cleaned) ? undefined : cleaned
}

export default function AdminSeatsPage() {
  const [rows, setRows] = useState<GetAllSeatsResponse[]>([])
  const [halls, setHalls] = useState<{ id: number; name: string }[]>([])
  const [loading, setLoading] = useState(true)
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState({
    hallId: 0,
    row: "",
    number: 1,
    type: "Standard" as SeatType,
  })
  const [editRow, setEditRow] = useState<GetAllSeatsResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateSeatRequest>({})

  const loadSeats = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const [seatsData, hallsData] = await Promise.all([
        getAllSeats({ pageNumber: 1, pageSize: 100 }),
        getAllHalls(1, 100, { quiet: true, sortBy: "createdAt", desc: true }),
      ])
      setRows(seatsData.items ?? [])
      setHalls((hallsData.items ?? []).filter((h) => h.isActive).map((h) => ({ id: h.id, name: h.name })))
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load seats.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadSeats()
  }, [loadSeats])

  useEffect(() => {
    if (halls.length > 0) {
      setCreateForm((prev) => (prev.hallId > 0 ? prev : { ...prev, hallId: halls[0].id }))
    }
  }, [halls])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    const hallId = Math.trunc(Number(createForm.hallId))
    const number = Math.trunc(Number(createForm.number))
    const row = createForm.row.trim().toUpperCase()
    if (hallId <= 0) return setError("Select a hall.")
    if (!row) return setError("Row is required.")
    if (number <= 0) return setError("Seat number must be greater than 0.")

    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await createSeat({
        hallId,
        row,
        number,
        type: createForm.type,
      })
      await loadSeats({ silent: true })
      setCreateOpen(false)
      setCreateForm({ hallId: halls[0]?.id ?? 0, row: "", number: 1, type: "Standard" })
      setSuccessMessage("Seat created.")
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
      await updateSeat(editRow.id, editForm)
      await loadSeats({ silent: true })
      setEditRow(null)
      setSuccessMessage("Seat updated.")
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this seat?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteSeat(id)
      await loadSeats({ silent: true })
      setSuccessMessage("Seat deleted.")
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Delete failed."))
    } finally {
      setBusy(false)
    }
  }

  const hallLabel = (hallId: number) => halls.find((h) => h.id === hallId)?.name ?? `#${hallId}`
  const sortedSeats = useMemo(() => [...rows].sort((a, b) => a.id - b.id), [rows])

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Seats</h1>
          <p className="text-sm text-muted-foreground">/api/seat - list/create/update/delete seat configuration.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New seat</Button>
          </DialogTrigger>
          <DialogContent>
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create seat</DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                <div className="space-y-1">
                  <Label htmlFor="seat-hall">Hall</Label>
                  <select
                    id="seat-hall"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={createForm.hallId}
                    onChange={(e) => setCreateForm((p) => ({ ...p, hallId: Number(e.target.value) }))}
                  >
                    {halls.map((h) => (
                      <option key={h.id} value={h.id}>
                        {h.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="space-y-1">
                  <Label htmlFor="seat-row">Row</Label>
                  <Input
                    id="seat-row"
                    value={createForm.row}
                    onChange={(e) => setCreateForm((p) => ({ ...p, row: e.target.value }))}
                    placeholder="A"
                    required
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="seat-number">Number</Label>
                  <Input
                    id="seat-number"
                    type="number"
                    min={1}
                    value={createForm.number}
                    onChange={(e) =>
                      setCreateForm((p) => ({
                        ...p,
                        number: normalizeSeatNumberInput(e.target.value) ?? 1,
                      }))
                    }
                    required
                  />
                </div>
                <div className="space-y-1">
                  <Label htmlFor="seat-type">Type</Label>
                  <select
                    id="seat-type"
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                    value={createForm.type}
                    onChange={(e) => setCreateForm((p) => ({ ...p, type: e.target.value as SeatType }))}
                  >
                    {seatTypes.map((t) => (
                      <option key={t} value={t}>
                        {t}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <DialogFooter>
                <Button type="submit" disabled={busy || halls.length === 0}>
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
                  <TableHead>Hall</TableHead>
                  <TableHead>Row</TableHead>
                  <TableHead>Number</TableHead>
                  <TableHead>Type</TableHead>
                  <TableHead>Active</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedSeats.map((s) => (
                  <TableRow key={s.id}>
                    <TableCell className="font-mono text-xs">{s.id}</TableCell>
                    <TableCell>{hallLabel(s.hallId)}</TableCell>
                    <TableCell className="font-medium">{s.row}</TableCell>
                    <TableCell>{s.number}</TableCell>
                    <TableCell>{s.type}</TableCell>
                    <TableCell>{s.isActive ? "yes" : "no"}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        type="button"
                        size="sm"
                        variant="secondary"
                        onClick={() => {
                          setEditRow(s)
                          setEditForm({
                            hallId: s.hallId,
                            row: s.row,
                            number: s.number,
                            type: s.type,
                            isActive: s.isActive,
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
                        onClick={() => void onDelete(s.id)}
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
              <DialogTitle>Edit seat</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label htmlFor="seat-edit-hall">Hall</Label>
                <select
                  id="seat-edit-hall"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  value={editForm.hallId ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, hallId: Number(e.target.value) }))}
                >
                  {halls.map((h) => (
                    <option key={h.id} value={h.id}>
                      {h.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="space-y-1">
                <Label htmlFor="seat-edit-row">Row</Label>
                <Input
                  id="seat-edit-row"
                  value={editForm.row ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, row: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="seat-edit-number">Number</Label>
                <Input
                  id="seat-edit-number"
                  type="number"
                  min={1}
                  value={editForm.number ?? ""}
                  onChange={(e) =>
                    setEditForm((p) => ({
                      ...p,
                      number: normalizeSeatNumberInput(e.target.value),
                    }))
                  }
                />
              </div>
              <div className="space-y-1">
                <Label htmlFor="seat-edit-type">Type</Label>
                <select
                  id="seat-edit-type"
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
                  value={editForm.type ?? "Standard"}
                  onChange={(e) => setEditForm((p) => ({ ...p, type: e.target.value as SeatType }))}
                >
                  {seatTypes.map((t) => (
                    <option key={t} value={t}>
                      {t}
                    </option>
                  ))}
                </select>
              </div>
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={Boolean(editForm.isActive)}
                  onChange={(e) => setEditForm((p) => ({ ...p, isActive: e.target.checked }))}
                />
                Active
              </label>
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

