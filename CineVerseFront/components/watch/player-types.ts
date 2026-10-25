/** Common control surface shared by every watch room player (YouTube iframe, Netflix extension bridge). */
export interface PlayerHandle {
  play: () => void
  pause: () => void
  seekTo: (seconds: number) => void
  getCurrentTime: () => number
  getState: () => number
  isPlaying: () => boolean
}

/** Numeric states match the YouTube IFrame API so both players can feed the same sync logic. */
export const PLAYER_STATE = {
  PLAYING: 1,
  PAUSED: 2,
  BUFFERING: 3,
} as const
