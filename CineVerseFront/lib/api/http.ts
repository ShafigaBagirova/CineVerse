import { ApiError, type BaseResponse } from "@/lib/api/types"

const ACCESS_TOKEN_KEY = "cineverse.accessToken"
const REFRESH_TOKEN_KEY = "cineverse.refreshToken"
const ACCESS_EXPIRES_AT_KEY = "cineverse.accessExpiresAtUtc"

const API_BASE_URL = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "").replace(/\/$/, "")

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown
  auth?: boolean
  retryOn401?: boolean
  rawResponse?: boolean
  /** When true, failed responses do not log to console (for non-critical background fetches). */
  quiet?: boolean
}

export type ParsedHttpBody =
  | { kind: "empty" }
  | { kind: "json"; value: unknown }
  | { kind: "text"; value: string }

/**
 * Maps raw model-binding / JSON parser messages to safe UI copy. Logs originals in development.
 */
export function sanitizeClientErrorMessage(raw: string): string {
  const t = raw.trim()
  if (!t) return "Something went wrong. Please try again."

  const technical =
    /\$\.|could not be converted|LineNumber|BytePositionInLine|The JSON value/i.test(t)

  if (technical && /gender/i.test(t)) {
    if (typeof process !== "undefined" && process.env.NODE_ENV === "development") {
      console.warn("[api] sanitized user-facing error (gender):", raw)
    }
    return "Please select a valid gender."
  }

  if (technical) {
    if (typeof process !== "undefined" && process.env.NODE_ENV === "development") {
      console.warn("[api] sanitized user-facing error:", raw)
    }
    return "Please check your information and try again."
  }

  return t
}

/**
 * Reads raw text first, then parses JSON when possible.
 * Non-JSON bodies (e.g. plain text or HTML error pages) return `text` kind.
 */
export function parseHttpResponseBody(rawText: string): ParsedHttpBody {
  const trimmed = rawText.trim()
  if (trimmed.length === 0) {
    return { kind: "empty" }
  }
  try {
    return { kind: "json", value: JSON.parse(trimmed) as unknown }
  } catch {
    return { kind: "text", value: trimmed }
  }
}

/**
 * Reads `BaseResponse`-style JSON from the API. Controllers use camelCase (`message`, `errors`);
 * some middleware serializes with System.Text.Json defaults (PascalCase `Message`, `Errors`).
 * Also handles `null`, `{}`, and primitive JSON values.
 */
export function extractBackendFailureMessage(data: unknown, httpStatus: number): string {
  const fallback = `Request failed with status ${httpStatus}`
  if (data === null || data === undefined) {
    return fallback
  }
  if (typeof data === "string") {
    const t = data.trim()
    return t.length > 0 ? sanitizeClientErrorMessage(t) : fallback
  }
  if (typeof data !== "object") {
    return fallback
  }
  if (Array.isArray(data)) {
    return fallback
  }
  const o = data as Record<string, unknown>
  if (Object.keys(o).length === 0) {
    return fallback
  }
  const primary =
    (typeof o.message === "string" && o.message.trim()) ||
    (typeof o.Message === "string" && o.Message.trim()) ||
    ""
  const rawErrs = o.errors ?? o.Errors
  const errLines = Array.isArray(rawErrs)
    ? rawErrs.filter((e): e is string => typeof e === "string" && e.length > 0)
    : []
  if (errLines.length > 0) {
    const joined = errLines.map(sanitizeClientErrorMessage).join(" | ")
    const safePrimary = primary ? sanitizeClientErrorMessage(primary) : ""
    return safePrimary ? `${safePrimary} ${joined}`.trim() : joined
  }
  return primary ? sanitizeClientErrorMessage(primary) : sanitizeClientErrorMessage(fallback)
}

