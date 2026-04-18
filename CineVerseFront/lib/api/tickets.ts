import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export type TicketStatus = "Paid" | "Cancelled" | "Refunded"

export interface GetMyTicketsRequest {
  status?: TicketStatus
  purchasedAfterUtc?: string
  purchasedBeforeUtc?: string
  pageNumber?: number
  pageSize?: number
}

export interface GetMyTicketsResponse {
  id: number
  screeningId: number
  movieId: number
  movieTitle: string
  cinemaId: number
  cinemaName: string
  hallId: number
  hallName: string
  seatId: number
  seatRow: string
  seatNumber: number
  screeningStartTime: string
  screeningEndTime?: string | null
  price: number
  status: string
  purchasedAt: string
}

export interface GetTicketByIdResponse extends GetMyTicketsResponse {}

function buildQuery(query: Record<string, string | number | boolean | undefined>) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === "") return
    params.set(key, String(value))
  })
  const queryString = params.toString()
  return queryString ? `?${queryString}` : ""
}

export async function getMyTickets(query: GetMyTicketsRequest = {}) {
  const qs = buildQuery({
    status: query.status,
    purchasedAfterUtc: query.purchasedAfterUtc,
    purchasedBeforeUtc: query.purchasedBeforeUtc,
    pageNumber: query.pageNumber ?? 1,
    pageSize: query.pageSize ?? 30,
  })
  return apiRequest<PaginatedResponse<GetMyTicketsResponse>>(`/api/ticket/my${qs}`, {
    method: "GET",
    auth: true,
  })
}

export async function getTicketById(id: number) {
  return apiRequest<GetTicketByIdResponse>(`/api/ticket/${id}`, {
    method: "GET",
    auth: true,
  })
}
