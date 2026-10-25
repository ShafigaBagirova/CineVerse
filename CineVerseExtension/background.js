// Routes messages between the CineVerse room page and the paired Netflix window.
// The service worker can be stopped at any time, so the pairing lives in storage.session.

const PAIRING_KEY = "pairing";
const SITE_ORIGINS = ["http://localhost:3000/", "http://127.0.0.1:3000/"];
const SITE_MATCHES = ["http://localhost:3000/*", "http://127.0.0.1:3000/*"];
const NETFLIX_ORIGIN = "https://www.netflix.com/";
const NETFLIX_WATCH_URL = /^https:\/\/www\.netflix\.com\/watch\/(\d{3,12})(?:[/?#]|$)/;
const NETFLIX_HOME_URLS = ["https://www.netflix.com", "https://www.netflix.com/browse"];

// Share of the room window's width that the Netflix window takes when the two are placed side by side.
const NETFLIX_WIDTH_SHARE = 0.6;
// Browsers do not shrink a window below roughly this width, so the room part never gets less than it.
const MIN_SITE_WIDTH = 520;
const MIN_NETFLIX_WIDTH = 480;
const MIN_ARRANGE_HEIGHT = 400;

// Messages that only feed the chat panel inside the Netflix window. They are dropped quietly when no window is open.
const OVERLAY_TYPES = ["ROOM_INFO", "CHAT_HISTORY", "CHAT_MESSAGE", "REACTION"];

async function getPairing() {
  const stored = await chrome.storage.session.get(PAIRING_KEY);
  return stored[PAIRING_KEY] || {};
}

async function setPairing(pairing) {
  await chrome.storage.session.set({ [PAIRING_KEY]: pairing });
}

function sendToSite(siteTabId, payload) {
  if (siteTabId === undefined) return Promise.resolve();
  return chrome.tabs.sendMessage(siteTabId, { to: "site", payload }).catch(() => {});
}

async function getTab(tabId) {
  if (tabId === undefined) return null;
  try {
    return await chrome.tabs.get(tabId);
  } catch (error) {
    return null;
  }
}

function emptyTabState(open) {
  return { type: "TAB_STATE", open, watchId: null, videoReady: false, playing: false, time: 0 };
}

// Only a specific video or Netflix's own home/browse page can be opened, never an arbitrary address.
function parseOpenUrl(url) {
  if (typeof url !== "string") return null;

  const watch = url.match(NETFLIX_WATCH_URL);
  if (watch) return { url, watchId: watch[1] };

  const bare = url.replace(/\/+$/, "");
  if (NETFLIX_HOME_URLS.includes(bare)) return { url, watchId: null };

  return null;
}

// Splits the room window's area: the room keeps the left part, Netflix gets the right part.
function sideBySideLayout(siteWindow) {
  const { left, top, width, height } = siteWindow;
  const values = [left, top, width, height];
  if (values.some((value) => typeof value !== "number" || !Number.isFinite(value))) return null;
  if (width < MIN_SITE_WIDTH + MIN_NETFLIX_WIDTH || height < MIN_ARRANGE_HEIGHT) return null;

  const siteWidth = Math.max(Math.round(width * (1 - NETFLIX_WIDTH_SHARE)), MIN_SITE_WIDTH);
  const netflixWidth = width - siteWidth;

  return {
    site: { left, top, width: siteWidth, height },
    netflix: { left: left + siteWidth, top, width: netflixWidth, height },
  };
}

async function getSideBySideLayout(siteWindowId) {
  if (siteWindowId === undefined) return null;

  try {
    const siteWindow = await chrome.windows.get(siteWindowId);
    if (siteWindow.state === "minimized" || siteWindow.state === "fullscreen") return null;
    return sideBySideLayout(siteWindow);
  } catch (error) {
    return null;
  }
}

// Arranging is a convenience; a failure here must never stop Netflix from opening.
async function arrangeSiteWindow(siteWindowId, bounds) {
  try {
    const current = await chrome.windows.get(siteWindowId);
    if (current.state !== "normal") await chrome.windows.update(siteWindowId, { state: "normal" });
    await chrome.windows.update(siteWindowId, bounds);
  } catch (error) {
    // ignore
  }
}

// Chrome rejects bounds that fall outside the visible screen, so the window is created with a fallback chain:
// compact window with the computed bounds, then a compact window anywhere, then a normal window.
async function createNetflixWindow(url, bounds) {
  const attempts = [];
  if (bounds) attempts.push({ options: { url, type: "popup", focused: true, ...bounds }, placed: true });
  attempts.push({ options: { url, type: "popup", focused: true }, placed: false });
  attempts.push({ options: { url, focused: true }, placed: false });

  let lastError = null;
  for (const attempt of attempts) {
    try {
      return { created: await chrome.windows.create(attempt.options), placed: attempt.placed };
    } catch (error) {
      lastError = error;
    }
  }

  throw lastError;
}

async function openNetflix(rawUrl, pairing, siteWindowId) {
  const target = parseOpenUrl(rawUrl);
  if (!target) return;
  const url = target.url;

  const existing = await getTab(pairing.netflixTabId);
  if (existing) {
    const currentUrl = typeof existing.url === "string" ? existing.url : "";
    const onWatchPage = /\/watch\/\d+/.test(currentUrl);
    // A specific video is already open if the address matches; for the browse page, anything that is not a video is fine.
    const alreadyThere = target.watchId ? currentUrl.includes(`/watch/${target.watchId}`) : !onWatchPage;
    await chrome.tabs.update(existing.id, alreadyThere ? { active: true } : { url, active: true });
    await chrome.windows.update(existing.windowId, { focused: true });
    return;
  }

  // A compact window (no tab strip or address bar) placed beside the room, so both fit on one screen.
  const layout = await getSideBySideLayout(siteWindowId);
  const { created, placed } = await createNetflixWindow(url, layout ? layout.netflix : null);
  const tab = created.tabs && created.tabs[0];
  if (tab) await setPairing({ ...pairing, netflixTabId: tab.id });

  // The room window only makes room if Netflix really took its place on the right.
  if (layout && placed) await arrangeSiteWindow(siteWindowId, layout.site);
}

// Brings the paired Netflix window to the front without navigating it (for example while the user signs in).
async function focusNetflix(pairing) {
  const tab = await getTab(pairing.netflixTabId);
  if (!tab) return;

  await chrome.tabs.update(tab.id, { active: true });
  await chrome.windows.update(tab.windowId, { focused: true });
}

async function handleFromSite(message, siteTabId, siteWindowId, pairing) {
  if (pairing.siteTabId !== siteTabId) {
    pairing = { ...pairing, siteTabId };
    await setPairing(pairing);
  }

  if (message.type === "OPEN") {
    await openNetflix(message.url, pairing, siteWindowId);
    return;
  }

  if (message.type === "FOCUS") {
    await focusNetflix(pairing);
    return;
  }

  const netflixTab = await getTab(pairing.netflixTabId);
  if (!netflixTab) {
    if (pairing.netflixTabId !== undefined) await setPairing({ ...pairing, netflixTabId: undefined });
    if (message.type === "REQUEST_STATE") await sendToSite(siteTabId, emptyTabState(false));
    else if (!OVERLAY_TYPES.includes(message.type)) await sendToSite(siteTabId, { type: "TAB_CLOSED" });
    return;
  }

  try {
    await chrome.tabs.sendMessage(netflixTab.id, {
      to: "netflix",
      type: message.type,
      seconds: message.seconds,
      payload: message.payload,
    });
  } catch (error) {
    // The window exists but its content script is not ready yet (page still loading).
    if (message.type === "REQUEST_STATE") await sendToSite(siteTabId, emptyTabState(true));
  }
}

async function handleFromNetflix(message, netflixTabId, pairing) {
  if (pairing.netflixTabId !== netflixTabId || !message.payload) return;
  await sendToSite(pairing.siteTabId, message.payload);
}

function startsWithAny(value, prefixes) {
  return typeof value === "string" && prefixes.some((prefix) => value.startsWith(prefix));
}

// Messages are handled one at a time so two quick messages cannot overwrite each other's pairing update.
let queue = Promise.resolve();

function enqueue(task) {
  const run = queue.then(task, task);
  queue = run.catch(() => {});
  return run;
}

async function handleMessage(message, sender) {
  const tabId = sender.tab ? sender.tab.id : undefined;
  if (tabId === undefined || !message) return;

  const pairing = await getPairing();

  if (message.from === "site" && startsWithAny(sender.url, SITE_ORIGINS)) {
    await handleFromSite(message, tabId, sender.tab.windowId, pairing);
  } else if (message.from === "netflix" && startsWithAny(sender.url, [NETFLIX_ORIGIN])) {
    await handleFromNetflix(message, tabId, pairing);
  }
}

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  enqueue(() => handleMessage(message, sender)).then(
    () => sendResponse({ ok: true }),
    () => sendResponse({ ok: false })
  );
  return true;
});

// A page that was already open when the extension was installed or updated has no bridge yet, because a page only
// gets its content scripts when it loads. Injecting the bridge here means nobody has to reload the room page.
async function injectIntoOpenSiteTabs() {
  let tabs = [];
  try {
    tabs = await chrome.tabs.query({ url: SITE_MATCHES });
  } catch (error) {
    return;
  }

  for (const tab of tabs) {
    if (tab.id === undefined) continue;

    try {
      await chrome.scripting.executeScript({ target: { tabId: tab.id }, files: ["site-bridge.js"] });
    } catch (error) {
      // This tab cannot be injected (for example it is being discarded); the others still get the bridge.
    }
  }
}

chrome.runtime.onInstalled.addListener(() => enqueue(injectIntoOpenSiteTabs));

chrome.tabs.onRemoved.addListener((tabId) => enqueue(async () => {
  const pairing = await getPairing();

  if (pairing.netflixTabId === tabId) {
    await setPairing({ ...pairing, netflixTabId: undefined });
    await sendToSite(pairing.siteTabId, { type: "TAB_CLOSED" });
  } else if (pairing.siteTabId === tabId) {
    await setPairing({ ...pairing, siteTabId: undefined });
  }
}));
