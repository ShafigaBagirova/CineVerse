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
  createFoodItem,
  deleteFoodItem,
  getAllFoodCategories,
  getAllFoodItems,
  updateFoodItem,
  type FoodCategoryResponse,
  type FoodItemResponse,
  type UpdateFoodItemRequest,
} from "@/lib/api/foods"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

const emptyCreate = {
  name: "",
  description: "",
  price: "",
  isAvailable: true,
  foodCategoryId: 0,
  image: null as File | null,
}

function normalizePriceInput(raw: string): string {
  const cleaned = raw.replace(/,/g, ".").replace(/[^\d.]/g, "")
  const firstDot = cleaned.indexOf(".")
  if (firstDot === -1) {
    // Keep only digits and trim leading zero noise (except single zero)
    const digits = cleaned.replace(/^0+(?=\d)/, "")
    return digits
  }
  const intPartRaw = cleaned.slice(0, firstDot).replace(/\./g, "")
  const fracPartRaw = cleaned.slice(firstDot + 1).replace(/\./g, "")
  const intPart = intPartRaw.replace(/^0+(?=\d)/, "")
  const fracPart = fracPartRaw.slice(0, 2)
  return `${intPart || "0"}.${fracPart}`
}

function parsePriceOrNull(value: string): number | null {
  if (!value.trim()) return null
  if (!/^\d+(\.\d{1,2})?$/.test(value)) return null
  const n = Number(value)
  if (!Number.isFinite(n) || n < 0) return null
  return n
}

