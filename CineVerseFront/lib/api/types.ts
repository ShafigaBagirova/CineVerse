export interface BaseResponse<T> {
  message?: string
  success: boolean
  data?: T
  errors?: string[]
  traceId?: string
}

export interface ApiErrorContext {
  path?: string
  method?: string
  rawText?: string
  parsedJson?: unknown
}

export class ApiError extends Error {
  status: number
  errors?: string[]
  path?: string
  method?: string
  rawText?: string
  parsedJson?: unknown

  constructor(message: string, status: number, errors?: string[], context?: ApiErrorContext) {
    super(message)
    this.name = "ApiError"
    this.status = status
    this.errors = errors
    this.path = context?.path
    this.method = context?.method
    this.rawText = context?.rawText
    this.parsedJson = context?.parsedJson
  }
}

/** Prefer server `message`; fall back to structured `errors` when the message is empty. */
export function userFacingApiErrorMessage(e: unknown, fallback: string): string {
  if (e instanceof ApiError) {
    const primary = e.message.trim()
    if (primary) return primary
    const fromList = e.errors?.filter((s) => s.trim()).join(" · ")
    if (fromList) return fromList
    return fallback
  }
  if (e instanceof Error && e.message.trim()) return e.message.trim()
  if (typeof e === "string" && e.trim()) return e.trim()
  return fallback
}
