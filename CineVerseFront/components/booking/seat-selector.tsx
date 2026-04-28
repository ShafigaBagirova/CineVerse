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
  type?: string
  seatType?: string
  isCouple?: boolean
}

function formatSeatLabel(row: string, number: number): string {
  return `${row}-${number}`
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
  const resetCheckout = searchParams.get("resetCheckout") === "1"

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
  const [holdOpsInFlight, setHoldOpsInFlight] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const [remainingSeconds, setRemainingSeconds] = useState<number | null>(null)
  const seatMetaById = useMemo(() => {
    const map = new Map<number, { row: string; number: number; type?: string; seatType?: string; isCouple?: boolean }>()
    for (const seat of seatRows) {
      map.set(seat.id, {
        row: seat.row,
        number: seat.number,
        type: seat.type,
        seatType: seat.seatType,
        isCouple: seat.isCouple,
      })
    }
    return map
  }, [seatRows])
  const holdSyncBusy = holdOpsInFlight > 0

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

      if (settled[2].status === "rejected") {
        setSeatRows([])
        setSoldIds(new Set())
        setHeldIds(new Set())
        setMyHoldSeatIds(new Set())
        setError("Could not load seats for this screening.")
        return
      }

      if (screeningSeats.length === 0) {
        setSeatRows([])
        setSoldIds(new Set())
        setHeldIds(new Set())
        setMyHoldSeatIds(new Set())
        setError("This hall has no seats configured. Please contact admin.")
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
        .filter((s) => Boolean(s.isActive ?? s.IsActive ?? false))
        .map((s) => {
          const id = normalizeSeatId(s.seatId ?? s.SeatId) ?? s.seatId
          return {
            id,
            row: String(s.row ?? s.Row ?? ""),
            number: Number(s.number ?? s.Number ?? 0),
            type: String(s.type ?? s.Type ?? "Standard"),
            seatType: typeof (s as { seatType?: unknown; SeatType?: unknown }).seatType === "string"
              ? String((s as { seatType?: string }).seatType)
              : typeof (s as { SeatType?: unknown }).SeatType === "string"
                ? String((s as { SeatType?: string }).SeatType)
                : undefined,
            isCouple: Boolean(
              (s as { isCouple?: unknown; IsCouple?: unknown }).isCouple ??
              (s as { IsCouple?: unknown }).IsCouple ??
              false
            ),
          }
        })
        .filter((s) => Number.isFinite(s.id) && s.row !== "" && Number.isFinite(s.number))

      if (rows.length === 0) {
        setSeatRows([])
        setSoldIds(new Set())
        setHeldIds(new Set())
        setMyHoldSeatIds(new Set())
        setError("This hall has no seats configured. Please contact admin.")
        return
      }

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
          const heldByOthersFromApi = new Set<number>()
          const holdMapFromApi = new Map<number, number>()
          for (const h of items) {
            const status = String(h.status ?? "").trim().toLowerCase()
            // Backend can surface active seat holds in different casing.
            if (status !== "active") continue
            if (h.userId === uid || String(h.userId).toLowerCase() === uid.toLowerCase()) {
              fromApi.add(h.seatId)
              if (h.id > 0) holdMapFromApi.set(h.seatId, h.id)
            } else {
              heldByOthersFromApi.add(h.seatId)
            }
          }
          if (heldByOthersFromApi.size > 0) {
            setHeldIds((prev) => {
              const next = new Set(prev)
              heldByOthersFromApi.forEach((id) => {
                if (!sold.has(id)) next.add(id)
              })
              return next
            })
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
            for (const id of prev) {
              if (!rowIds.has(id)) continue
              if (sold.has(id)) continue
              if (!fromApi!.has(id)) continue
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
    if (remainingSeconds == null) return
    const t = setInterval(() => {
      setRemainingSeconds((prev) => {
        if (prev == null) return null
        if (prev > 1) return prev - 1
        holdIdBySeatIdRef.current.clear()
        setSelectedSeatIds(new Set())
        setMyHoldSeatIds(new Set())
        setError("Seat hold expired. Please reselect.")
        void loadSeats()
        return null
      })
    }, 1000)
    return () => clearInterval(t)
  }, [remainingSeconds, loadSeats])

  useEffect(() => {
    if (!resetCheckout) return
    if (typeof window === "undefined") return
    const oldLocalSeatHoldId = localStorage.getItem("seatHoldId")
    const oldSessionSeatHoldId = sessionStorage.getItem("seatHoldId")
    console.log("[seat-reset] cleared old seatHoldId", {
      localStorageSeatHoldId: oldLocalSeatHoldId,
      sessionStorageSeatHoldId: oldSessionSeatHoldId,
    })
    const keys = [
      "seatHoldId",
      "seatHoldIds",
      "selectedSeats",
      "checkoutState",
      "bookingCheckout",
      "bookingState",
      "paymentState",
      "paymentStatus",
    ]
    keys.forEach((k) => {
      localStorage.removeItem(k)
      sessionStorage.removeItem(k)
    })
  }, [resetCheckout])

  useEffect(() => {
    if (lastScreeningIdRef.current === screeningId) return
    lastScreeningIdRef.current = screeningId
    setSeatRows([])
    setSoldIds(new Set())
    setHeldIds(new Set())
    setSelectedSeatIds(new Set())
    setMyHoldSeatIds(new Set())
    setRemainingSeconds(null)
    setError(null)
    holdIdBySeatIdRef.current.clear()
  }, [screeningId])

  useEffect(() => {
    void loadSeats()
  }, [loadSeats])

  const rows = Array.from(new Set(seatRows.map((s) => s.row))).sort((a, b) => a.localeCompare(b))

  /** Seats that are payable / show as selected in the grid (excludes sold & others' holds). */
  const payableSeatIdsArray = Array.from(selectedSeatIds).filter(
    (id) => !soldIds.has(id) && !(heldIds.has(id) && !myHoldSeatIds.has(id))
  )
  const isVipSeatMeta = (meta: { row: string; number: number; type?: string; seatType?: string; isCouple?: boolean }) => {
    const seatType = String(meta.type ?? "").trim().toLowerCase()
    if (seatType === "vip") return true
    const rowNumber = Number(String(meta.row ?? "").trim())
    return Number.isFinite(rowNumber) && rowNumber === 2
  }

  const isCoupleSeatMeta = (meta: { row: string; number: number; type?: string; seatType?: string; isCouple?: boolean }) => {
    if (meta.isCouple === true) return true
    const type = String(meta.type ?? "").trim().toLowerCase()
    const seatType = String(meta.seatType ?? "").trim().toLowerCase()
    return type === "couple" || seatType === "couple"
  }

  const getSeatUnitPrice = (meta: { row: string; number: number; type?: string; seatType?: string; isCouple?: boolean } | undefined) => {
    if (!meta) return price
    if (isCoupleSeatMeta(meta)) return price * 2
    if (isVipSeatMeta(meta)) return price * 1.5
    return price
  }

  const selectedSeatLabels = payableSeatIdsArray
    .map((id) => seatMetaById.get(id))
    .filter((s): s is { row: string; number: number; type?: string; seatType?: string; isCouple?: boolean } => s != null)
    .map((s) => formatSeatLabel(s.row, s.number))
  const selectedSeatUnitPrices = payableSeatIdsArray.map((id) => getSeatUnitPrice(seatMetaById.get(id)))
  const total = selectedSeatUnitPrices.reduce((sum, seatPrice) => sum + seatPrice, 0)
  const uniqueSeatUnitPrices = Array.from(new Set(selectedSeatUnitPrices.map((v) => Number(v.toFixed(2)))))
  const pricePerSeatLabel =
    payableSeatIdsArray.length === 0
      ? `${price} AZN`
      : uniqueSeatUnitPrices.length === 1
        ? `${uniqueSeatUnitPrices[0]} AZN`
        : "Mixed"
  const handleContinueToCheckout = async () => {
    if (checkoutBusy || holdSyncBusy) return
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
    const checkoutTotal = snapshotSeatIds.reduce((sum, seatId) => {
      return sum + getSeatUnitPrice(seatMetaById.get(seatId))
    }, 0)

    setCheckoutBusy(true)
    setError(null)
    try {
      console.info("[checkout-resolution] before-checkout-hold-resolution", {
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

      const holdIdBySeat = new Map<number, number>()
      const unresolvedSeatIds: number[] = []
      let soonestExpiry: number | null = null
      const holdResponses: Array<{ seatId: number; holdId: number; expiresAtUtc: string }> = []
      for (const seatId of snapshotSeatIds) {
        holdIdBySeatIdRef.current.delete(seatId)
      }
      for (const seatId of snapshotSeatIds) {
        try {
          const fresh = await createSeatHold({ screeningId, seatId })
          if (fresh?.id && fresh.id > 0) {
            console.log("[seat-hold] newly created/returned seatHoldId", {
              seatId,
              seatHoldId: fresh.id,
              screeningId,
            })
            holdIdBySeatIdRef.current.set(seatId, fresh.id)
            holdIdBySeat.set(seatId, fresh.id)
            setMyHoldSeatIds((prev) => new Set(prev).add(seatId))
            holdResponses.push({ seatId, holdId: fresh.id, expiresAtUtc: fresh.expiresAtUtc })
            const expiresAt = new Date(fresh.expiresAtUtc).getTime()
            if (Number.isFinite(expiresAt) && expiresAt > Date.now()) {
              soonestExpiry = soonestExpiry == null ? expiresAt : Math.min(soonestExpiry, expiresAt)
            }
          } else {
            unresolvedSeatIds.push(seatId)
          }
        } catch {
          unresolvedSeatIds.push(seatId)
        }
      }

      const orderedHoldIds = snapshotSeatIds
        .map((sid) => holdIdBySeat.get(sid))
        .filter((id): id is number => id !== undefined && id > 0)

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

      if (unresolvedSeatIds.length > 0 || orderedHoldIds.length !== snapshotSeatIds.length) {
        unresolvedSeatIds.forEach((id) => holdIdBySeatIdRef.current.delete(id))
        setMyHoldSeatIds((prev) => {
          const next = new Set(prev)
          unresolvedSeatIds.forEach((id) => next.delete(id))
          return next
        })
        setSelectedSeatIds((prev) => {
          const next = new Set(prev)
          unresolvedSeatIds.forEach((id) => next.delete(id))
          return next
        })
        await loadSeats()
        setError("Some seat holds could not be created. Please try selecting seats again.")
        setRemainingSeconds(null)
        return
      }

      const firstId = orderedHoldIds[0]!
      setError(null)
      if (soonestExpiry != null) {
        const nextSeconds = Math.max(1, Math.floor((soonestExpiry - Date.now()) / 1000))
        setRemainingSeconds(nextSeconds)
      } else {
        setRemainingSeconds(600)
      }
      const seatsParam = snapshotSeatIds.map((id) => labelForSeatId(id)).join(", ")
      const holdIdsParam = orderedHoldIds.join(",")
      if (typeof window !== "undefined") {
        // Persist only the latest, fresh checkout hold state.
        localStorage.removeItem("paymentState")
        localStorage.removeItem("paymentStatus")
        sessionStorage.removeItem("paymentState")
        sessionStorage.removeItem("paymentStatus")
        localStorage.setItem("seatHoldId", String(firstId))
        localStorage.setItem("seatHoldIds", holdIdsParam)
        sessionStorage.setItem("seatHoldId", String(firstId))
        sessionStorage.setItem("seatHoldIds", holdIdsParam)
      }
      const checkoutUrl = `/checkout?movie=${movieId}&cinema=${encodeURIComponent(cinemaParam)}&time=${encodeURIComponent(timeParam)}&screeningId=${screeningId}&seats=${encodeURIComponent(seatsParam)}&seatHoldId=${firstId}&seatHoldIds=${encodeURIComponent(holdIdsParam)}&total=${checkoutTotal}`

      console.log("CHECKOUT_NAVIGATION_DATA", {
        selectedSeatIds: snapshotSeatIds,
        holdResponse: holdResponses,
        seatHoldIds: orderedHoldIds,
        checkoutUrl,
      })

      router.push(checkoutUrl)
    } catch (e: unknown) {
      const msg =
        e instanceof ApiError ? e.message : e instanceof Error ? e.message : "Could not continue to checkout."
      setError(msg)
    } finally {
      setCheckoutBusy(false)
    }
  }

  const tryReleaseSeat = async (seatId: number) => {
    setHoldOpsInFlight((v) => v + 1)
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
        if (next.size === 0) setRemainingSeconds(null)
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
        if (next.size === 0) setRemainingSeconds(null)
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
    } finally {
      setHoldOpsInFlight((v) => Math.max(0, v - 1))
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
                      const isSelected = seatStatus === "selected"
                      const isHeld = !isSelected && seatStatus === "held"
                      const isBusy = !isSelected && !isHeld && seatStatus === "busy"
                      const isAvailable = !isSelected && !isHeld && !isBusy
                      const seatMeta = {
                        row: seat.row,
                        number: seat.number,
                        type: seat.type,
                        seatType: seat.seatType,
                        isCouple: seat.isCouple,
                      }
                      const isCoupleSeat = isCoupleSeatMeta(seatMeta)
                      const isVipSeat = isVipSeatMeta(seatMeta)

                      return (
                        <button
                          key={seat.id}
                          type="button"
                          onClick={() => {
                            if (isBusy) return
                            if (isHeld && !myHoldSeatIds.has(seat.id)) return
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
                                  if (next.size === 0) setRemainingSeconds(null)
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
                            setHoldOpsInFlight((v) => v + 1)
                            void (async () => {
                              try {
                                holdIdBySeatIdRef.current.delete(seat.id)
                                setError(null)
                                setError(null)
                                setSelectedSeatIds((prev) => {
                                  const next = new Set(prev).add(seat.id)
                                  if (prev.size === 0) setRemainingSeconds(600)
                                  return next
                                })
                              } catch (e) {
                                setError(e instanceof Error ? e.message : "Could not hold this seat.")
                              } finally {
                                setHoldOpsInFlight((v) => Math.max(0, v - 1))
                              }
                            })()
                          }}
                          disabled={isBusy || (isHeld && !myHoldSeatIds.has(seat.id)) || checkoutBusy || holdSyncBusy}
                          aria-label={`Seat ${seatLabel}${
                            isBusy
                              ? " (busy)"
                              : isHeld
                                ? " (held)"
                                : isSelected
                                  ? " (selected)"
                                  : " (available)"
                          }`}
                          className={cn(
                            "flex h-7 w-7 items-center justify-center rounded-md text-[10px] font-medium transition-all sm:h-8 sm:w-8",
                            isSelected
                              ? "bg-primary text-primary-foreground shadow-md shadow-primary/20"
                              : isBusy
                                  ? "bg-gray-600 border border-gray-700 text-white cursor-not-allowed opacity-100"
                                  : isHeld
                                    ? "bg-blue-600 border border-blue-700 text-white opacity-100 cursor-not-allowed"
                                    : isCoupleSeat
                                      ? "bg-red-500 border border-red-600 text-white hover:bg-red-400"
                                      : isVipSeat
                                        ? "bg-yellow-400 border border-yellow-500 text-yellow-900 hover:bg-yellow-300"
                                        : "bg-secondary text-muted-foreground hover:bg-primary/20 hover:text-primary"
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
              <div className="h-4 w-4 rounded border border-gray-700 bg-gray-600" />
              <span className="text-xs text-muted-foreground">Busy</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border border-blue-700 bg-blue-600" />
              <span className="text-xs text-muted-foreground">Held</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border border-red-600 bg-red-500" />
              <span className="text-xs text-muted-foreground">Couple</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="h-4 w-4 rounded border border-yellow-500 bg-yellow-400" />
              <span className="text-xs text-muted-foreground">VIP</span>
            </div>
          </div>
        </div>

        <div className="w-full lg:w-72">
          <div className="sticky top-24 rounded-2xl border border-border/50 bg-card p-6">
            <h3 className="mb-4 font-serif text-lg font-bold text-foreground">Order Summary</h3>

            {selectedSeatIds.size > 0 && (
              <div className="mb-4 rounded-lg bg-destructive/10 p-3 text-center">
                <p className="text-xs font-medium text-destructive">Seats held for</p>
                <p className="text-lg font-bold text-destructive">
                  {remainingSeconds == null
                    ? "10:00"
                    : `${String(Math.floor(remainingSeconds / 60)).padStart(2, "0")}:${String(remainingSeconds % 60).padStart(2, "0")}`}
                </p>
              </div>
            )}

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
                <span className="font-medium text-foreground">{pricePerSeatLabel}</span>
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
              disabled={
                payableSeatIdsArray.length === 0 ||
                checkoutBusy ||
                holdSyncBusy
              }
              onClick={() => void handleContinueToCheckout()}
              className={cn(
                "w-full rounded-lg py-3 text-center text-sm font-semibold transition-colors",
                payableSeatIdsArray.length > 0 && !checkoutBusy
                  ? "bg-primary text-primary-foreground hover:bg-primary/90"
                  : "cursor-not-allowed bg-secondary text-muted-foreground"
              )}
            >
              {checkoutBusy || holdSyncBusy ? (
                <span className="inline-flex items-center justify-center gap-2">
                  <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground/30 border-t-primary-foreground" />
                  Holding seats…
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
