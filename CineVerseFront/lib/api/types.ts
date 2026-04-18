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
