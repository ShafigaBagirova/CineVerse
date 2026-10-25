// Runs in the Netflix page's own world, where the player object lives.
// Setting video.currentTime directly makes Netflix show an error, so seeking has to use its player API.
// That API is not documented and can change without notice.
(() => {
  const MAIN_SOURCE = "cineverse-netflix-main";
  const CONTENT_SOURCE = "cineverse-netflix-content";

  function getPlayer() {
    try {
      const videoPlayer = window.netflix.appContext.state.playerApp.getAPI().videoPlayer;
      const sessionIds = videoPlayer.getAllPlayerSessionIds();
      if (!sessionIds || sessionIds.length === 0) return null;
      return videoPlayer.getVideoPlayerBySessionId(sessionIds[0]);
    } catch (error) {
      return null;
    }
  }

  window.addEventListener("message", (event) => {
    if (event.source !== window) return;

    const data = event.data;
    if (!data || data.source !== CONTENT_SOURCE) return;
    if (data.type !== "PLAY" && data.type !== "PAUSE" && data.type !== "SEEK") return;

    let ok = false;
    let error = null;

    try {
      const player = getPlayer();
      if (!player) throw new Error("Netflix player is not ready yet.");

      if (data.type === "PLAY") player.play();
      else if (data.type === "PAUSE") player.pause();
      else player.seek(Math.max(0, Math.round(data.seconds * 1000)));

      ok = true;
    } catch (caught) {
      error = String((caught && caught.message) || caught);
    }

    window.postMessage({ source: MAIN_SOURCE, type: "COMMAND_RESULT", command: data.type, ok, error }, "*");
  });
})();
