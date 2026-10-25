/** Netflix pages where the user is signing in or signing up rather than browsing. */
const LOGIN_PATH = /login|signup|loginhelp/i

/** After signing in, Netflix often lands on its home page, so the room opens its video again a few times. */
export const AUTO_OPEN_MAX_ATTEMPTS = 5
export const AUTO_OPEN_MIN_GAP_MS = 8000

export function isNetflixLoginPath(path: string | null | undefined) {
  return typeof path === "string" && LOGIN_PATH.test(path)
}

/** Whether the room may try to open its video again now; `lastAttemptAt` is 0 before the first try. */
export function canAutoOpen(attempts: number, lastAttemptAt: number, now: number) {
  return attempts < AUTO_OPEN_MAX_ATTEMPTS && now - lastAttemptAt >= AUTO_OPEN_MIN_GAP_MS
}
