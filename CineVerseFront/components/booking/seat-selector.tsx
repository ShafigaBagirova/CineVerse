"use client"

import { useState, useEffect, useCallback, useRef, useMemo } from "react"
import { useSearchParams, useRouter } from "next/navigation"
import { Clock, MapPin, Film } from "lucide-react"
import { cn } from "@/lib/utils"
import { getMovieById } from "@/lib/api/movies"
import { getScreeningById } from "@/lib/api/screenings"
import { getOccupiedSeatsByScreening, getSeatsByScreening } from "@/lib/api/seats"
import { createSeatHold, getSeatHoldsByScreening, releaseSeatHold } from "@/lib/api/seat-holds"
import { ApiError } from "@/lib/api/types"
import { useAuth } from "@/components/providers/auth-provider"
import { normalizeSeatId, occupancyKindBySeatId } from "@/lib/booking/seat-status-map"

function safeSeatArray<T>(value: T[] | null | undefined): T[] {
  return Array.isArray(value) ? value : []
}

export type SeatVisualStatus = "selected" | "held" | "busy" | "available"

interface SeatRow {
  id: number
  row: string
  number: number
}

function formatSeatLabel(row: string, number: number): string {
  return `${row}${number}`
}

/**
 * Render priority (backend truth first):
 * 1) Sold / purchased (non-payable) → busy
 * 2) Held by someone else → held
 * 3) Our active hold or local selection for a free seat → selected
 * 4) available
 * A purchased or non-active seat must never show as selected.
 * `heldIds` = occupancy "Held"; `myHoldSeatIds` = our Active holds from API (+ short-lived optimistic merge).
 */
function computeSeatStatus(
  seatId: number,
  selectedSeatIds: Set<number>,
  soldIds: Set<number>,
  heldIds: Set<number>,
  myHoldSeatIds: Set<number>
): SeatVisualStatus {
  if (soldIds.has(seatId)) return "busy"
  if (heldIds.has(seatId) && !myHoldSeatIds.has(seatId)) return "held"
  if (myHoldSeatIds.has(seatId)) return "selected"
  if (selectedSeatIds.has(seatId)) return "selected"
  return "available"
}

