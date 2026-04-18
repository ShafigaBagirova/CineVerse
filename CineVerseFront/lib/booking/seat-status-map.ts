import type { GetOccupiedSeatsByScreeningResponse } from "@/lib/api/seats"

/** Normalize seat id from API (number or numeric string). */
export function normalizeSeatId(value: unknown): number | undefined {
  if (typeof value === "number" && Number.isFinite(value)) return value
  if (typeof value === "string" && value.trim() !== "") {
    const n = Number(value)
    if (Number.isFinite(n)) return n
  }
  return undefined
}

export type UiSeatStatus = "free" | "busy" | "held" | "selected"

export interface SeatRowInput {
  seatId?: number
  SeatId?: number
  row: string
  number: number
  isActive?: boolean
}

/**
 * Backend `/occupied-seats` returns both sold tickets and active holds, distinguished by
 * `occupancyType`: "Sold" | "Held". Map each seat id to that classification.
 */
export function occupancyKindBySeatId(
  occupiedRows: GetOccupiedSeatsByScreeningResponse[]
): Map<number, "Sold" | "Held"> {
  const map = new Map<number, "Sold" | "Held">()
  for (const row of occupiedRows) {
    const raw = row as GetOccupiedSeatsByScreeningResponse & {
      OccupancyType?: string
      SeatId?: number
    }
    const id = normalizeSeatId(raw.seatId ?? raw.SeatId)
    if (id === undefined) continue
    const typeRaw = raw.occupancyType ?? raw.OccupancyType ?? ""
    const t = String(typeRaw).trim().toLowerCase()
    if (t === "sold") map.set(id, "Sold")
    else if (t === "held") map.set(id, "Held")
  }
  return map
}

export interface MapSeatStatusArgs {
  screeningSeats: SeatRowInput[]
  availableSeatIds: Set<number>
  occupancyBySeatId: Map<number, "Sold" | "Held">
  myHoldSeatIds: Set<number>
}

/**
 * Single source of truth for UI seat state, aligned with backend semantics:
 * 1) Occupancy "Sold" → busy (purchased / non-payable — never "selected")
 * 2) Occupancy "Held" (not our active hold) → held
 * 3) Current user's active hold → selected
 * 4) Seat id in available-seats response → free
 * 5) Otherwise → held (unavailable)
 */
export function mapSeatsToViewModels(args: MapSeatStatusArgs): {
  seatId: number
  row: string
  number: number
  status: UiSeatStatus
}[] {
  const { screeningSeats, availableSeatIds, occupancyBySeatId, myHoldSeatIds } = args

  return screeningSeats
    .filter((seat) => seat.isActive)
    .map((seat) => {
      const id = normalizeSeatId(seat.seatId ?? seat.SeatId)
      if (id === undefined) {
        return null
      }

      const occ = occupancyBySeatId.get(id)
      if (occ === "Sold") {
        return { seatId: id, row: seat.row, number: seat.number, status: "busy" as const }
      }
      if (occ === "Held" && !myHoldSeatIds.has(id)) {
        return { seatId: id, row: seat.row, number: seat.number, status: "held" as const }
      }

      if (myHoldSeatIds.has(id)) {
        return { seatId: id, row: seat.row, number: seat.number, status: "selected" as const }
      }

      if (availableSeatIds.has(id)) {
        return { seatId: id, row: seat.row, number: seat.number, status: "free" as const }
      }

      return { seatId: id, row: seat.row, number: seat.number, status: "held" as const }
    })
    .filter((s): s is NonNullable<typeof s> => s !== null)
    .sort((a, b) => (a.row === b.row ? a.number - b.number : a.row.localeCompare(b.row)))
}
