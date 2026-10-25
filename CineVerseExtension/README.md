# CineVerse Watch Sync (browser extension)

Keeps your Netflix window in sync with a CineVerse watch room. Not affiliated with or endorsed by Netflix.

Netflix cannot be embedded in a web page (Netflix forbids it), so each person watches in their own Netflix window
with their own subscription. The extension relays play, pause and seek between that window and the CineVerse room
page, and shows the room's chat and reactions inside the Netflix window. The room page does the rest (SignalR sync,
chat, reactions), exactly like for YouTube rooms.

## Install (development)

Chrome or Edge, version 111 or newer.

1. Open `chrome://extensions` (Edge: `edge://extensions`).
2. Turn on **Developer mode**.
3. Click **Load unpacked** and choose this `CineVerseExtension` folder (the one that contains `manifest.json`).
4. After changing any file here, click the reload icon on the extension card. A CineVerse page that is already open
   connects to the extension by itself within a few seconds, because the extension adds itself to open CineVerse tabs
   when it is installed or updated. If it does not connect, reload the page.

## Use

Creating a room:

1. In CineVerse go to **Watch**, choose **Netflix** and name the room.
2. Netflix opens in a compact window next to the room. Sign in with your own Netflix account there and find a film
   or episode.
3. Start playing it. CineVerse notices the video and creates the room for it automatically, then takes you into the room.

Joining a room:

1. Join with the room code. The room opens the room's video in a Netflix window for you.
2. Sign in to Netflix there if it asks. After signing in (and choosing a profile if Netflix asks) the room opens its
   video in that window by itself, so there is nothing to press. When the video is open the room shows
   **Netflix connected**.

If Netflix lands somewhere else after signing in, the room tries again a few times, spaced out (up to 5 tries, at
least 8 seconds apart). After that the **Open the room's video** button is the way. If the window shows a different
video than the room, it is left alone and the button offers to switch.

From then on play, pause and seeking are shared. Everyone needs the extension and their own Netflix subscription.

### Window layout

When Netflix is opened from a room window that is wide enough (at least 1000 px), the room window keeps the left
part (about 40 percent, never less than 520 px) and the Netflix window, which has no tab strip or address bar, gets
the right part. This happens once, when the Netflix window is created. If the browser refuses the position, a compact
window is opened anywhere, and as a last resort a normal window.

### Chat inside Netflix

In a Netflix room a round chat button appears at the right edge of the Netflix window. It opens the same chat as the
room page, with reaction buttons. While it is closed, new messages show as short previews next to the button and a
counter. Reactions float over the video. Keys typed into the chat (space, F, the arrows) do not reach Netflix, and the
panel moves into the fullscreen element so it also works in fullscreen. The panel only exists on Netflix video pages.

## How it works

- `site-bridge.js` runs on the CineVerse site and passes messages between the page and the extension. Anything the
  page sends along is reduced to plain, size-limited values first.
- `background.js` pairs one CineVerse tab with one Netflix window, routes messages between them and arranges the two
  windows. It only ever opens a specific `netflix.com/watch/...` video or Netflix's own home/browse page. When the
  extension is installed or updated it also injects `site-bridge.js` into CineVerse tabs that are already open
  (this is what the `scripting` permission and the site address in `host_permissions` are for). A bridge that is
  injected more than once is harmless: each copy writes a token on the page, the newest wins and older copies stand
  down.
- `netflix-content.js` watches the Netflix `<video>` element (play, pause, seeked) and reports its position.
- `netflix-main.js` runs inside the Netflix page and calls the Netflix player to play, pause and seek.
  Setting `video.currentTime` directly makes Netflix show an error, so seeking uses the player object instead.
- `netflix-overlay.js` draws the chat panel in a closed shadow root. Messages are inserted as plain text, never as
  markup. It runs at `document_start` so its key listeners come before Netflix's own.

## Limits

- Seeking depends on Netflix's internal player object. It is not documented and can change without notice.
  If seeking stops working after a Netflix update, the extension needs an update. Play and pause fall back to the
  plain video element.
- Works only on desktop Chrome and Edge (Chromium based browsers). The site checks the browser: phones, Firefox and
  Safari get a clear notice instead of a Netflix room, and the Netflix tile on the Watch page is disabled there.
- Once the extension is published, set `NEXT_PUBLIC_EXTENSION_URL` in the site's environment to its store page. The
  "extension not found" notice then shows an **Add the extension** button; until it is set, no store button is shown.
- The site bridge is allowed on `http://localhost:3000` and `http://127.0.0.1:3000` only. Add your real domain to
  `content_scripts[0].matches` in `manifest.json` (and to `SITE_ORIGINS` in `background.js`) before using it elsewhere.
- Only one room page and one Netflix window are paired at a time.
- Browsers slow down timers in hidden tabs, so keep the room page open and visible.
- Browsers do not make a window narrower than roughly 500 px, which is why smaller room windows are not rearranged.

## Privacy

The extension makes no network requests and stores nothing permanently. Its access to the CineVerse site and to
netflix.com is used only for the bridge, the Netflix window and the chat panel described above. It only forwards playback state and chat
text between two tabs of your own browser. It reads the Netflix page's video element and the page URL to know which
video is open.
