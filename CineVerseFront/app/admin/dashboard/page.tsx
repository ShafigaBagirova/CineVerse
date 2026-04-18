"use client"

import { useEffect, useState } from "react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Spinner } from "@/components/ui/spinner"
import {
  getAdminDashboardSummary,
  getAdminRecentPayments,
  getAdminRevenueChart,
  getAdminTopMovies,
  type AdminDashboardSummaryDto,
  type RecentPaymentDto,
  type RevenueChartItemDto,
  type TopMovieDto,
} from "@/lib/api/admin-dashboard"
import { ApiError } from "@/lib/api/types"

export default function AdminDashboardPage() {
  const [summary, setSummary] = useState<AdminDashboardSummaryDto | null>(null)
  const [revenue, setRevenue] = useState<RevenueChartItemDto[]>([])
  const [top, setTop] = useState<TopMovieDto[]>([])
  const [recent, setRecent] = useState<RecentPaymentDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      setLoading(true)
      setError(null)
      try {
        const [s, r, t, p] = await Promise.all([
          getAdminDashboardSummary(),
          getAdminRevenueChart(7),
          getAdminTopMovies(5),
          getAdminRecentPayments(8),
        ])
        if (cancelled) return
        setSummary(s)
        setRevenue(Array.isArray(r) ? r : [])
        setTop(Array.isArray(t) ? t : [])
        setRecent(Array.isArray(p) ? p : [])
      } catch (e) {
        if (!cancelled) {
          setError(e instanceof ApiError ? e.message : "Failed to load dashboard.")
        }
      } finally {
        if (!cancelled) setLoading(false)
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  if (loading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner className="h-10 w-10 text-primary" />
      </div>
    )
  }

  if (error || !summary) {
    return <p className="text-sm text-destructive">{error ?? "No data."}</p>
  }

  const stat = (label: string, value: string | number) => (
    <Card key={label} className="border-border/60 bg-card/50">
      <CardHeader className="pb-2">
        <CardTitle className="text-xs font-medium uppercase tracking-wide text-muted-foreground">{label}</CardTitle>
      </CardHeader>
      <CardContent>
        <p className="text-2xl font-semibold tabular-nums">{value}</p>
      </CardContent>
    </Card>
  )

  return (
    <div className="space-y-8">
      <div>
        <h1 className="font-serif text-2xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-sm text-muted-foreground">Overview from AdminDashboard API.</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {stat("Total users", summary.totalUsers)}
        {stat("Movies", summary.totalMovies)}
        {stat("Active screenings", summary.activeScreenings)}
        {stat("Total revenue", summary.totalRevenue.toFixed(2))}
        {stat("Tickets sold", summary.totalTicketsSold)}
        {stat("Successful payments", summary.successfulPayments)}
        {stat("Pending payments", summary.pendingPayments)}
        {stat("Failed payments", summary.failedPayments)}
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="border-border/60 bg-card/50">
          <CardHeader>
            <CardTitle className="text-base">Revenue (last 7 days)</CardTitle>
          </CardHeader>
          <CardContent>
            {revenue.length === 0 ? (
              <p className="text-sm text-muted-foreground">No chart rows.</p>
            ) : (
              <ul className="max-h-56 space-y-2 overflow-y-auto text-sm">
                {revenue.map((row, i) => (
                  <li key={i} className="flex justify-between border-b border-border/40 py-1">
                    <span className="text-muted-foreground">
                      {typeof row.date === "string" ? row.date.slice(0, 10) : String(row.date)}
                    </span>
                    <span className="tabular-nums">{Number(row.revenue).toFixed(2)}</span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>

        <Card className="border-border/60 bg-card/50">
          <CardHeader>
            <CardTitle className="text-base">Top movies</CardTitle>
          </CardHeader>
          <CardContent>
            {top.length === 0 ? (
              <p className="text-sm text-muted-foreground">No data.</p>
            ) : (
              <ul className="space-y-2 text-sm">
                {top.map((m) => (
                  <li key={m.movieId} className="flex justify-between gap-2 border-b border-border/40 py-1">
                    <span className="truncate font-medium">{m.title}</span>
                    <span className="shrink-0 tabular-nums text-muted-foreground">{m.ticketCount} tix</span>
                  </li>
                ))}
              </ul>
            )}
          </CardContent>
        </Card>
      </div>

      <Card className="border-border/60 bg-card/50">
        <CardHeader>
          <CardTitle className="text-base">Recent payments</CardTitle>
        </CardHeader>
        <CardContent>
          {recent.length === 0 ? (
            <p className="text-sm text-muted-foreground">No recent payments.</p>
          ) : (
            <ul className="divide-y divide-border/40 text-sm">
              {recent.map((p) => (
                <li key={p.paymentId} className="flex flex-wrap items-center justify-between gap-2 py-2">
                  <span className="font-mono text-xs text-muted-foreground">#{p.paymentId}</span>
                  <span className="tabular-nums">{Number(p.amount).toFixed(2)}</span>
                  <span>{p.status}</span>
                  <span className="text-xs text-muted-foreground">
                    {p.createdAt ? String(p.createdAt) : "—"}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
