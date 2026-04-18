"use client"

import { useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Spinner } from "@/components/ui/spinner"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import {
  getAllPayments,
  retryPayment,
  type GetAllPaymentsResponse,
  type PaymentStatus,
} from "@/lib/api/payments"
import { ApiError } from "@/lib/api/types"

const statusOptions: (PaymentStatus | "")[] = ["", "Pending", "Succeeded", "Failed", "Cancelled", "Refunded"]

function formatMoneyAmount(amount: number, currency: string | undefined) {
  const code = currency?.trim()
  if (code) {
    try {
      return new Intl.NumberFormat(undefined, { style: "currency", currency: code }).format(amount)
    } catch {
      return `${amount.toFixed(2)} ${code}`.trim()
    }
  }
  return amount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function formatPaymentInstant(iso: string | undefined | null, paidFallback?: string | null) {
  const primary = iso?.trim()
  if (primary) {
    const d = new Date(primary)
    if (!Number.isNaN(d.getTime()) && d.getFullYear() >= 1970) {
      return d.toLocaleString()
    }
  }
  if (paidFallback?.trim()) {
    const d2 = new Date(paidFallback)
    if (!Number.isNaN(d2.getTime()) && d2.getFullYear() >= 1970) {
      return d2.toLocaleString()
    }
  }
  return "—"
}

export default function AdminBookingsPage() {
  const [status, setStatus] = useState<PaymentStatus | "">("")
  const [rows, setRows] = useState<GetAllPaymentsResponse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getAllPayments(
        {
          pageNumber: 1,
          pageSize: 10,
          status: status || undefined,
        },
        { quiet: true },
      )
      setRows(data.items)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load payments.")
    } finally {
      setLoading(false)
    }
  }, [status])

  useEffect(() => {
    void load()
  }, [load])

  async function onRetry(row: GetAllPaymentsResponse) {
    if (row.status !== "Failed" || row.seatHoldId == null) return
    setBusyId(row.seatHoldId)
    setError(null)
    try {
      await retryPayment(row.seatHoldId)
      await load()
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Retry failed.")
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-serif text-2xl font-bold tracking-tight">Bookings (payments)</h1>
        <p className="text-sm text-muted-foreground">
          GET /api/payment — ticket payments (ManageCinemas). Status is driven by Stripe/webhooks; use retry for failed
          checkouts when appropriate.
        </p>
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <label className="text-sm text-muted-foreground">Filter</label>
        <select
          className="h-10 rounded-md border border-input bg-background px-3 text-sm"
          value={status}
          onChange={(e) => setStatus(e.target.value as PaymentStatus | "")}
        >
          {statusOptions.map((s) => (
            <option key={s || "all"} value={s}>
              {s || "All statuses"}
            </option>
          ))}
        </select>
        <Button type="button" size="sm" variant="secondary" onClick={() => void load()}>
          Refresh
        </Button>
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
                    <TableHead className="w-14">ID</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Amount</TableHead>
                    <TableHead>User</TableHead>
                    <TableHead>Seat hold</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {rows.map((p, idx) => (
                    <TableRow key={p.id != null ? String(p.id) : `pay-${idx}`}>
                      <TableCell className="font-mono text-xs">{p.id ?? "—"}</TableCell>
                      <TableCell>{p.status ?? "—"}</TableCell>
                      <TableCell className="whitespace-nowrap tabular-nums">
                        {p.amount != null && Number.isFinite(p.amount)
                          ? formatMoneyAmount(p.amount, p.currency)
                          : "—"}
                      </TableCell>
                      <TableCell className="max-w-[120px] truncate font-mono text-xs">{p.userId ?? "—"}</TableCell>
                      <TableCell className="font-mono text-xs">{p.seatHoldId ?? "—"}</TableCell>
                      <TableCell className="whitespace-nowrap text-xs">
                        {formatPaymentInstant(p.createdAtUtc, p.paidAtUtc)}
                      </TableCell>
                      <TableCell className="text-right">
                        {p.status === "Failed" && p.seatHoldId != null && (
                          <Button
                            type="button"
                            size="sm"
                            variant="outline"
                            disabled={busyId === p.seatHoldId}
                            onClick={() => void onRetry(p)}
                          >
                            {busyId === p.seatHoldId ? "…" : "Retry"}
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
