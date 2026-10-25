const configured = (process.env.NEXT_PUBLIC_EXTENSION_URL ?? "").trim()

/**
 * Where people get the CineVerse extension (its Chrome Web Store or Edge Add-ons page). Set
 * NEXT_PUBLIC_EXTENSION_URL once it is published; while it is empty no store button is shown.
 */
export const EXTENSION_STORE_URL = /^https:\/\//i.test(configured) ? configured : ""
