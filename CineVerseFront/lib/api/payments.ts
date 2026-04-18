import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"
import { ApiError } from "@/lib/api/types"

export type PaymentStatus = "Pending" | "Succeeded" | "Failed" | "Cancelled" | "Refunded"
export type PaymentProvider = "Stripe" | string

export interface CreatePaymentIntentRequest {
  seatHoldId: number
}

export interface CreatePaymentIntentResponse {
  paymentId: number
  seatHoldId: number
  ticketAmount: number
  foodAmount: number
  totalAmount: number
  clientSecret: string
  providerPaymentIntentId: string
  status: PaymentStatus
}

export interface RetryPaymentResponse {
  paymentId: number
  seatHoldId: number
  provider: PaymentProvider
  providerPaymentIntentId: string
  clientSecret: string
  totalAmount: number
  ticketAmount: number
  foodAmount: number
  currency: string
  status: PaymentStatus
}

export interface GetPaymentStatusBySeatHoldIdResponse {
  seatHoldId: number
  hasPayment: boolean
  status: PaymentStatus
  amount?: number | null
  currency?: string | null
  paymentIntentId?: string | null
  paidAtUtc?: string | null
}

export interface GetMyPaymentsRequest {
  pageNumber?: number
  pageSize?: number
  status?: PaymentStatus
}

export interface GetMyPaymentsResponse {
  id: number
  seatHoldId: number
  screeningId: number
  seatId: number
  amount: number
  currency: string
  provider: PaymentProvider
  status: PaymentStatus
  providerPaymentIntentId: string
  createdAtUtc: string
  paidAtUtc?: string | null
}

function buildQuery(query: Record<string, string | number | boolean | undefined>) {
  const params = new URLSearchParams()
  Object.entries(query).forEach(([key, value]) => {
    if (value === undefined || value === "") return
    params.set(key, String(value))
  })
  const queryString = params.toString()
  return queryString ? `?${queryString}` : ""
}

export async function createPaymentIntent(request: CreatePaymentIntentRequest) {
  const seatHoldId = Number(request.seatHoldId)
  if (!Number.isFinite(seatHoldId) || seatHoldId <= 0) {
    throw new ApiError("seatHoldId must be a positive number", 400)
  }
  return apiRequest<CreatePaymentIntentResponse>("/api/payment/create-intent", {
    method: "POST",
    auth: true,
    body: { seatHoldId },
  })
}

export async function retryPayment(seatHoldId: number) {
  return apiRequest<RetryPaymentResponse>(`/api/payment/retry/${seatHoldId}`, {
    method: "POST",
    auth: true,
  })
}

export async function getPaymentStatusBySeatHoldId(seatHoldId: number) {
  return apiRequest<GetPaymentStatusBySeatHoldIdResponse>(`/api/payment/seat-hold/${seatHoldId}/status`, {
    method: "GET",
    auth: true,
  })
}

export async function getMyPayments(query: GetMyPaymentsRequest = {}) {
  const qs = buildQuery({
    pageNumber: query.pageNumber ?? 1,
    pageSize: query.pageSize ?? 10,
    status: query.status,
  })
  return apiRequest<PaginatedResponse<GetMyPaymentsResponse>>(`/api/payment/my${qs}`, {
    method: "GET",
    auth: true,
  })
}

export interface GetAllPaymentsRequest {
  pageNumber?: number
  pageSize?: number
  status?: PaymentStatus
  provider?: PaymentProvider
  userId?: string
  fromDateUtc?: string
  toDateUtc?: string
}

/** Normalized row from GET /api/payment list; optional fields stay unset when absent on the wire. */
export interface GetAllPaymentsResponse {
  id?: number
  seatHoldId?: number
  userId?: string
  amount?: number
  currency?: string
  status?: PaymentStatus
  provider?: PaymentProvider
  providerPaymentIntentId?: string | null
  createdAtUtc?: string
  paidAtUtc?: string | null
  refundedAtUtc?: string | null
}

function pickFiniteNumber(...candidates: unknown[]): number | undefined {
  for (const v of candidates) {
    if (typeof v === "number" && Number.isFinite(v)) return v
    if (typeof v === "string" && v.trim() !== "") {
      const n = Number(v)
      if (Number.isFinite(n)) return n
    }
  }
  return undefined
}

function pickString(...candidates: unknown[]): string {
  for (const v of candidates) {
    if (typeof v === "string" && v.length > 0) return v
  }
  return ""
}

function toIsoDateString(v: unknown): string | null {
  if (v == null) return null
  if (typeof v === "string" && v.length > 0) return v
  if (v instanceof Date && !Number.isNaN(v.getTime())) return v.toISOString()
  return null
}

function isPlausibleUtc(iso: string): boolean {
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return false
  return d.getFullYear() >= 1970
}

/** Prefer `amount` / `totalAmount`; otherwise sum ticket + food when those fields exist (matches payment entity shape). */
function resolvePaymentAmount(o: Record<string, unknown>): number | undefined {
  const direct = pickFiniteNumber(o.amount, o.Amount, o.totalAmount, o.TotalAmount)
  if (direct !== undefined) return direct
  const ticket = pickFiniteNumber(o.ticketAmount, o.TicketAmount)
  const food = pickFiniteNumber(o.foodAmount, o.FoodAmount)
  if (ticket !== undefined && food !== undefined) return ticket + food
  if (ticket !== undefined) return ticket
  if (food !== undefined) return food
  return undefined
}

