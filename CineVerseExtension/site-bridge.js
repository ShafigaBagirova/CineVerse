// Runs on the CineVerse site. Relays messages between the page and the extension.
(() => {
  const PAGE_SOURCE = "cineverse-page";
  const EXTENSION_SOURCE = "cineverse-extension";
  const MARKER_ATTRIBUTE = "data-cineverse-bridge";
  const FORWARDED_TYPES = [
    "OPEN",
    "FOCUS",
    "PLAY",
    "PAUSE",
    "SEEK",
    "REQUEST_STATE",
    "ROOM_INFO",
    "CHAT_HISTORY",
    "CHAT_MESSAGE",
    "REACTION",
  ];

  const MAX_STRING_LENGTH = 600;
  const MAX_ITEMS = 100;
  const MAX_KEYS = 20;
  const MAX_DEPTH = 4;
  const BLOCKED_KEYS = ["__proto__", "constructor", "prototype"];

  // The bridge can be injected more than once (on page load, and again when the extension is installed or updated
  // while the page is open). The newest copy writes its token on the page and wins; older copies stand down.
  const token = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
  const root = document.documentElement;
  if (root) root.setAttribute(MARKER_ATTRIBUTE, token);

  function isCurrent() {
    return !root || root.getAttribute(MARKER_ATTRIBUTE) === token;
  }

  // The payload is page-controlled data, so it is reduced to plain, size-limited values before it is forwarded.
  function sanitize(value, depth) {
    if (typeof value === "string") return value.slice(0, MAX_STRING_LENGTH);
    if (typeof value === "number") return Number.isFinite(value) ? value : null;
    if (typeof value === "boolean") return value;
    if (depth >= MAX_DEPTH) return null;

    if (Array.isArray(value)) return value.slice(0, MAX_ITEMS).map((item) => sanitize(item, depth + 1));

    if (value && typeof value === "object") {
      const result = {};
      for (const key of Object.keys(value).slice(0, MAX_KEYS)) {
        if (BLOCKED_KEYS.includes(key)) continue;
        result[key] = sanitize(value[key], depth + 1);
      }
      return result;
    }

    return null;
  }

  function postToPage(payload) {
    window.postMessage({ source: EXTENSION_SOURCE, ...payload }, window.location.origin);
  }

  window.addEventListener("message", (event) => {
    if (!isCurrent()) return;
    if (event.source !== window || event.origin !== window.location.origin) return;

    const data = event.data;
    if (!data || data.source !== PAGE_SOURCE || typeof data.type !== "string") return;

    if (data.type === "PING") {
      postToPage({ type: "READY" });
      return;
    }

    if (!FORWARDED_TYPES.includes(data.type)) return;

    const message = { from: "site", type: data.type };
    if (data.type === "OPEN" && typeof data.url === "string") message.url = data.url;
    if (data.type === "SEEK" && typeof data.seconds === "number" && Number.isFinite(data.seconds)) {
      message.seconds = data.seconds;
    }
    if (data.payload && typeof data.payload === "object") message.payload = sanitize(data.payload, 0);

    try {
      chrome.runtime.sendMessage(message).catch(() => {});
    } catch (error) {
      // The extension was reloaded; the page will detect that on its next check.
    }
  });

  chrome.runtime.onMessage.addListener((message) => {
    if (!isCurrent()) return;
    if (message && message.to === "site" && message.payload) postToPage(message.payload);
  });
})();
