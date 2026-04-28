import { apiRequest } from "@/lib/api/http"
import type { PaginatedResponse } from "@/lib/api/movies"

export type SeatHoldStatus = "Active" | "Released" | "Expired" | "Purchased"

export interface CreateSeatHoldRequest {
  screeningId: number
  seatId: number
}

/** Normalized seat hold returned to UI after POST create (from body or follow-up GET). */
export interface CreateSeatHoldResponse {
  id: number
  screeningId: number
  seatId: number
  expiresAtUtc: string
}

/** POST /api/seathold often returns non-generic `BaseResponse` (success + message only, no `data`). */
function parseCreateSeatHoldDto(raw: unknown): CreateSeatHoldResponse | null {
  if (raw == null || typeof raw !== "object") return null
  const o = raw as Record<string, unknown>

  const fromObj = (x: Record<string, unknown>): CreateSeatHoldResponse | null => {
    const id = x.id ?? x.Id
    const screeningId = x.screeningId ?? x.ScreeningId
    const seatId = x.seatId ?? x.SeatId
    const expiresRaw = x.expiresAtUtc ?? x.ExpiresAtUtc
    if (typeof id !== "number" || typeof screeningId !== "number" || typeof seatId !== "number") {
      return null
    }
    let expiresAtUtc: string
    if (typeof expiresRaw === "string") expiresAtUtc = expiresRaw
    else if (typeof expiresRaw === "number" && Number.isFinite(expiresRaw)) {
      expiresAtUtc = new Date(expiresRaw).toISOString()
    } else {
      return null
    }
    return { id, screeningId, seatId, expiresAtUtc }
  }

  const direct = fromObj(o)
  if (direct) return direct

  const nested = o.data ?? o.Data
  if (nested != null && typeof nested === "object" && !Array.isArray(nested)) {
    return fromObj(nested as Record<string, unknown>)
  }
  return null
}

export interface GetSeatHoldsByScreeningResponse {
  id: number
  screeningId: number
  seatId: number
  userId: string
  expiresAtUtc: string
  status: SeatHoldStatus
}

export interface GetSeatHoldByIdResponse {
  id: number
  screeningId: number
  seatId: number
  userId: string
  expiresAtUtc: string
  status: SeatHoldStatus
  isActive?: boolean
}

function normalizeSeatHold(raw: unknown): GetSeatHoldByIdResponse {
  const o = raw !== null && typeof raw === "object" ? (raw as Record<string, unknown>) : {}
  const isActiveRaw = o.isActive ?? o.IsActive
  const isActive =
    typeof isActiveRaw === "boolean"
      ? isActiveRaw
      : typeof isActiveRaw === "string"
        ? isActiveRaw.trim().toLowerCase() === "true"
        : undefined
  return {
    id: Number(o.id ?? o.Id ?? o.ID ?? 0),
    screeningId: Number(o.screeningId ?? o.ScreeningId ?? 0),
    seatId: Number(o.seatId ?? o.SeatId ?? 0),
    userId: String(o.userId ?? o.UserId ?? ""),
    expiresAtUtc: String(o.expiresAtUtc ?? o.ExpiresAtUtc ?? o.expiresAt ?? o.ExpiresAt ?? ""),
    status: String(o.status ?? o.Status ?? "") as SeatHoldStatus,
    ...(typeof isActive === "boolean" ? { isActive } : {}),
  }
}

/**
 * Creates a seat hold. HTTP success is treated as success even when the body has no DTO:
 * we merge GET list when possible, otherwise return a minimal object keyed by the request `seatId`
 * (callers must not assume `id` is non-zero until refreshed from GET).
 */
export async function createSeatHold(request: CreateSeatHoldRequest): Promise<CreateSeatHoldResponse> {
  let raw: unknown
  try {
    raw = await apiRequest<unknown>("/api/seathold", {
      method: "POST",
      auth: true,
      body: request,
    })
  } catch (e) {
    console.error("[createSeatHold] request failed", { payload: request, error: e })
    throw e
  }

  console.info("[createSeatHold] response", {
    payload: request,
    rawJson: typeof raw === "object" && raw !== null ? JSON.stringify(raw) : String(raw),
  })

  const fromBody = parseCreateSeatHoldDto(raw)
  if (fromBody) return fromBody

  try {
    const page = await getSeatHoldsByScreening(request.screeningId, 1, 100)
    const hold = page.items.find((h) => h.seatId === request.seatId && h.status === "Active")
    if (hold) {
      return {
        id: hold.id,
        screeningId: hold.screeningId,
        seatId: hold.seatId,
        expiresAtUtc: hold.expiresAtUtc,
      }
    }
  } catch (e) {
    console.warn("[createSeatHold] follow-up GET failed; assuming hold from POST success", e)
  }

  return {
    id: 0,
    screeningId: request.screeningId,
    seatId: request.seatId,
    expiresAtUtc: new Date().toISOString(),
  }
}

export async function releaseSeatHold(id: number) {
  return apiRequest<unknown>(`/api/seathold/${id}/release`, {
    method: "PUT",
    auth: true,
  })
}

export async function getSeatHoldById(id: number) {
  const data = await apiRequest<GetSeatHoldByIdResponse>(`/api/seathold/${id}`, {
    method: "GET",
    auth: true,
    cache: "no-store",
    headers: {
      "Cache-Control": "no-cache",
      Pragma: "no-cache",
    },
  })
  return normalizeSeatHold(data)
}

function normalizeSeatHoldPageSize(pageSize?: number): number {
  if (pageSize === undefined || pageSize === null || !Number.isFinite(pageSize)) return 10
  const n = Math.floor(Number(pageSize))
  if (n < 1 || n > 100) return 10
  return n
}

export async function getSeatHoldsByScreening(screeningId: number, pageNumber = 1, pageSize?: number) {
  const ps = normalizeSeatHoldPageSize(pageSize)
  const data = await apiRequest<PaginatedResponse<GetSeatHoldsByScreeningResponse>>(
    `/api/seathold/screenings/${screeningId}?pageNumber=${pageNumber}&pageSize=${ps}`,
    {
      method: "GET",
      auth: true,
      cache: "no-store",
      headers: {
        "Cache-Control": "no-cache",
        Pragma: "no-cache",
      },
    }
  )
  const raw = data as PaginatedResponse<GetSeatHoldsByScreeningResponse> & { Items?: GetSeatHoldsByScreeningResponse[] }
  return {
    ...data,
    items: (data.items ?? raw.Items ?? []).map(normalizeSeatHold),
  }
}
