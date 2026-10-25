// Runs on netflix.com (isolated world). Watches the <video> element and executes commands.
(() => {
  const MAIN_SOURCE = "cineverse-netflix-main";
  const CONTENT_SOURCE = "cineverse-netflix-content";
  const SCAN_INTERVAL_MS = 1000;
  const COMMANDS = ["PLAY", "PAUSE", "SEEK"];

  let video = null;
  let lastWatchId = null;
  let lastPath = null;

  function currentWatchId() {
    const match = window.location.pathname.match(/\/watch\/(\d+)/);
    return match ? match[1] : null;
  }

  function send(payload) {
    try {
      chrome.runtime.sendMessage({ from: "netflix", payload }).catch(() => {});
    } catch (error) {
      // The extension was reloaded; this script is stale until the page is refreshed.
    }
  }

  function tabState() {
    const watchId = currentWatchId();
    return {
      type: "TAB_STATE",
      open: true,
      watchId,
      path: window.location.pathname,
      videoReady: video !== null && watchId !== null,
      playing: video ? !video.paused : false,
      time: video ? video.currentTime : 0,
    };
  }

  function emit(element, action) {
    if (element !== video || currentWatchId() === null) return;
    send({ type: "EVENT", action, time: element.currentTime, playing: !element.paused });
  }

  function attach(element) {
    video = element;
    element.addEventListener("play", () => emit(element, "play"));
    element.addEventListener("pause", () => emit(element, "pause"));
    element.addEventListener("seeked", () => emit(element, "seek"));
  }

  function scan() {
    const element = document.querySelector("video");
    const watchId = currentWatchId();
    let changed = false;

    if (element && element !== video) {
      attach(element);
      changed = true;
    } else if (!element && video) {
      video = null;
      changed = true;
    }

    if (watchId !== lastWatchId) {
      lastWatchId = watchId;
      changed = true;
    }

    // Pages without a video (login, profile picker) still tell the site that the Netflix window is open and where it
    // is, so the room can open its video once the user has signed in. The first scan counts as a change too.
    if (window.location.pathname !== lastPath) {
      lastPath = window.location.pathname;
      changed = true;
    }

    if (changed) {
      send(tabState());
    } else if (video && watchId !== null && !video.paused) {
      send({ type: "TIME", time: video.currentTime, playing: true });
    }
  }

  chrome.runtime.onMessage.addListener((message) => {
    if (!message || message.to !== "netflix") return;

    if (message.type === "REQUEST_STATE") {
      send(tabState());
      return;
    }

    if (!COMMANDS.includes(message.type)) return;
    if (message.type === "SEEK" && (typeof message.seconds !== "number" || !Number.isFinite(message.seconds))) return;

    // The player API only exists in the page's own world, so the command is handed over there.
    window.postMessage({ source: CONTENT_SOURCE, type: message.type, seconds: message.seconds }, "*");
  });

  // When the Netflix player API is unavailable, fall back to the plain <video> element for play and pause.
  window.addEventListener("message", (event) => {
    if (event.source !== window) return;

    const data = event.data;
    if (!data || data.source !== MAIN_SOURCE || data.type !== "COMMAND_RESULT" || data.ok) return;

    if (data.command === "PLAY" && video) {
      video.play().catch(() => {});
    } else if (data.command === "PAUSE" && video) {
      video.pause();
    } else if (data.command === "SEEK") {
      send({
        type: "COMMAND_ERROR",
        command: "SEEK",
        message: "Could not seek in the Netflix player. Netflix may have changed its player, so the extension needs an update.",
      });
    }
  });

  scan();
  setInterval(scan, SCAN_INTERVAL_MS);
})();