export function SeatSelector() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const { status: authStatus, user } = useAuth()
  const movieId = Number(searchParams.get("movie")) || 1
  const screeningId = Number(searchParams.get("screeningId")) || 0
  const cinemaParam = searchParams.get("cinema") || ""
  const timeParam = searchParams.get("time") || ""

  const [movieTitle, setMovieTitle] = useState("Movie")
  const [price, setPrice] = useState(0)
  const [seatRows, setSeatRows] = useState<SeatRow[]>([])
  const [soldIds, setSoldIds] = useState<Set<number>>(() => new Set())
  const [heldIds, setHeldIds] = useState<Set<number>>(() => new Set())
  /** Seats with an active hold owned by the current user (from GET seat-holds or after create). */
  const [myHoldSeatIds, setMyHoldSeatIds] = useState<Set<number>>(() => new Set())
  const [selectedSeatIds, setSelectedSeatIds] = useState<Set<number>>(() => new Set())
  /** Known hold DB ids for seats already held (from API or after checkout), for release on deselect. */
  const holdIdBySeatIdRef = useRef<Map<number, number>>(new Map())

  const loadGen = useRef(0)
  /** Clear session-only seat state when switching screening (avoids ghost selections). */
  const lastScreeningIdRef = useRef(screeningId)
  const [loading, setLoading] = useState(false)
  const [checkoutBusy, setCheckoutBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [timeLeft, setTimeLeft] = useState(600)
  const seatMetaById = useMemo(() => {
    const map = new Map<number, { row: string; number: number }>()
    for (const seat of seatRows) map.set(seat.id, { row: seat.row, number: seat.number })
    return map
  }, [seatRows])

  useEffect(() => {
    const t = setInterval(() => {
      setTimeLeft((prev) => (prev <= 0 ? 0 : prev - 1))
    }, 1000)
    return () => clearInterval(t)
  }, [])

  useEffect(() => {
    if (lastScreeningIdRef.current === screeningId) return
    lastScreeningIdRef.current = screeningId
    setSelectedSeatIds(new Set())
    setMyHoldSeatIds(new Set())
    holdIdBySeatIdRef.current.clear()
  }, [screeningId])

  const loadSeats = useCallback(async () => {
    if (!screeningId) {
      setError("Missing screening id.")
      setSeatRows([])
      setMyHoldSeatIds(new Set())
      setLoading(false)
      return
    }

    const run = ++loadGen.current
    setLoading(true)

    try {
      const settled = await Promise.allSettled([
        getMovieById(movieId),
        getScreeningById(screeningId),
        getSeatsByScreening(screeningId),
        getOccupiedSeatsByScreening(screeningId),
      ])

      if (run !== loadGen.current) return

      const movie = settled[0].status === "fulfilled" ? settled[0].value : null
      const screeningInfo = settled[1].status === "fulfilled" ? settled[1].value : null
      const screeningSeats = safeSeatArray(settled[2].status === "fulfilled" ? settled[2].value : null)
      const occupiedList = safeSeatArray(settled[3].status === "fulfilled" ? settled[3].value : null)

      if (movie && typeof movie === "object" && "title" in movie) {
        setMovieTitle(String((movie as { title?: string }).title ?? "Movie"))
      }
      if (screeningInfo && typeof screeningInfo === "object" && "price" in screeningInfo) {
        setPrice(Number((screeningInfo as { price?: number }).price) || 0)
      } else {
        setPrice(0)
      }

      if (settled[2].status === "rejected" || screeningSeats.length === 0) {
        setSeatRows([])
        setSoldIds(new Set())
        setHeldIds(new Set())
        setMyHoldSeatIds(new Set())
        setError("Could not load seats for this screening.")
        return
      }

      const occupancy = occupancyKindBySeatId(occupiedList)
      const sold = new Set<number>()
      const held = new Set<number>()
      occupancy.forEach((kind, id) => {
        if (kind === "Sold") sold.add(id)
        else if (kind === "Held") held.add(id)
      })

      const rows: SeatRow[] = screeningSeats
        .filter((s) => s.isActive)
        .map((s) => {
          const id = normalizeSeatId(s.seatId ?? s.SeatId) ?? s.seatId
          return { id, row: s.row, number: s.number }
        })
        .filter((s) => Number.isFinite(s.id))

      setSoldIds(sold)
      setHeldIds(held)
      setSeatRows(rows)

      const rowIds = new Set(rows.map((r) => r.id))

      if (authStatus !== "authenticated" || !user?.userId) {
        setMyHoldSeatIds(new Set())
        setSelectedSeatIds((prev) => {
          const next = new Set<number>()
          for (const id of prev) {
            if (!rowIds.has(id)) continue
            if (sold.has(id)) continue
            if (held.has(id)) continue
            next.add(id)
          }
          return next
        })
      } else {
        let fromApi: Set<number> | undefined
        try {
          const holdsData = await getSeatHoldsByScreening(screeningId, 1, 200)
          if (run !== loadGen.current) return
          const items = holdsData.items ?? []
          const uid = user.userId
          fromApi = new Set<number>()
          const holdMapFromApi = new Map<number, number>()
          for (const h of items) {
            if (h.status !== "Active") continue
            if (h.userId === uid || String(h.userId).toLowerCase() === uid.toLowerCase()) {
              fromApi.add(h.seatId)
              if (h.id > 0) holdMapFromApi.set(h.seatId, h.id)
            }
          }
          if (holdMapFromApi.size > 0) {
            holdMapFromApi.forEach((holdId, seatId) => {
              holdIdBySeatIdRef.current.set(seatId, holdId)
            })
          }
          setMyHoldSeatIds((prev) => {
            const merged = new Set<number>()
            for (const id of fromApi!) {
              if (!sold.has(id)) merged.add(id)
            }
            for (const id of prev) {
              if (sold.has(id)) continue
              if (merged.has(id)) continue
              if (held.has(id)) merged.add(id)
            }
            return merged
          })
          setSelectedSeatIds((prev) => {
            const next = new Set<number>()
            for (const id of fromApi!) {
              if (rowIds.has(id) && !sold.has(id)) next.add(id)
            }
            for (const id of prev) {
              if (!rowIds.has(id)) continue
              if (sold.has(id)) continue
              if (held.has(id) && !fromApi!.has(id)) continue
              next.add(id)
            }
            return next
          })
        } catch {
          if (run !== loadGen.current) return
          setMyHoldSeatIds((prev) => {
            const next = new Set<number>()
            for (const id of prev) {
              if (!sold.has(id)) next.add(id)
            }
            return next
          })
          setSelectedSeatIds((prev) => {
            const next = new Set<number>()
            for (const id of prev) {
              if (!rowIds.has(id)) continue
              if (sold.has(id)) continue
              if (held.has(id)) continue
              next.add(id)
            }
            return next
          })
        }
      }

      const warnings: string[] = []
      ;[0, 1, 3].forEach((i) => {
        if (settled[i].status === "rejected") {
          const r = settled[i] as PromiseRejectedResult
          warnings.push(r.reason instanceof Error ? r.reason.message : String(r.reason))
        }
      })
      if (warnings.length > 0) setError(warnings.join(" · "))
    } catch (e) {
      if (run === loadGen.current) {
        setSeatRows([])
        setError(e instanceof Error ? e.message : "Failed to load seats.")
      }
    } finally {
      if (run === loadGen.current) {
        setLoading(false)
      }
    }
  }, [movieId, screeningId, authStatus, user?.userId])

  useEffect(() => {
    void loadSeats()
  }, [loadSeats])

  const rows = Array.from(new Set(seatRows.map((s) => s.row))).sort((a, b) => a.localeCompare(b))

  /** Seats that are payable / show as selected in the grid (excludes sold & others' holds). */
  const payableSeatIdsArray = Array.from(selectedSeatIds).filter(
    (id) => !soldIds.has(id) && !(heldIds.has(id) && !myHoldSeatIds.has(id))
  )
  const selectedSeatLabels = payableSeatIdsArray
    .map((id) => seatMetaById.get(id))
    .filter((s): s is { row: string; number: number } => s != null)
    .map((s) => formatSeatLabel(s.row, s.number))
  const total = payableSeatIdsArray.length > 0 ? price * payableSeatIdsArray.length : 0

  /** Load active holds for an explicit seat-id snapshot (avoids stale selectedSeatIds closures). */
  const fetchActiveHoldsForSeatIds = useCallback(
    async (seatIds: readonly number[]) => {
      if (!screeningId || !user?.userId) return []
      const holdsData = await getSeatHoldsByScreening(screeningId, 1, 200)
      const uid = user.userId
      const want = new Set(seatIds)
      return (holdsData.items ?? []).filter(
        (h) =>
          h.status === "Active" &&
          want.has(h.seatId) &&
          (h.userId === uid || String(h.userId).toLowerCase() === uid.toLowerCase())
      )
    },
    [screeningId, user?.userId]
  )

  const handleContinueToCheckout = async () => {
    if (checkoutBusy) return
    if (!screeningId) return
    if (authStatus !== "authenticated" || !user?.userId) {
      setError("Sign in to continue to checkout.")
      return
    }

    const snapshotSeatIds = Array.from(selectedSeatIds)
      .filter((id) => !soldIds.has(id) && !(heldIds.has(id) && !myHoldSeatIds.has(id)))
      .sort((a, b) => a - b)
    if (snapshotSeatIds.length === 0) {
      setError("No payable seats selected. Refresh if seats were purchased or released.")
      return
    }
    const labelForSeatId = (seatId: number) => {
      const meta = seatMetaById.get(seatId)
      return meta ? formatSeatLabel(meta.row, meta.number) : "Unknown seat"
    }
    const checkoutTotal = price * snapshotSeatIds.length

    setCheckoutBusy(true)
    setError(null)
    try {
      console.info("[checkout-resolution] before-create-seat-holds", {
        screeningId,
        selectedSeatIds: snapshotSeatIds,
        holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
        selectedSeatSummarySource: snapshotSeatIds.map((seatId) => {
          const seat = seatRows.find((s) => s.id === seatId)
          return {
            seatId,
            row: seat?.row ?? null,
            number: seat?.number ?? null,
            label: seat ? formatSeatLabel(seat.row, seat.number) : `#${seatId}`,
          }
        }),
      })

      const settled = await Promise.allSettled(
        snapshotSeatIds.map((seatId) => createSeatHold({ screeningId, seatId }))
      )

      const failed: { seatId: number; label: string; message: string }[] = []
      settled.forEach((result, i) => {
        const seatId = snapshotSeatIds[i]!
        if (result.status === "rejected") {
          const message =
            result.reason instanceof Error ? result.reason.message : String(result.reason)
          console.error("[seat-hold] rejected", { screeningId, seatId, label: labelForSeatId(seatId), message })
          failed.push({ seatId, label: labelForSeatId(seatId), message })
        } else {
          const holdResponse = result.value
          setMyHoldSeatIds((prev) => new Set(prev).add(seatId))
          if (holdResponse.id > 0) {
            holdIdBySeatIdRef.current.set(seatId, holdResponse.id)
          }
          console.info("[seat-hold] fulfilled", {
            screeningId,
            seatId,
            label: labelForSeatId(seatId),
            holdIdFromResponse: holdResponse.id > 0 ? holdResponse.id : "(use GET merge or 0)",
            selectedSeatIds: snapshotSeatIds,
            holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
          })
        }
      })

      if (failed.length > 0) {
        setError(
          `Could not hold: ${failed.map((f) => `${f.label} — ${f.message}`).join("; ")}`
        )
        return
      }

      void loadSeats()

      const holdIdBySeat = new Map<number, number>()
      snapshotSeatIds.forEach((seatId) => {
        const cached = holdIdBySeatIdRef.current.get(seatId)
        if (cached && cached > 0) holdIdBySeat.set(seatId, cached)
      })
      settled.forEach((r, i) => {
        if (r.status !== "fulfilled") return
        const seatId = snapshotSeatIds[i]!
        if (r.value.id > 0) {
          holdIdBySeat.set(seatId, r.value.id)
          holdIdBySeatIdRef.current.set(seatId, r.value.id)
        }
      })

      try {
        const merged = await fetchActiveHoldsForSeatIds(snapshotSeatIds)
        merged.forEach((h) => {
          if (h.id > 0 && (!holdIdBySeat.has(h.seatId) || holdIdBySeat.get(h.seatId) === 0)) {
            holdIdBySeat.set(h.seatId, h.id)
            holdIdBySeatIdRef.current.set(h.seatId, h.id)
          }
        })
      } catch (e) {
        console.warn("[seat-hold] optional GET merge skipped (checkout still proceeds)", e)
      }

      const orderedHoldIds = snapshotSeatIds
        .map((sid) => holdIdBySeat.get(sid))
        .filter((id): id is number => id !== undefined && id > 0)
      const unresolvedSeatIds = snapshotSeatIds.filter((sid) => !holdIdBySeat.get(sid))

      console.info("[checkout-resolution] after-hold-resolution", {
        screeningId,
        selectedSeatIds: snapshotSeatIds,
        holdMapping: Object.fromEntries(holdIdBySeat.entries()),
        unresolvedSeatIds,
        selectedSeatSummarySource: snapshotSeatIds.map((seatId) => {
          const seat = seatMetaById.get(seatId)
          return {
            seatId,
            row: seat?.row ?? null,
            number: seat?.number ?? null,
            label: seat ? formatSeatLabel(seat.row, seat.number) : "Unknown seat",
          }
        }),
      })

      if (orderedHoldIds.length === 0) {
        setError("Could not resolve seat hold ids for checkout. Please tap Continue again.")
        return
      }

      const firstId = orderedHoldIds[0]!
      const seatsParam = snapshotSeatIds.map((id) => labelForSeatId(id)).join(", ")
      const holdIdsParam = orderedHoldIds.join(",")

      router.push(
        `/checkout?movie=${movieId}&cinema=${encodeURIComponent(cinemaParam)}&time=${encodeURIComponent(timeParam)}&screeningId=${screeningId}&seats=${encodeURIComponent(seatsParam)}&seatHoldId=${firstId}&seatHoldIds=${encodeURIComponent(holdIdsParam)}&total=${checkoutTotal}`
      )
    } catch (e: unknown) {
      const msg =
        e instanceof ApiError ? e.message : e instanceof Error ? e.message : "Could not continue to checkout."
      setError(msg)
    } finally {
      setCheckoutBusy(false)
    }
  }

  const tryReleaseSeat = async (seatId: number) => {
    const label = (() => {
      const seat = seatMetaById.get(seatId)
      return seat ? formatSeatLabel(seat.row, seat.number) : "Unknown seat"
    })()
    let holdId = holdIdBySeatIdRef.current.get(seatId)
    console.info("[seat-release] start", {
      screeningId,
      seatId,
      seatHoldId: holdId ?? null,
      label,
      selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
      holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
    })
    if (!holdId && screeningId) {
      try {
        const page = await getSeatHoldsByScreening(screeningId, 1, 100)
        const uid = user?.userId
        const row = page.items.find(
          (h) =>
            h.seatId === seatId &&
            h.status === "Active" &&
            !!uid &&
            (h.userId === uid || String(h.userId).toLowerCase() === uid.toLowerCase())
        )
        if (row) {
          holdId = row.id
          holdIdBySeatIdRef.current.set(seatId, row.id)
        }
      } catch {
        /* ignore */
      }
    }
    if (!holdId || holdId <= 0) {
      console.warn("[seat-release] skipped-no-hold-id", {
        screeningId,
        seatId,
        label,
        selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
        holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
      })
      holdIdBySeatIdRef.current.delete(seatId)
      setSelectedSeatIds((prev) => {
        const next = new Set(prev)
        next.delete(seatId)
        return next
      })
      setMyHoldSeatIds((prev) => {
        const next = new Set(prev)
        next.delete(seatId)
        return next
      })
      return true
    }

    try {
      console.info("[seat-release] before-api-call", {
        screeningId,
        seatId,
        seatHoldId: holdId,
        label,
        selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
        holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
      })
      await releaseSeatHold(holdId)
      holdIdBySeatIdRef.current.delete(seatId)
      setSelectedSeatIds((prev) => {
        const next = new Set(prev)
        next.delete(seatId)
        return next
      })
      setMyHoldSeatIds((prev) => {
        const next = new Set(prev)
        next.delete(seatId)
        return next
      })
      console.info("[seat-release] success", {
        screeningId,
        seatId,
        seatHoldId: holdId,
        label,
        selectedSeatIdsAfter: Array.from(selectedSeatIds).filter((id) => id !== seatId).sort((a, b) => a - b),
        holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
      })
      void loadSeats()
      return true
    } catch (e) {
      console.error("[seat-release] failed", {
        screeningId,
        seatId,
        seatHoldId: holdId,
        label,
        error: e instanceof Error ? e.message : String(e),
        status: e instanceof ApiError ? e.status : undefined,
        apiErrors: e instanceof ApiError ? e.errors : undefined,
        selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
        holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
      })
      setError("Could not release selected seat. Please try again.")
      return false
    }
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
          Select Your Seats
        </p>
        <h1 className="font-serif text-3xl font-bold text-foreground">{movieTitle}</h1>
        <div className="mt-2 flex flex-wrap items-center gap-4 text-sm text-muted-foreground">
          <span className="flex items-center gap-1">
            <MapPin className="h-4 w-4 text-primary" />
            {cinemaParam || "Cinema"}
          </span>
          <span className="flex items-center gap-1">
            <Clock className="h-4 w-4" />
            {timeParam || "TBD"}
          </span>
          <span className="flex items-center gap-1">
            <Film className="h-4 w-4" />
            Screening #{screeningId || "N/A"}
          </span>
        </div>
      </div>

      {loading && <p className="mb-4 text-sm text-muted-foreground">Loading seat availability...</p>}
      {error && <p className="mb-4 text-sm text-muted-foreground">{error}</p>}

      <div className="flex flex-col gap-8 lg:flex-row">
        <div className="flex-1">
          <div className="mb-8 text-center">
            <div className="mx-auto h-1.5 w-3/4 rounded-full bg-primary/30" />
            <p className="mt-2 text-xs font-medium uppercase tracking-wider text-muted-foreground">Screen</p>
          </div>

          <div className="flex flex-col items-center gap-2">
            {!loading && seatRows.length === 0 && (
              <p className="text-sm text-muted-foreground">No seats found for this screening.</p>
            )}
            {rows.map((row) => (
              <div key={row} className="flex items-center gap-2">
                <span className="w-6 text-right text-xs font-medium text-muted-foreground">{row}</span>
                <div className="flex gap-1.5">
                  {seatRows
                    .filter((seat) => seat.row === row)
                    .map((seat) => {
                      const seatStatus = computeSeatStatus(seat.id, selectedSeatIds, soldIds, heldIds, myHoldSeatIds)
                      const seatLabel = formatSeatLabel(seat.row, seat.number)
                      const isBusy = seatStatus === "busy"
                      const isHeld = seatStatus === "held"
                      const isSelected = seatStatus === "selected"
                      const isAvailable = seatStatus === "available"

                      return (
                        <button
                          key={seat.id}
                          type="button"
                          onClick={() => {
                            if (isBusy) return
                            if (isHeld && !myHoldSeatIds.has(seat.id)) return
                            if (myHoldSeatIds.has(seat.id) && !selectedSeatIds.has(seat.id)) return
                            setError(null)

                            if (selectedSeatIds.has(seat.id)) {
                              console.info("[seat-select] deselect-click", {
                                screeningId,
                                seatId: seat.id,
                                seatHoldId: holdIdBySeatIdRef.current.get(seat.id) ?? null,
                                label: seatLabel,
                                selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
                                holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
                              })
                              if (myHoldSeatIds.has(seat.id)) {
                                void tryReleaseSeat(seat.id)
                              } else {
                                setSelectedSeatIds((prev) => {
                                  const next = new Set(prev)
                                  next.delete(seat.id)
                                  return next
                                })
                              }
                              return
                            }

                            if (authStatus !== "authenticated") {
                              setError("Sign in to select seats.")
                              return
                            }

                            if (!isAvailable) return

                            console.info("[seat-select] select-click", {
                              screeningId,
                              seatId: seat.id,
                              seatHoldId: holdIdBySeatIdRef.current.get(seat.id) ?? null,
                              label: seatLabel,
                              selectedSeatIds: Array.from(selectedSeatIds).sort((a, b) => a - b),
                              holdMapping: Object.fromEntries(holdIdBySeatIdRef.current.entries()),
                            })
                            setSelectedSeatIds((prev) => new Set(prev).add(seat.id))
                          }}
                          disabled={isBusy || (isHeld && !myHoldSeatIds.has(seat.id)) || checkoutBusy}
                          aria-label={`Seat ${seatLabel}${
                            isBusy ? " (busy)" : isHeld ? " (held)" : isSelected ? " (selected)" : " (available)"
                          }`}
                          className={cn(
                            "flex h-7 w-7 items-center justify-center rounded-md text-[10px] font-medium transition-all sm:h-8 sm:w-8",
                            isBusy && "cursor-not-allowed bg-secondary/50 text-muted-foreground/30",
                            isHeld && "cursor-not-allowed bg-warning/20 text-warning",
                            isAvailable &&
                              "bg-secondary text-muted-foreground hover:bg-primary/20 hover:text-primary",
                            isSelected && "bg-primary text-primary-foreground shadow-md shadow-primary/20"
                          )}
                        >
                          {seat.number}
                        </button>
                      )
                    })}
                </div>
                <span className="w-6 text-xs font-medium text-muted-foreground">{row}</span>
              </div>
            ))}
          </div>

          <div className="mt-6 flex justify-center gap-6">
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded bg-secondary" />
              <span className="text-xs text-muted-foreground">Available</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded bg-primary" />
              <span className="text-xs text-muted-foreground">Selected</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded bg-secondary/50" />
              <span className="text-xs text-muted-foreground">Busy</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded bg-warning/20" />
              <span className="text-xs text-muted-foreground">Held</span>
            </div>
          </div>
        </div>

        <div className="w-full lg:w-72">
          <div className="sticky top-24 rounded-2xl border border-border/50 bg-card p-6">
            <h3 className="mb-4 font-serif text-lg font-bold text-foreground">Order Summary</h3>

            <div className="mb-4 rounded-lg bg-destructive/10 p-3 text-center">
              <p className="text-xs font-medium text-destructive">Seats held for</p>
              <p className="text-lg font-bold text-destructive">
                {String(Math.floor(timeLeft / 60)).padStart(2, "0")}:{String(timeLeft % 60).padStart(2, "0")}
              </p>
            </div>

            <div className="mb-4 flex flex-col gap-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Movie</span>
                <span className="font-medium text-foreground">{movieTitle}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Seats</span>
                <span className="max-w-[12rem] text-right font-medium text-foreground">
                  {selectedSeatLabels.length > 0 ? selectedSeatLabels.join(", ") : "None"}
                </span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Price per seat</span>
                <span className="font-medium text-foreground">{price} AZN</span>
              </div>
            </div>

            <div className="mb-4 border-t border-border/50 pt-3">
              <div className="flex justify-between">
                <span className="font-semibold text-foreground">Total</span>
                <span className="font-bold text-primary">{total} AZN</span>
              </div>
            </div>

            <button
              type="button"
              disabled={payableSeatIdsArray.length === 0 || checkoutBusy}
              onClick={() => void handleContinueToCheckout()}
              className={cn(
                "w-full rounded-lg py-3 text-center text-sm font-semibold transition-colors",
                payableSeatIdsArray.length > 0 && !checkoutBusy
                  ? "bg-primary text-primary-foreground hover:bg-primary/90"
                  : "cursor-not-allowed bg-secondary text-muted-foreground"
              )}
            >
              {checkoutBusy ? (
                <span className="inline-flex items-center justify-center gap-2">
                  <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground" />
                  Holding all seats…
                </span>
              ) : payableSeatIdsArray.length > 0 ? (
                "Continue to Checkout"
              ) : (
                "Select seats"
              )}
            </button>
            <p className="mt-2 text-center text-[10px] text-muted-foreground">
              Selection is saved in this step; checkout will complete your booking.
            </p>
          </div>
        </div>
      </div>
    </div>
  )
}
