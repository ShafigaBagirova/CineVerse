export function isOptionalAbsenceError(
  err: unknown,
  opts?: { allowNoContent?: boolean }
): boolean {
  if (!err) return true

  if (typeof err === "object" && err !== null) {
    const anyErr = err as any
    const status = anyErr.status ?? anyErr.statusCode

    if (status === 404) return true
    if (opts?.allowNoContent && status === 204) return true
    if (status === undefined && anyErr.response?.status === 404) return true
    if (opts?.allowNoContent && anyErr.response?.status === 204) return true
  }

  if (err instanceof Error) {
    const msg = err.message.toLowerCase()

    if (
      msg.includes("status 404") ||
      msg.includes("request failed with status 404") ||
      msg.includes("not found") ||
      msg.includes("no review") ||
      msg.includes("review not found") ||
      msg.includes("user review not found") ||
      msg.includes("not reviewed") ||
      msg.includes("no review yet") ||
      msg.includes("no summary") ||
      msg.includes("no ratings") ||
      msg.includes("no analysis") ||
      msg.includes("no content") ||
      msg.includes("not available")
    ) {
      return true
    }
  }

  if (typeof err === "string") {
    const msg = err.toLowerCase()

    if (
      msg.includes("status 404") ||
      msg.includes("request failed with status 404") ||
      msg.includes("not found") ||
      msg.includes("no review") ||
      msg.includes("review not found") ||
      msg.includes("user review not found") ||
      msg.includes("not reviewed") ||
      msg.includes("no review yet") ||
      msg.includes("no summary") ||
      msg.includes("no ratings") ||
      msg.includes("no analysis") ||
      msg.includes("no content") ||
      msg.includes("no data")
    ) {
      return true
    }
  }

  return false
}
