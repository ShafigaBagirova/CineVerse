"use client"

import { FormEvent, useCallback, useEffect, useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Spinner } from "@/components/ui/spinner"
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table"
import { getUsersList } from "@/lib/api/user"
import { ApiError } from "@/lib/api/types"

export default function AdminUsersPage() {
  const [search, setSearch] = useState("")
  const [q, setQ] = useState("")
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [rows, setRows] = useState<
    { id: string; userName: string; fullName: string; avatarUrl?: string | null }[]
  >([])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const data = await getUsersList({ pageNumber: 1, pageSize: 50, searchTerm: q.trim() || undefined })
      setRows(data.items)
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "Failed to load users.")
    } finally {
      setLoading(false)
    }
  }, [q])

  useEffect(() => {
    void load()
  }, [load])

  function onSearch(e: FormEvent) {
    e.preventDefault()
    setQ(search)
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-serif text-2xl font-bold tracking-tight">Users</h1>
        <p className="text-sm text-muted-foreground">
          GET /api/user — directory search. There is no block/unlock endpoint in the API; manage accounts in your identity
          admin if needed.
        </p>
      </div>

      <form onSubmit={onSearch} className="flex flex-wrap gap-2">
        <Input
          className="max-w-sm"
          placeholder="Search by name, email, username…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Button type="submit" variant="secondary">
          Search
        </Button>
      </form>

      {error && <p className="text-sm text-destructive">{error}</p>}

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
                  <TableHead>Username</TableHead>
                  <TableHead>Full name</TableHead>
                  <TableHead className="font-mono text-xs">User id</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.map((u) => (
                  <TableRow key={u.id}>
                    <TableCell className="font-medium">{u.userName}</TableCell>
                    <TableCell>{u.fullName || "—"}</TableCell>
                    <TableCell className="max-w-[220px] truncate text-xs text-muted-foreground">{u.id}</TableCell>
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