export default function AdminFoodItemsPage() {
  const [rows, setRows] = useState<FoodItemResponse[]>([])
  const [categories, setCategories] = useState<FoodCategoryResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [createOpen, setCreateOpen] = useState(false)
  const [createForm, setCreateForm] = useState(emptyCreate)
  const [createPriceError, setCreatePriceError] = useState<string | null>(null)
  const [editRow, setEditRow] = useState<FoodItemResponse | null>(null)
  const [editForm, setEditForm] = useState<UpdateFoodItemRequest>({})
  const [editPriceText, setEditPriceText] = useState("")
  const [editPriceError, setEditPriceError] = useState<string | null>(null)

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const [items, cats] = await Promise.all([
        getAllFoodItems({ pageNumber: 1, pageSize: 10 }),
        getAllFoodCategories({ pageNumber: 1, pageSize: 10 }),
      ])
      setRows(items)
      setCategories(cats)
      if ((createForm.foodCategoryId ?? 0) === 0 && cats.length > 0) {
        setCreateForm((p) => ({ ...p, foodCategoryId: cats[0].id }))
      }
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load food items.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [createForm.foodCategoryId])

  useEffect(() => {
    void load()
  }, [load])

  async function onCreate(e: FormEvent) {
    e.preventDefault()
    const parsedPrice = parsePriceOrNull(createForm.price)
    if (parsedPrice === null) {
      setCreatePriceError("Enter a valid price (e.g. 6, 6.50, 0.66).")
      return
    }
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    setCreatePriceError(null)
    try {
      await createFoodItem({
        name: createForm.name.trim(),
        description: createForm.description.trim() || undefined,
        price: parsedPrice,
        isAvailable: createForm.isAvailable,
        foodCategoryId: createForm.foodCategoryId,
        image: createForm.image,
      })
      setCreateOpen(false)
      setCreateForm(emptyCreate)
      setCreatePriceError(null)
      setSuccessMessage("Food item created.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Create failed."))
    } finally {
      setBusy(false)
    }
  }

  function openEdit(row: FoodItemResponse) {
    setEditRow(row)
    setEditPriceText(String(row.price))
    setEditPriceError(null)
    setEditForm({
      name: row.name,
      description: row.description ?? "",
      price: undefined,
      isAvailable: row.isAvailable,
      foodCategoryId: row.foodCategoryId,
    })
  }

  async function onSaveEdit(e: FormEvent) {
    e.preventDefault()
    if (!editRow) return
    const parsedPrice = parsePriceOrNull(editPriceText)
    if (parsedPrice === null) {
      setEditPriceError("Enter a valid price (e.g. 6, 6.50, 0.66).")
      return
    }
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    setEditPriceError(null)
    try {
      await updateFoodItem(editRow.id, { ...editForm, price: parsedPrice })
      setEditRow(null)
      setSuccessMessage("Food item updated.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Update failed."))
    } finally {
      setBusy(false)
    }
  }

  async function onDelete(id: number) {
    if (!window.confirm("Delete this food item?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteFoodItem(id)
      setSuccessMessage("Food item deleted.")
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
          <h1 className="font-serif text-2xl font-bold tracking-tight">Food Items</h1>
          <p className="text-sm text-muted-foreground">/api/fooditem — Admin CRUD.</p>
        </div>
        <Dialog open={createOpen} onOpenChange={setCreateOpen}>
          <DialogTrigger asChild>
            <Button type="button">New food item</Button>
          </DialogTrigger>
          <DialogContent className="max-h-[90vh] overflow-y-auto">
            <form onSubmit={onCreate}>
              <DialogHeader>
                <DialogTitle>Create food item</DialogTitle>
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
                <div className="grid grid-cols-2 gap-2">
                  <div className="space-y-1">
                    <Label>Price</Label>
                    <Input
                      type="text"
                      inputMode="decimal"
                      placeholder="0.00"
                      required
                      value={createForm.price}
                      onChange={(e) =>
                        setCreateForm((p) => ({ ...p, price: normalizePriceInput(e.target.value) }))
                      }
                    />
                    {createPriceError && <p className="text-xs text-destructive">{createPriceError}</p>}
                  </div>
                  <div className="space-y-1">
                    <Label>Category</Label>
                    <select
                      className="border-input flex h-10 w-full rounded-md border bg-background px-3 text-sm"
                      value={createForm.foodCategoryId}
                      onChange={(e) => setCreateForm((p) => ({ ...p, foodCategoryId: Number(e.target.value) }))}
                    >
                      {categories.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.name}
                        </option>
                      ))}
                    </select>
                  </div>
                </div>
                <div className="space-y-1">
                  <Label>Image</Label>
                  <Input
                    type="file"
                    accept="image/*"
                    onChange={(e) => setCreateForm((p) => ({ ...p, image: e.target.files?.[0] ?? null }))}
                  />
                </div>
                <label className="flex items-center gap-2 text-sm">
                  <input
                    type="checkbox"
                    checked={createForm.isAvailable}
                    onChange={(e) => setCreateForm((p) => ({ ...p, isAvailable: e.target.checked }))}
                  />
                  Available
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
                  <TableHead>Category</TableHead>
                  <TableHead>Price</TableHead>
                  <TableHead>Available</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="font-mono text-xs">{r.id}</TableCell>
                    <TableCell className="font-medium">{r.name}</TableCell>
                    <TableCell>{r.foodCategoryName}</TableCell>
                    <TableCell>{r.price}</TableCell>
                    <TableCell>{r.isAvailable ? "Yes" : "No"}</TableCell>
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
              <DialogTitle>Edit food item</DialogTitle>
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
              <div className="grid grid-cols-2 gap-2">
                <div className="space-y-1">
                  <Label>Price</Label>
                  <Input
                    type="text"
                    inputMode="decimal"
                    placeholder="0.00"
                    value={editPriceText}
                    onChange={(e) => setEditPriceText(normalizePriceInput(e.target.value))}
                  />
                  {editPriceError && <p className="text-xs text-destructive">{editPriceError}</p>}
                </div>
                <div className="space-y-1">
                  <Label>Category</Label>
                  <select
                    className="border-input flex h-10 w-full rounded-md border bg-background px-3 text-sm"
                    value={editForm.foodCategoryId ?? ""}
                    onChange={(e) => setEditForm((p) => ({ ...p, foodCategoryId: Number(e.target.value) }))}
                  >
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={Boolean(editForm.isAvailable)}
                  onChange={(e) => setEditForm((p) => ({ ...p, isAvailable: e.target.checked }))}
                />
                Available
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