function optionalTrimmedString(v: unknown): string | undefined {
  if (typeof v !== "string") return undefined
  const t = v.trim()
  return t.length > 0 ? t : undefined
}

/**
 * Maps one GET /api/payment row to `GetAllPaymentsResponse`, tolerating camelCase/PascalCase
 * and common amount fields (`amount`, `totalAmount`, `ticketAmount` + `foodAmount`).
 */
export function normalizeGetAllPaymentsItem(raw: unknown): GetAllPaymentsResponse {
  const o = raw !== null && typeof raw === "object" ? (raw as Record<string, unknown>) : {}

  const idRaw = pickFiniteNumber(o.id, o.Id)
  const id = idRaw !== undefined ? Math.trunc(idRaw) : undefined

  const seatRaw = pickFiniteNumber(o.seatHoldId, o.SeatHoldId)
  const seatHoldId = seatRaw !== undefined ? Math.trunc(seatRaw) : undefined

  const amount = resolvePaymentAmount(o)

  const currency =
    optionalTrimmedString(o.currency) ??
    optionalTrimmedString(o.Currency)

  const statusRaw =
    optionalTrimmedString(o.status) ?? optionalTrimmedString(o.Status)
  const status = statusRaw as PaymentStatus | undefined

  const providerRaw =
    optionalTrimmedString(o.provider) ?? optionalTrimmedString(o.Provider)
  const provider = providerRaw as PaymentProvider | undefined

  const userId =
    optionalTrimmedString(o.userId) ?? optionalTrimmedString(o.UserId)

  const providerPaymentIntentId =
    (typeof o.providerPaymentIntentId === "string" ? o.providerPaymentIntentId : null) ??
    (typeof o.ProviderPaymentIntentId === "string" ? o.ProviderPaymentIntentId : null)

  const createdRaw =
    toIsoDateString(o.createdAtUtc ?? o.CreatedAtUtc ?? o.createdAt ?? o.CreatedAt) ?? undefined
  let createdAtUtc: string | undefined =
    createdRaw && isPlausibleUtc(createdRaw) ? createdRaw : undefined

  const paidAtUtc =
    toIsoDateString(o.paidAtUtc ?? o.PaidAtUtc) ??
    optionalTrimmedString(o.paidAtUtc) ??
    optionalTrimmedString(o.PaidAtUtc) ??
    undefined

  const refundedAtUtc =
    toIsoDateString(o.refundedAtUtc ?? o.RefundedAtUtc) ??
    optionalTrimmedString(o.refundedAtUtc) ??
    optionalTrimmedString(o.RefundedAtUtc) ??
    undefined

  const out: GetAllPaymentsResponse = {}
  if (id !== undefined) out.id = id
  if (seatHoldId !== undefined) out.seatHoldId = seatHoldId
  if (userId !== undefined) out.userId = userId
  if (amount !== undefined) out.amount = amount
  if (currency !== undefined) out.currency = currency
  if (status !== undefined) out.status = status
  if (provider !== undefined) out.provider = provider
  if (providerPaymentIntentId != null) out.providerPaymentIntentId = providerPaymentIntentId
  if (createdAtUtc !== undefined) out.createdAtUtc = createdAtUtc
  if (paidAtUtc !== undefined) out.paidAtUtc = paidAtUtc
  if (refundedAtUtc !== undefined) out.refundedAtUtc = refundedAtUtc
  return out
}

export type GetAllPaymentsOptions = { quiet?: boolean }

export async function getAllPayments(query: GetAllPaymentsRequest = {}, options?: GetAllPaymentsOptions) {
  const qs = buildQuery({
    pageNumber: query.pageNumber ?? 1,
    pageSize: Math.min(100, Math.max(1, query.pageSize ?? 10)),
    status: query.status,
    provider: query.provider,
    userId: query.userId,
    fromDateUtc: query.fromDateUtc,
    toDateUtc: query.toDateUtc,
  })
  const data = await apiRequest<PaginatedResponse<unknown>>(`/api/payment${qs}`, {
    method: "GET",
    auth: true,
    quiet: options?.quiet,
  })
  const rawItems = data.items ?? (data as unknown as { Items?: unknown[] }).Items ?? []
  const items = Array.isArray(rawItems) ? rawItems.map(normalizeGetAllPaymentsItem) : []
  return {
    ...data,
    items,
  } as PaginatedResponse<GetAllPaymentsResponse>
}

export interface GetPaymentByIdResponse {
  id: number
  seatHoldId: number
  provider: PaymentProvider
  status: PaymentStatus
  amount: number
  currency: string
  providerPaymentIntentId?: string | null
  createdAtUtc: string
  paidAtUtc?: string | null
}

export async function getPaymentById(id: number) {
  return apiRequest<GetPaymentByIdResponse>(`/api/payment/${id}`, {
    method: "GET",
    auth: true,
  })
}
