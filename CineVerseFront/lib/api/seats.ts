import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

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
  HallId?: number
  row: string
  Row?: string
  number: number
  Number?: number
  type: SeatType
  Type?: SeatType
  isActive: boolean
  IsActive?: boolean
  status: SeatHoldStatus
  Status?: SeatHoldStatus
}

export interface GetOccupiedSeatsByScreeningResponse {
  seatId: number
  SeatId?: number
  row: string
  Row?: string
  number: number
  Number?: number
  occupancyType: string
  OccupancyType?: string
}

export interface GetAvailableSeatsByScreeningResponse {
  seatId: number
  SeatId?: number
  row: string
  Row?: string
  number: number
  Number?: number
}

export interface GetAllSeatsRequest {
  hallId?: number
  row?: string
  type?: SeatType
  isActive?: boolean
  pageNumber?: number
  pageSize?: number
}

export interface GetAllSeatsResponse {
  id: number
  hallId: number
  row: string
  number: number
  type: SeatType
  isActive: boolean
}

export interface CreateSeatRequest {
  hallId: number
  row: string
  number: number
  type: SeatType
}

export interface UpdateSeatRequest {
  hallId?: number
  row?: string
  number?: number
  type?: SeatType
  isActive?: boolean
}

function normalizeSeat(item: GetSeatsByScreeningResponse): GetSeatsByScreeningResponse {
  return {
    seatId: Number(item.seatId ?? item.SeatId ?? 0),
    SeatId: Number(item.seatId ?? item.SeatId ?? 0),
    hallId: Number(item.hallId ?? item.HallId ?? 0),
    HallId: Number(item.hallId ?? item.HallId ?? 0),
    row: String(item.row ?? item.Row ?? ""),
    Row: String(item.row ?? item.Row ?? ""),
    number: Number(item.number ?? item.Number ?? 0),
    Number: Number(item.number ?? item.Number ?? 0),
    type: (item.type ?? item.Type ?? "Standard") as SeatType,
    Type: (item.type ?? item.Type ?? "Standard") as SeatType,
    isActive: Boolean(item.isActive ?? item.IsActive ?? false),
    IsActive: Boolean(item.isActive ?? item.IsActive ?? false),
    status: (item.status ?? item.Status ?? "Active") as SeatHoldStatus,
    Status: (item.status ?? item.Status ?? "Active") as SeatHoldStatus,
  }
}

function normalizeOccupiedSeat(
  item: GetOccupiedSeatsByScreeningResponse
): GetOccupiedSeatsByScreeningResponse {
  return {
    seatId: Number(item.seatId ?? item.SeatId ?? 0),
    SeatId: Number(item.seatId ?? item.SeatId ?? 0),
    row: String(item.row ?? item.Row ?? ""),
    Row: String(item.row ?? item.Row ?? ""),
    number: Number(item.number ?? item.Number ?? 0),
    Number: Number(item.number ?? item.Number ?? 0),
    occupancyType: String(item.occupancyType ?? item.OccupancyType ?? ""),
    OccupancyType: String(item.occupancyType ?? item.OccupancyType ?? ""),
  }
}

function normalizeGetAllSeat(item: GetAllSeatsResponse & Record<string, unknown>): GetAllSeatsResponse {
  return {
    id: Number(item.id ?? item.Id ?? 0),
    hallId: Number(item.hallId ?? item.HallId ?? 0),
    row: String(item.row ?? item.Row ?? ""),
    number: Number(item.number ?? item.Number ?? 0),
    type: (item.type ?? item.Type ?? "Standard") as SeatType,
    isActive: Boolean(item.isActive ?? item.IsActive ?? false),
  }
}

function clampSeatPagination(pageNumber?: number, pageSize?: number) {
  const pn = Math.max(1, Math.trunc(Number(pageNumber ?? 1)) || 1)
  const ps = Math.min(100, Math.max(1, Math.trunc(Number(pageSize ?? 10)) || 10))
  return { pageNumber: pn, pageSize: ps }
}

export async function getSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetSeatsByScreeningResponse[]>(`/api/seat/${screeningId}/seats`, {
    method: "GET",
    auth: false,
    cache: "no-store",
    headers: {
      "Cache-Control": "no-cache",
      Pragma: "no-cache",
    },
  })
  return asList(data).map(normalizeSeat)
}

export async function getOccupiedSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetOccupiedSeatsByScreeningResponse[]>(
    `/api/seat/${screeningId}/occupied-seats`,
    {
      method: "GET",
      auth: false,
      cache: "no-store",
      headers: {
        "Cache-Control": "no-cache",
        Pragma: "no-cache",
      },
    }
  )
  return asList(data).map(normalizeOccupiedSeat)
}

export async function getAvailableSeatsByScreening(screeningId: number) {
  const data = await apiRequest<GetAvailableSeatsByScreeningResponse[]>(
    `/api/seat/${screeningId}/available-seats`,
    {
      method: "GET",
      auth: false,
      cache: "no-store",
      headers: {
        "Cache-Control": "no-cache",
        Pragma: "no-cache",
      },
    }
  )
  return asList(data)
}

export async function getAllSeats(query?: GetAllSeatsRequest) {
  const { pageNumber, pageSize } = clampSeatPagination(query?.pageNumber, query?.pageSize)
  const params = new URLSearchParams()
  params.set("pageNumber", String(pageNumber))
  params.set("pageSize", String(pageSize))
  if (query?.hallId != null && Number.isFinite(query.hallId)) params.set("hallId", String(Math.trunc(query.hallId)))
  if (query?.row != null && query.row.trim() !== "") params.set("row", query.row.trim())
  if (query?.type != null && query.type !== "") params.set("type", query.type)
  if (typeof query?.isActive === "boolean") params.set("isActive", query.isActive ? "true" : "false")

  const data = await apiRequest<PaginatedResponse<GetAllSeatsResponse> & { Items?: GetAllSeatsResponse[] }>(
    `/api/seat?${params.toString()}`,
    {
      method: "GET",
      auth: false,
      cache: "no-store",
      headers: {
        "Cache-Control": "no-cache",
        Pragma: "no-cache",
      },
    }
  )
  const raw = Array.isArray(data.items) ? data.items : Array.isArray(data.Items) ? data.Items : []
  return {
    ...data,
    items: raw.map((x) => normalizeGetAllSeat(x as GetAllSeatsResponse & Record<string, unknown>)),
  }
}

export async function createSeat(body: CreateSeatRequest) {
  return apiRequest<unknown>("/api/seat", {
    method: "POST",
    auth: true,
    body,
  })
}

export async function updateSeat(id: number, body: UpdateSeatRequest) {
  return apiRequest<unknown>(`/api/seat/${id}`, {
    method: "PUT",
    auth: true,
    body,
  })
}

export async function deleteSeat(id: number) {
  return apiRequest<unknown>(`/api/seat/${id}`, {
    method: "DELETE",
    auth: true,
  })
}