export function extractBackendErrorsArray(data: unknown): string[] | undefined {
  if (data === null || typeof data !== "object") return undefined
  const o = data as Record<string, unknown>
  const rawErrs = o.errors ?? o.Errors
  if (!Array.isArray(rawErrs)) return undefined
  const errLines = rawErrs
    .filter((e): e is string => typeof e === "string")
    .map(sanitizeClientErrorMessage)
  return errLines.length > 0 ? errLines : undefined
}

function isFailureEnvelope(payload: unknown): boolean {
  if (payload === null || typeof payload !== "object") return false
  const p = payload as Record<string, unknown>
  if (typeof p.success === "boolean") return !p.success
  if (typeof p.Success === "boolean") return !p.Success
  return false
}

type ApiRequestFailureMeta = {
  path: string
  method: string
  requestBody?: unknown
}

function logApiFailure(
  quiet: boolean,
  status: number,
  message: string,
  parsed: ParsedHttpBody,
  structuredPayload: unknown,
  rawResponseText: string | undefined,
  meta?: ApiRequestFailureMeta
): void {
  if (quiet) return
  const ctx = meta ?? {}
  if (parsed.kind === "empty") {
    console.error("API ERROR:", {
      ...ctx,
      status,
      message,
      parsedKind: parsed.kind,
      rawText: rawResponseText ?? "",
      body: "(empty)",
    })
    return
  }
  if (parsed.kind === "text") {
    console.error("API ERROR:", {
      ...ctx,
      status,
      message,
      parsedKind: parsed.kind,
      rawText: rawResponseText ?? "",
      body: parsed.value,
    })
    return
  }
  console.error("API ERROR:", {
    ...ctx,
    status,
    message,
    parsedKind: parsed.kind,
    rawText: rawResponseText ?? "",
    parsedJson: structuredPayload,
  })
}

function buildApiErrorMessage(parsed: ParsedHttpBody, httpStatus: number): string {
  if (parsed.kind === "text") {
    return sanitizeClientErrorMessage(parsed.value)
  }
  if (parsed.kind === "empty") {
    return extractBackendFailureMessage(null, httpStatus)
  }
  const v = parsed.value
  if (typeof v === "string") {
    return sanitizeClientErrorMessage(v.trim() || extractBackendFailureMessage(null, httpStatus))
  }
  if (v !== null && typeof v === "object" && !Array.isArray(v)) {
    return extractBackendFailureMessage(v, httpStatus)
  }
  return extractBackendFailureMessage(v, httpStatus)
}

