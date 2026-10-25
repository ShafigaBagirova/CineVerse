const PAGE_SOURCE = "cineverse-page"
const EXTENSION_SOURCE = "cineverse-extension"

/** Messages the CineVerse browser extension posts to this page. */
export type ExtensionMessage =
  | { type: "READY" }
  | {
      type: "TAB_STATE"
      open: boolean
      watchId: string | null
      /** Address path of the Netflix page, for example "/login" or "/browse". */
      path?: string | null
      videoReady: boolean
      playing: boolean
      time: number
    }
  | { type: "EVENT"; action: "play" | "pause" | "seek"; time: number; playing: boolean }
  | { type: "TIME"; time: number; playing: boolean }
  | { type: "TAB_CLOSED" }
  | { type: "COMMAND_ERROR"; command: string; message: string }
  | { type: "OVERLAY_READY" }
  | { type: "SEND_CHAT"; content: string }
  | { type: "SEND_REACTION"; emoji: string }

/** A chat message as the panel inside the Netflix window shows it. */
export type OverlayChatMessage = {
  senderName: string
  mine: boolean
  content: string
  sentAtUtc: string
}

/** Messages this page sends to the extension. */
export type PageMessage =
  | { type: "PING" }
  | { type: "REQUEST_STATE" }
  | { type: "OPEN"; url: string }
  | { type: "FOCUS" }
  | { type: "PLAY" }
  | { type: "PAUSE" }
  | { type: "SEEK"; seconds: number }
  | { type: "ROOM_INFO"; payload: { title: string; code: string } }
  | { type: "CHAT_HISTORY"; payload: { messages: OverlayChatMessage[] } }
  | { type: "CHAT_MESSAGE"; payload: OverlayChatMessage }
  | { type: "REACTION"; payload: { emoji: string; senderName: string } }

export function sendToExtension(message: PageMessage) {
  if (typeof window === "undefined") return
  window.postMessage({ source: PAGE_SOURCE, ...message }, window.location.origin)
}

export function listenToExtension(listener: (message: ExtensionMessage) => void) {
  const handler = (event: MessageEvent) => {
    if (event.source !== window || event.origin !== window.location.origin) return
    const data = event.data as { source?: string } | null
    if (!data || data.source !== EXTENSION_SOURCE) return
    listener(data as unknown as ExtensionMessage)
  }

  window.addEventListener("message", handler)
  return () => window.removeEventListener("message", handler)
}
