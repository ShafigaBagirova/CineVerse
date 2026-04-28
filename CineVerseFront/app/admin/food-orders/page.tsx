"use client"

import { useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Spinner } from "@/components/ui/spinner"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import {
  cancelFoodOrder,
  getAllFoodOrders,
  type FoodOrderResponse,
} from "@/lib/api/foods"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

export default function AdminFoodOrdersPage() {
  const [rows, setRows] = useState<FoodOrderResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const data = await getAllFoodOrders({ pageNumber: 1, pageSize: 10 })
      setRows(data)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load food orders.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function onCancel(id: number) {
    if (!window.confirm("Cancel this food order?")) return
    setBusy(true)
    setError(null)
    setSuccessMessage(null)
    try {
      await cancelFoodOrder(id)
      setSuccessMessage("Food order cancelled.")
      await load({ silent: true })
    } catch (err) {
      setError(userFacingApiErrorMessage(err, "Cancel failed."))
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="font-serif text-2xl font-bold tracking-tight">Food Orders</h1>
          <p className="text-sm text-muted-foreground">/api/foodorder — list and cancel management.</p>
        </div>
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
                  <TableHead>User</TableHead>
                  <TableHead>Total</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Items</TableHead>
                  <TableHead className="text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((r) => (
                  <TableRow key={r.id}>
                    <TableCell className="font-mono text-xs">{r.id}</TableCell>
                    <TableCell className="truncate">{r.userId}</TableCell>
                    <TableCell>{r.totalAmount}</TableCell>
                    <TableCell>{r.status}</TableCell>
                    <TableCell>{r.items.length}</TableCell>
                    <TableCell className="text-right">
                      <Button type="button" size="sm" variant="destructive" disabled={busy} onClick={() => void onCancel(r.id)}>
                        Cancel
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

    </div>
  )
}