function buildUrl(path: string) {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`

  if (typeof window !== "undefined") {
    return normalizedPath
  }

  if (!API_BASE_URL) {
    throw new ApiError(
      "Missing NEXT_PUBLIC_API_BASE_URL. Set it to your backend API base URL (used for server-side requests).",
      500
    )
  }
  return `${API_BASE_URL}${normalizedPath}`
}

function readTokens() {
  if (typeof window === "undefined") return null
  const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY)
  const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
  const accessExpiresAtUtc = localStorage.getItem(ACCESS_EXPIRES_AT_KEY)
  return { accessToken, refreshToken, accessExpiresAtUtc }
}

function writeTokens(tokens: { accessToken: string; refreshToken: string; expiresAtUtc: string }) {
  if (typeof window === "undefined") return
  localStorage.setItem(ACCESS_TOKEN_KEY, tokens.accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, tokens.refreshToken)
  localStorage.setItem(ACCESS_EXPIRES_AT_KEY, tokens.expiresAtUtc)
}

export function clearTokens() {
  if (typeof window === "undefined") return
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(ACCESS_EXPIRES_AT_KEY)
}

export function getAccessToken() {
  return readTokens()?.accessToken ?? null
}

async function refreshAccessToken() {
  const refreshToken = readTokens()?.refreshToken
  if (!refreshToken) return false

  const response = await fetch(buildUrl("/api/auth/refresh"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  })

  const rawText = await response.text()
  const parsed = parseHttpResponseBody(rawText)

  let payload: BaseResponse<{ accessToken: string; refreshToken: string; expiresAtUtc: string }> | null = null
  if (parsed.kind === "json" && parsed.value !== null && typeof parsed.value === "object") {
    payload = parsed.value as BaseResponse<{
      accessToken: string
      refreshToken: string
      expiresAtUtc: string
    }>
  }

  if (!response.ok || !payload?.success || !payload.data) {
    clearTokens()
    return false
  }

  writeTokens(payload.data)
  return true
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { body, auth = true, retryOn401 = true, rawResponse = false, quiet = false, headers, ...rest } = options
  const token = auth ? getAccessToken() : null
  const method = (rest.method as string | undefined) ?? "GET"
  const failureMeta: ApiRequestFailureMeta = { path, method, requestBody: body }
  const debugPayment =
    path.includes("/api/payment/") &&
    typeof process !== "undefined" &&
    process.env.NEXT_PUBLIC_DEBUG_PAYMENT === "1"

  if (debugPayment && typeof window !== "undefined") {
    console.info("[payment-flow] request", { path, method, requestBody: body })
  }

  const response = await fetch(buildUrl(path), {
    ...rest,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(headers ?? {}),
    },
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  const rawText = await response.text()
  const parsed = parseHttpResponseBody(rawText)

  if (debugPayment && typeof window !== "undefined") {
    console.info("[payment-flow] response", { path, status: response.status, rawText })
  }

  let payload: BaseResponse<T> | null = null

  if (parsed.kind === "json" && !rawResponse) {
    payload = parsed.value as BaseResponse<T>
  }

  if (response.status === 401 && auth && retryOn401) {
    const refreshed = await refreshAccessToken()
    if (refreshed) {
      return apiRequest<T>(path, { ...options, retryOn401: false, quiet })
    }
    throw new ApiError("Unauthorized", response.status, undefined, {
      path,
      method,
      rawText,
      parsedJson: parsed.kind === "json" ? parsed.value : undefined,
    })
  }

  if (!response.ok) {
    const msg = buildApiErrorMessage(parsed, response.status)
    const structured = parsed.kind === "json" ? parsed.value : null
    logApiFailure(quiet, response.status, msg, parsed, structured, rawText, failureMeta)
    const errs =
      structured !== null && typeof structured === "object" && !Array.isArray(structured)
        ? extractBackendErrorsArray(structured)
        : undefined
    throw new ApiError(msg, response.status, errs, {
      path,
      method,
      rawText,
      parsedJson: structured ?? undefined,
    })
  }

  if (rawResponse) {
    if (parsed.kind === "empty") {
      return undefined as T
    }
    if (parsed.kind === "text") {
      throw new ApiError(parsed.value, response.status, undefined, {
        path,
        method,
        rawText,
      })
    }
    return parsed.value as T
  }

  if (payload !== null && isFailureEnvelope(payload)) {
    const msg = buildApiErrorMessage(parsed, response.status)
    logApiFailure(quiet, response.status, msg, parsed, payload, rawText, failureMeta)
    throw new ApiError(msg, response.status, extractBackendErrorsArray(payload), {
      path,
      method,
      rawText,
      parsedJson: payload,
    })
  }

  if (parsed.kind === "empty") {
    return undefined as T
  }

  if (parsed.kind === "text") {
    throw new ApiError(`Unexpected non-JSON response (${response.status})`, response.status, undefined, {
      path,
      method,
      rawText,
    })
  }

  const p = payload as unknown
  if (p !== null && typeof p === "object" && !Array.isArray(p)) {
    const o = p as Record<string, unknown>
    // Treat null data as "missing" so callers see the envelope or root DTO, not `null` / `{}` confusion.
    if (o.data !== undefined && o.data !== null) return o.data as T
    if (o.Data !== undefined && o.Data !== null) return o.Data as T
    return p as T
  }
  return p as T
}

export function persistTokens(tokens: { accessToken: string; refreshToken: string; expiresAtUtc: string }) {
  writeTokens(tokens)
}
