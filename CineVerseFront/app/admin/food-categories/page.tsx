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
  createFoodCategory,
  deleteFoodCategory,
  getAllFoodCategories,
  updateFoodCategory,
  type FoodCategoryResponse,
  type UpdateFoodCategoryRequest,
} from "@/lib/api/foods"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

const emptyCreate = {
  name: "",
  description: "",
  cinemaId: 0,
  isActive: true,
}

export default function AdminFoodCategoriesPage() {
  const [rows, setRows] = useState<FoodCategoryResponse[]>([])
  const [cinemas, setCinemas] = useState<{ id: number; name: string }[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [editRow, setEditRow] = useState<FoodCategoryResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateFoodCategoryRequest>({})

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const [data, c] = await Promise.all([
        getAllFoodCategories({ pageNumber: 1, pageSize: 10 }),
        getAllCinemas(1, 50, { quiet: true }),
      ])
      setRows(data)
      setCinemas(c.items.map((x) => ({ id: x.id, name: x.name })))
      if (createForm.cinemaId === 0 && c.items.length > 0) {
        setCreateForm((p) => ({ ...p, cinemaId: c.items[0].id }))
      }
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load food categories.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [createForm.cinemaId])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await createFoodCategory({
        name: createForm.name.trim(),
        description: createForm.description.trim() || undefined,
        cinemaId: Math.max(1, Math.trunc(Number(createForm.cinemaId)) || 1),
        displayOrder: 0,
        isActive: createForm.isActive,
      })
      setCreateOpen(false)
      setCreateForm(emptyCreate)
      setSuccessMessage("Food category created.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  function openEdit(row: FoodCategoryResponse) {
    setEditRow(row)
    setEditForm({
      name: row.name,
      description: row.description ?? "",
      cinemaId: row.cinemaId,
      isActive: row.isActive,
    })
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await updateFoodCategory(editRow.id, { ...editForm, displayOrder: 0 })
      setEditRow(null)
      setSuccessMessage("Food category updated.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this food category?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteFoodCategory(id)
      setSuccessMessage("Food category deleted.")
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
          <h1 className="font-serif text-2xl font-bold tracking-tight">Food Categories</h1>
          <p className="text-sm text-muted-foreground">/api/foodcategory — Admin CRUD.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New category</Button>
          </DialogTrigger>
          <DialogContent>
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create food category</DialogTitle>
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
                  <Label>Cinema</Label>
                  <select
                    className="border-input flex h-10 w-full rounded-md border bg-background px-3 text-sm"
                    required
                    value={createForm.cinemaId || ""}
                    onChange={(e) => setCreateForm((p) => ({ ...p, cinemaId: Number(e.target.value) }))}
                  >
                    {cinemas.map((cinema) => (
                      <option key={cinema.id} value={cinema.id}>
                        {cinema.name}
                      </option>
                    ))}
                  </select>
                </div>
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={createForm.isActive}
                    onChange={(e) => setCreateForm((p) => ({ ...p, isActive: e.target.checked }))}
                  />
                  Active
                </label>
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
                  <TableHead>Cinema</TableHead>
                  <TableHead>Active</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="font-mono text-xs">{r.id}</TableCell>
                    <TableCell className="font-medium">{r.name}</TableCell>
                    <TableCell>{r.cinemaId}</TableCell>
                    <TableCell>{r.isActive ? "Yes" : "No"}</TableCell>
                    <TableCell className="text-right">
                      <Button type="button" size="sm" variant="secondary" onClick={() => openEdit(r)}>
                        Edit
                      </Button>{" "}
                      <Button type="button" size="sm" variant="destructive" disabled={busy} onClick={() => void onDelete(r.id)}>
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
              <DialogTitle>Edit food category</DialogTitle>
            </DialogHeader>
            <div className="grid gap-3 py-4">
              <div className="space-y-1">
                <Label>Name</Label>
                <Input
                  value={editForm.name ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, name: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label>Description</Label>
                <Input
                  value={editForm.description ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, description: e.target.value }))}
                />
              </div>
              <div className="space-y-1">
                <Label>Cinema</Label>
                <select
                  className="border-input flex h-10 w-full rounded-md border bg-background px-3 text-sm"
                  value={editForm.cinemaId ?? ""}
                  onChange={(e) => setEditForm((p) => ({ ...p, cinemaId: Number(e.target.value) }))}
                >
                  {cinemas.map((cinema) => (
                    <option key={cinema.id} value={cinema.id}>
                      {cinema.name}
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

