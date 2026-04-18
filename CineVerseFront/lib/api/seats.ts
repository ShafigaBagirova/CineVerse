import { apiRequest } from "@/lib/api/http"

function asList<T>(data: T[] | null | undefined): T[] {
  return Array.isArray(data) ? data : []
}

export type SeatType = "Standard" | "VIP" | "Couple"
export type SeatHoldStatus = "Active" | "Released" | "Expired" | "Purchased"

export interface GetSeatsByScreeningResponse {
  seatId: number
  /** Present if JSON used PascalCase */
  SeatId?: number
  hallId: number
  row: string
  number: number
  type: SeatType
  isActive: boolean
  status: SeatHoldStatus
}

export interface GetOccupiedSeatsByScreeningResponse {
  seatId: number
  row: string
  number: number
  occupancyType: string
}

export interface GetAvailableSeatsByScreeningResponse {
  seatId: number
  row: string
  number: number
}

export async function getSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetSeatsByScreeningResponse[]>(`/api/seat/${screeningId}/seats`, {
    method: "GET",
    auth: false,
  })
  return asList(data)
}

export async function getOccupiedSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetOccupiedSeatsByScreeningResponse[]>(
    `/api/seat/${screeningId}/occupied-seats`,
    {
      method: "GET",
      auth: false,
    }
  )
  return asList(data)
}

export async function getAvailableSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetAvailableSeatsByScreeningResponse[]>(
    `/api/seat/${screeningId}/available-seats`,
    {
      method: "GET",
      auth: false,
    }
  )
  return asList(data)
}
