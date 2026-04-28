"use client"

import { useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Spinner } from "@/components/ui/spinner"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { deleteReviewByAdmin, getAllReviews, type ReviewDto } from "@/lib/api/reviews"
import { ApiError, userFacingApiErrorMessage } from "@/lib/api/types"

function formatDate(iso: string | undefined | null) {
  if (!iso?.trim()) return "—"
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return "—"
  return d.toLocaleString()
}

export default function AdminReviewsPage() {
  const [rows, setRows] = useState<ReviewDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [successMessage, setSuccessMessage] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  const load = useCallback(async (opts?: { silent?: boolean }) => {
    if (!opts?.silent) setLoading(true)
    setError(null)
    try {
      const data = await getAllReviews({ quiet: true })
      setRows(data)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load reviews.")
    } finally {
      if (!opts?.silent) setLoading(false)
    }
  }, [])

  useEffect(() => {
    void load()
  }, [load])

  async function onDelete(row: ReviewDto) {
    if (!window.confirm("Delete this review permanently?")) return
    setBusyId(row.id)
    setError(null)
    setSuccessMessage(null)
    try {
      await deleteReviewByAdmin(row.id)
      setRows((prev) => prev.filter((r) => r.id !== row.id))
      setSuccessMessage("Review deleted.")
    } catch (e) {
      setError(userFacingApiErrorMessage(e, "Delete failed."))
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-serif text-2xl font-bold tracking-tight">Reviews</h1>
        <p className="text-sm text-muted-foreground">
          {`GET /api/review — list all reviews. DELETE /api/review/reviews/{id} — remove any review (ManageMovies). Star ratings are stored separately from text reviews; the rating column shows "—".`}
        </p>
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
                  <TableHead className="w-[72px]">ID</TableHead>
                  <TableHead>User</TableHead>
                  <TableHead>Movie</TableHead>
                  <TableHead className="w-[88px]">Rating</TableHead>
                  <TableHead>Comment</TableHead>
                  <TableHead className="w-[160px]">Created</TableHead>
                  <TableHead className="w-[100px] text-right">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center text-muted-foreground">
                      No reviews found.
                    </TableCell>
                  </TableRow>
                ) : (
                  rows.map((r) => (
                    <TableRow key={r.id}>
                      <TableCell className="font-mono text-xs">{r.id}</TableCell>
                      <TableCell className="max-w-[140px] truncate" title={r.userName ?? r.userId}>
                        {r.userName?.trim() || r.userId || "—"}
                      </TableCell>
                      <TableCell className="max-w-[180px] truncate" title={r.movieTitle ?? undefined}>
                        {r.movieTitle?.trim() || `Movie #${r.movieId}`}
                      </TableCell>
                      <TableCell className="text-muted-foreground">—</TableCell>
                      <TableCell className="max-w-[320px]">
                        <span className="line-clamp-2 whitespace-pre-wrap break-words" title={r.content}>
                          {r.isDeleted ? (
                            <span className="text-muted-foreground">(removed) </span>
                          ) : null}
                          {r.content}
                        </span>
                      </TableCell>
                      <TableCell className="whitespace-nowrap text-xs text-muted-foreground">
                        {formatDate(r.createdAt)}
                      </TableCell>
                      <TableCell className="text-right">
                        <Button
                          type="button"
                          variant="destructive"
                          size="sm"
                          disabled={busyId !== null}
                          onClick={() => void onDelete(r)}
                        >
                          {busyId === r.id ? "…" : "Delete"}
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
