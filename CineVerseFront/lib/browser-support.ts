export type NetflixSupport = "supported" | "mobile" | "unsupported-browser"

/**
 * Netflix rooms need the CineVerse extension, which exists for Chromium based desktop browsers (Chrome, Edge,
 * Brave, Opera). Firefox, Safari and phones cannot use it.
 */
export function getNetflixSupport(userAgent: string, maxTouchPoints = 0): NetflixSupport {
  const ua = userAgent || ""

  // iPadOS asks for the desktop site and reports itself as a Mac, but a Mac has no multi-touch screen.
  const touchMac = /Macintosh/i.test(ua) && maxTouchPoints > 1
  if (/Android|iPhone|iPad|iPod|Mobile/i.test(ua) || touchMac) return "mobile"

  if (/Firefox\/|FxiOS/i.test(ua)) return "unsupported-browser"
  if (/Chrome\//.test(ua)) return "supported"

  return "unsupported-browser"
}
