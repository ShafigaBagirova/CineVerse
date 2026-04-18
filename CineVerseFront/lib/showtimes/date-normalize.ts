/** Local calendar day key YYYY-MM-DD (not UTC) for grouping/filtering. */
export function toLocalDateKey(d: Date): string {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, "0")
  const day = String(d.getDate()).padStart(2, "0")
  return `${y}-${m}-${day}`
}

/** Parse API start time (ISO string) to local date key; invalid → null. */
export function parseStartTimeToLocalDateKey(iso: string): string | null {
  if (!iso || typeof iso !== "string") return null
  const t = Date.parse(iso)
  if (Number.isNaN(t)) return null
  return toLocalDateKey(new Date(t))
}

/** Fixed locale so SSR and browser produce identical strings (avoids hydration mismatch). */
const DISPLAY_LOCALE = "en-US"

/** Display label for a date key (UI only; not used for filtering). */
export function formatLocalDateLong(dateKey: string): string {
  const parts = dateKey.split("-").map(Number)
  const y = parts[0]
  const mo = parts[1]
  const d = parts[2]
  if (!y || !mo || !d) return dateKey
  const date = new Date(y, mo - 1, d)
  return new Intl.DateTimeFormat(DISPLAY_LOCALE, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  }).format(date)
}

/** Short date for inline showtime rows (UI only). */
export function formatLocalDateShort(dateKey: string): string {
  const parts = dateKey.split("-").map(Number)
  const y = parts[0]
  const mo = parts[1]
  const d = parts[2]
  if (!y || !mo || !d) return dateKey
  const date = new Date(y, mo - 1, d)
  return new Intl.DateTimeFormat(DISPLAY_LOCALE, {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(date)
}
