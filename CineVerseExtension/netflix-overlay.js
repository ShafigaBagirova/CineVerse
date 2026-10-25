// Chat and reaction panel inside the Netflix window, so the room can be used without leaving the video.
// It only appears after the CineVerse room (the paired site tab) sends room data to this window.
// This script runs at document_start so its key listeners are registered before Netflix's own.
(() => {
  const HOST_ID = "cineverse-overlay-host";
  const MAX_MESSAGES = 100;
  const MAX_MESSAGE_LENGTH = 500;
  const MAX_NAME_LENGTH = 40;
  const MAX_EMOJI_LENGTH = 8;
  const MAX_TOASTS = 3;
  const TOAST_MS = 4500;
  const FLOAT_MS = 4000;
  const REACTIONS = ["❤️", "😂", "😮", "😢", "👏", "🔥"];

  const STYLES = `
    .cv { position: fixed; inset: 0; pointer-events: none; font-family: system-ui, -apple-system, "Segoe UI", sans-serif; font-size: 14px; line-height: 1.35; color: #fff; }
    .toggle { position: fixed; right: 12px; top: 45%; width: 44px; height: 44px; border: 0; border-radius: 50%; background: rgba(20, 20, 20, 0.85); color: #fff; font-size: 20px; cursor: pointer; pointer-events: auto; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.5); }
    .badge { position: absolute; top: -4px; right: -4px; min-width: 18px; height: 18px; padding: 0 4px; border-radius: 9px; background: #2f8f89; font-size: 11px; line-height: 18px; text-align: center; box-sizing: border-box; }
    .badge[hidden], .panel[hidden] { display: none; }
    .panel { position: fixed; right: 12px; top: 12%; width: 300px; height: 62%; display: flex; flex-direction: column; border-radius: 12px; background: rgba(10, 10, 10, 0.9); box-shadow: 0 6px 24px rgba(0, 0, 0, 0.6); pointer-events: auto; overflow: hidden; }
    .head { display: flex; align-items: center; justify-content: space-between; gap: 8px; padding: 10px 12px; border-bottom: 1px solid rgba(255, 255, 255, 0.12); font-weight: 600; }
    .head span { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .close { border: 0; background: transparent; color: #fff; font-size: 20px; line-height: 1; cursor: pointer; }
    .list { flex: 1; overflow-y: auto; padding: 10px 12px; display: flex; flex-direction: column; gap: 8px; }
    .msg { max-width: 90%; }
    .msg.mine { align-self: flex-end; text-align: right; }
    .who { display: block; font-size: 11px; color: rgba(255, 255, 255, 0.6); }
    .body { display: inline-block; padding: 6px 10px; border-radius: 10px; background: rgba(255, 255, 255, 0.14); text-align: left; overflow-wrap: anywhere; }
    .msg.mine .body { background: #2f8f89; }
    .empty { color: rgba(255, 255, 255, 0.5); text-align: center; font-size: 12px; }
    .react { display: flex; gap: 4px; padding: 6px 10px; border-top: 1px solid rgba(255, 255, 255, 0.12); }
    .react button { flex: 1; border: 0; border-radius: 8px; background: rgba(255, 255, 255, 0.1); font-size: 16px; padding: 4px 0; cursor: pointer; }
    .form { display: flex; gap: 6px; padding: 8px 10px; border-top: 1px solid rgba(255, 255, 255, 0.12); }
    .input { flex: 1; min-width: 0; padding: 8px 10px; border: 1px solid rgba(255, 255, 255, 0.2); border-radius: 8px; background: rgba(255, 255, 255, 0.08); color: #fff; font: inherit; outline: none; }
    .send { border: 0; border-radius: 8px; background: #2f8f89; color: #fff; padding: 0 12px; font: inherit; cursor: pointer; }
    .toasts { position: fixed; right: 68px; top: 45%; display: flex; flex-direction: column; align-items: flex-end; gap: 6px; max-width: 280px; }
    .toast { padding: 6px 10px; border-radius: 10px; background: rgba(10, 10, 10, 0.88); overflow-wrap: anywhere; }
    .floats { position: fixed; right: 70px; bottom: 90px; display: flex; flex-direction: column-reverse; align-items: flex-end; gap: 4px; }
    .float { padding: 4px 10px; border-radius: 999px; background: rgba(10, 10, 10, 0.7); animation: rise ${FLOAT_MS}ms ease-out forwards; }
    @keyframes rise {
      0% { opacity: 0; transform: translateY(10px); }
      10% { opacity: 1; transform: translateY(0); }
      80% { opacity: 1; }
      100% { opacity: 0; transform: translateY(-20px); }
    }
  `;

  let host = null;
  let ui = null;
  let roomTitle = "CineVerse room";
  let messages = [];
  let unread = 0;
  let panelOpen = false;
  let announcedPath = null;

  function isWatchPage() {
    return /\/watch\//.test(window.location.pathname);
  }

  function sendToSite(payload) {
    try {
      chrome.runtime.sendMessage({ from: "netflix", payload }).catch(() => {});
    } catch (error) {
      // The extension was reloaded; this panel is stale until the page is refreshed.
    }
  }

  function text(value, maxLength) {
    return typeof value === "string" ? value.slice(0, maxLength) : "";
  }

  function normalizeMessage(raw) {
    if (!raw || typeof raw !== "object") return null;
    const content = text(raw.content, MAX_MESSAGE_LENGTH).trim();
    if (!content) return null;

    return {
      senderName: text(raw.senderName, MAX_NAME_LENGTH).trim() || "Someone",
      mine: raw.mine === true,
      content,
    };
  }

  function element(tag, className, content) {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (content !== undefined) node.textContent = content;
    return node;
  }

  function applyStyles(root) {
    try {
      const sheet = new CSSStyleSheet();
      sheet.replaceSync(STYLES);
      root.adoptedStyleSheets = [sheet];
    } catch (error) {
      const style = document.createElement("style");
      style.textContent = STYLES;
      root.appendChild(style);
    }
  }

  function submit() {
    if (!ui) return;
    const content = ui.input.value.trim().slice(0, MAX_MESSAGE_LENGTH);
    if (!content) return;

    ui.input.value = "";
    sendToSite({ type: "SEND_CHAT", content });
  }

  function build(root) {
    applyStyles(root);

    const wrapper = element("div", "cv");
    const floats = element("div", "floats");
    const toasts = element("div", "toasts");

    const toggle = element("button", "toggle", "💬");
    toggle.type = "button";
    toggle.title = "CineVerse chat";
    const badge = element("span", "badge", "0");
    badge.hidden = true;
    toggle.appendChild(badge);

    const panel = element("div", "panel");
    panel.hidden = true;

    const head = element("div", "head");
    const title = element("span", "", roomTitle);
    const close = element("button", "close", "×");
    close.type = "button";
    close.title = "Close";
    head.append(title, close);

    const list = element("div", "list");

    const reactions = element("div", "react");
    for (const emoji of REACTIONS) {
      const button = element("button", "", emoji);
      button.type = "button";
      button.addEventListener("click", () => sendToSite({ type: "SEND_REACTION", emoji }));
      reactions.appendChild(button);
    }

    const form = element("div", "form");
    const input = element("input", "input");
    input.type = "text";
    input.maxLength = MAX_MESSAGE_LENGTH;
    input.placeholder = "Message";
    const send = element("button", "send", "Send");
    send.type = "button";
    send.addEventListener("click", submit);
    form.append(input, send);

    panel.append(head, list, reactions, form);
    wrapper.append(floats, toasts, toggle, panel);
    root.appendChild(wrapper);

    toggle.addEventListener("click", () => setPanelOpen(!panelOpen));
    close.addEventListener("click", () => setPanelOpen(false));

    return { title, list, floats, toasts, badge, panel, input };
  }

  function syncAttributes() {
    if (!host) return;
    host.setAttribute("data-open", String(panelOpen));
    host.setAttribute("data-unread", String(unread));
  }

  // In fullscreen only the fullscreen element and its children are visible, so the panel moves into it.
  function ensureMounted() {
    if (!host) {
      host = document.createElement("div");
      host.id = HOST_ID;
      host.style.cssText = "all: initial; position: fixed; top: 0; left: 0; width: 0; height: 0; z-index: 2147483647;";
      ui = build(host.attachShadow({ mode: "closed" }));
      syncAttributes();
    }

    const parent = document.fullscreenElement || document.documentElement;
    if (parent && host.parentNode !== parent) parent.appendChild(host);
  }

  function updateBadge() {
    if (!ui) return;
    ui.badge.hidden = unread === 0;
    ui.badge.textContent = unread > 99 ? "99+" : String(unread);
    syncAttributes();
  }

  function messageNode(message) {
    const wrap = element("div", message.mine ? "msg mine" : "msg");
    wrap.append(element("span", "who", message.mine ? "You" : message.senderName), element("span", "body", message.content));
    return wrap;
  }

  function renderList() {
    if (!ui) return;
    ui.list.textContent = "";

    if (messages.length === 0) {
      ui.list.appendChild(element("div", "empty", "No messages yet."));
      return;
    }

    for (const message of messages) ui.list.appendChild(messageNode(message));
    ui.list.scrollTop = ui.list.scrollHeight;
  }

  function setPanelOpen(open) {
    panelOpen = open;
    if (!ui) return;

    ui.panel.hidden = !open;
    if (open) {
      unread = 0;
      updateBadge();
      renderList();
      ui.input.focus();
    }
    syncAttributes();
  }

  function showToast(message) {
    if (!ui) return;

    const toast = element("div", "toast", `${message.senderName}: ${message.content}`.slice(0, 160));
    ui.toasts.appendChild(toast);
    while (ui.toasts.childNodes.length > MAX_TOASTS) ui.toasts.removeChild(ui.toasts.firstChild);
    window.setTimeout(() => toast.remove(), TOAST_MS);
  }

  function addMessage(message) {
    messages.push(message);
    if (messages.length > MAX_MESSAGES) messages = messages.slice(-MAX_MESSAGES);
    if (!ui) return;

    if (panelOpen) {
      if (messages.length === 1) ui.list.textContent = "";
      ui.list.appendChild(messageNode(message));
      ui.list.scrollTop = ui.list.scrollHeight;
    } else if (!message.mine) {
      unread += 1;
      updateBadge();
      showToast(message);
    }
  }

  function showReaction(raw) {
    const emoji = text(raw && raw.emoji, MAX_EMOJI_LENGTH).trim();
    if (!emoji || !ui) return;

    const name = text(raw.senderName, MAX_NAME_LENGTH).trim();
    const chip = element("div", "float", name ? `${emoji} ${name}` : emoji);
    ui.floats.appendChild(chip);
    window.setTimeout(() => chip.remove(), FLOAT_MS);
  }

  function handleRoomMessage(message) {
    if (!message || message.to !== "netflix" || !isWatchPage()) return;

    const payload = message.payload && typeof message.payload === "object" ? message.payload : {};

    if (message.type === "ROOM_INFO") {
      roomTitle = text(payload.title, 80).trim() || "CineVerse room";
      ensureMounted();
      ui.title.textContent = roomTitle;
    } else if (message.type === "CHAT_HISTORY") {
      ensureMounted();
      messages = (Array.isArray(payload.messages) ? payload.messages : []).map(normalizeMessage).filter(Boolean).slice(-MAX_MESSAGES);
      if (panelOpen) renderList();
    } else if (message.type === "CHAT_MESSAGE") {
      const normalized = normalizeMessage(payload);
      if (!normalized) return;
      ensureMounted();
      addMessage(normalized);
    } else if (message.type === "REACTION") {
      ensureMounted();
      showReaction(payload);
    }
  }

  // Netflix reacts to keys such as space, F and the arrows, and the panel's input is a normal text field.
  // These listeners run first and keep such keys away from Netflix while the input has focus.
  function isOverlayEvent(event) {
    return Boolean(host) && event.target === host;
  }

  for (const type of ["keydown", "keyup", "keypress"]) {
    window.addEventListener(
      type,
      (event) => {
        if (!isOverlayEvent(event)) return;

        event.stopImmediatePropagation();

        if (type === "keydown" && event.key === "Enter" && !event.isComposing) {
          event.preventDefault();
          submit();
        }
      },
      true
    );
  }

  // A double click would otherwise toggle Netflix's fullscreen.
  window.addEventListener(
    "dblclick",
    (event) => {
      if (isOverlayEvent(event)) event.stopImmediatePropagation();
    },
    true
  );

  document.addEventListener("fullscreenchange", () => {
    if (host) ensureMounted();
  });

  chrome.runtime.onMessage.addListener(handleRoomMessage);

  // The panel only exists on video pages. Entering one asks the room for its details.
  function tick() {
    const watching = isWatchPage();
    if (host) host.style.display = watching ? "block" : "none";

    if (watching && announcedPath !== window.location.pathname) {
      announcedPath = window.location.pathname;
      sendToSite({ type: "OVERLAY_READY" });
    } else if (!watching) {
      announcedPath = null;
    }
  }

  tick();
  setInterval(tick, 1000);
})();
