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
  createCinema,
  deleteCinema,
  getAllCinemas,
  updateCinema,
  type GetAllCinemasResponse,
  type UpdateCinemaRequest,
} from "@/lib/api/cinemas"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

const emptyCreate = {
  name: "",
  description: "",
  address: "",
  city: "",
  country: "",
  phone: "",
  email: "",
}

export default function AdminCinemasPage() {
  const [rows, setRows] = useState<GetAllCinemasResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [editRow, setEditRow] = useState<GetAllCinemasResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateCinemaRequest>({})

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const data = await getAllCinemas(1, 50, { quiet: true, auth: true, cacheBust: true })
      setRows([...(data.items ?? [])])
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load cinemas.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await createCinema({
        name: createForm.name.trim(),
        description: createForm.description.trim() || undefined,
        address: createForm.address.trim(),
        city: createForm.city.trim(),
        country: createForm.country.trim(),
        phone: createForm.phone.trim() || undefined,
        email: createForm.email.trim() || undefined,
      })
      setCreateOpen(false)
      setCreateForm(emptyCreate)
      setSuccessMessage("Cinema created.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  function openEdit(c: GetAllCinemasResponse) {
    setEditRow(c)
    setEditForm({
      name: c.name,
      address: c.address,
      phone: c.phone ?? "",
    })
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await updateCinema(editRow.id, {
        name: editForm.name?.trim() || undefined,
        description: editForm.description?.trim() || undefined,
        address: editForm.address?.trim() || undefined,
        phone: editForm.phone?.trim() || undefined,
        email: editForm.email?.trim() || undefined,
      })
      setEditRow(null)
      setSuccessMessage("Cinema updated.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this cinema?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteCinema(id)
      setSuccessMessage("Cinema deleted.")
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
          <h1 className="font-serif text-2xl font-bold tracking-tight">Cinemas</h1>
          <p className="text-sm text-muted-foreground">/api/cinema — ManageCinemas.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New cinema</Button>
          </DialogTrigger>
          <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-lg">
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create cinema</DialogTitle>
              </DialogHeader>
              <div className="grid gap-3 py-4">
                <div className="space-y-1">
                  <Label>Name</Label>
                  <Input
                    required
                    value={createForm.name}
                    onChange={(e) => setCreateForm((p) => ({ ...p, name: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label>Description</Label>
                  <Input
                    value={createForm.description}
                    onChange={(e) => setCreateForm((p) => ({ ...p, description: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label>Address</Label>
                  <Input
                    required
                    value={createForm.address}
                    onChange={(e) => setCreateForm((p) => ({ ...p, address: e.target.value }))}
                  />
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label>City</Label>
                    <Input
                      required
                      value={createForm.city}
                      onChange={(e) => setCreateForm((p) => ({ ...p, city: e.target.value }))}
                    />
                  </div>
                  <div className="space-y-1">
                    <Label>Country</Label>
                    <Input
                      required
                      value={createForm.country}
                      onChange={(e) => setCreateForm((p) => ({ ...p, country: e.target.value }))}
                    />
                  </div>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label>Phone</Label>
                    <Input
                      value={createForm.phone}
                      onChange={(e) => setCreateForm((p) => ({ ...p, phone: e.target.value }))}
                    />
                  </div>
                  <div className="space-y-1">
                    <Label>Email</Label>
                    <Input
                      type="email"
                      value={createForm.email}
                      onChange={(e) => setCreateForm((p) => ({ ...p, email: e.target.value }))}
                    />
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
                  <TableHead>Location</TableHead>
                  <TableHead>Address</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((c) => (
                  <TableRow key={c.id}>
                    <TableCell className="font-mono text-xs">{c.id}</TableCell>
                    <TableCell className="font-medium">{c.name}</TableCell>
                    <TableCell>{`${c.city}, ${c.country}`}</TableCell>
                    <TableCell>{c.address}</TableCell>
                    <TableCell className="text-right">
                      <Button type="button" size="sm" variant="secondary" onClick={() => openEdit(c)}>
                        Edit
                      </Button>{" "}
                      <Button type="button" size="sm" variant="destructive" disabled={busy} onClick={() => void onDelete(c.id)}>
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
              <DialogTitle>Edit cinema</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label>Name</Label>
                <Input value={editForm.name ?? ""} onChange={(e) => setEditForm((p) => ({ ...p, name: e.target.value }))} />
              </div>
              <div className="space-y-1">
                <Label>Description</Label>
                <Input
                  value={editForm.description ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, description: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label>Address</Label>
                <Input
                  value={editForm.address ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, address: e.target.value }))}
                />
              </div>
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1">
                  <Label>Phone</Label>
                  <Input
                    value={editForm.phone ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, phone: e.target.value }))}
                  />
                </div>
                <div className="space-y-1">
                  <Label>Email</Label>
                  <Input
                    type="email"
                    value={editForm.email ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, email: e.target.value }))}
                  />
                </div>
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

