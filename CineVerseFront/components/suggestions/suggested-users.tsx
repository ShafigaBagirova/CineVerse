"use client"

import { useEffect } from "react"
import { UserCheck, UserPlus, Crown } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { useFollow } from "@/components/providers/follow-provider"

export function SuggestedUsers() {
  const { status } = useAuth()
  const {
    suggestedUsers,
    suggestedLoading,
    suggestedError,
    suggestedAuthRequired,
    followingByUserId,
    loadingByUserId,
    ensureFollowStatus,
    toggleFollow,
  } = useFollow()

  useEffect(() => {
    suggestedUsers.forEach((item) => {
      void ensureFollowStatus(item.userId)
    })
  }, [ensureFollowStatus, suggestedUsers])

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-foreground">People You Might Know</h3>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {suggestedLoading && <p className="text-sm text-muted-foreground md:col-span-2 lg:col-span-3">Loading suggestions...</p>}
        {!suggestedLoading && suggestedAuthRequired && (
          <p className="text-sm text-muted-foreground md:col-span-2 lg:col-span-3">
            {status === "authenticated"
              ? "Suggested users are available for VIP accounts."
              : "Sign in to see suggested users."}
          </p>
        )}
        {!suggestedLoading && suggestedError && !suggestedAuthRequired && (
          <p className="text-sm text-muted-foreground md:col-span-2 lg:col-span-3">{suggestedError}</p>
        )}
        {!suggestedLoading && !suggestedError && !suggestedAuthRequired && suggestedUsers.length === 0 && (
          <p className="text-sm text-muted-foreground md:col-span-2 lg:col-span-3">No suggestions available right now.</p>
        )}
        {suggestedUsers.map((user) => (
          <div
            key={user.userId}
            className="rounded-lg border border-border/20 bg-surface p-4 hover:bg-surface-hover transition-colors"
          >
            <div className="flex items-start justify-between mb-3">
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary text-primary-foreground font-bold text-sm">
                  {(user.fullName || user.userName).slice(0, 2).toUpperCase()}
                </div>
                <div>
                  <h4 className="font-semibold text-foreground text-sm">{user.fullName || user.userName}</h4>
                  {user.tasteScore > 0 && (
                    <div className="flex items-center gap-1 text-primary text-xs">
                      <Crown className="h-3 w-3" />
                      Taste {Math.round(user.tasteScore)}%
                    </div>
                  )}
                </div>
              </div>
            </div>

            <p className="text-xs text-muted-foreground mb-3">@{user.userName}</p>

            <div className="flex gap-4 text-center text-xs mb-4 pb-4 border-b border-border/20">
              <div>
                <p className="font-semibold text-foreground">{user.commonMoviesCount}</p>
                <p className="text-muted-foreground">Movies</p>
              </div>
              <div>
                <p className="font-semibold text-foreground">{Math.round(user.tasteScore)}%</p>
                <p className="text-muted-foreground">Taste</p>
              </div>
            </div>

            <button
              onClick={() => void toggleFollow(user.userId)}
              disabled={status !== "authenticated" || loadingByUserId[user.userId]}
              className={`w-full flex items-center justify-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                followingByUserId[user.userId]
                  ? "bg-primary/20 text-primary border border-primary/30"
                  : "bg-primary text-primary-foreground hover:bg-primary/90"
              } disabled:opacity-60`}
            >
              {followingByUserId[user.userId] ? (
                <>
                  <UserCheck className="h-4 w-4" />
                  {loadingByUserId[user.userId] ? "Updating..." : "Following"}
                </>
              ) : (
                <>
                  <UserPlus className="h-4 w-4" />
                  {status !== "authenticated"
                    ? "Sign in to follow"
                    : loadingByUserId[user.userId]
                      ? "Updating..."
                      : "Follow"}
                </>
              )}
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
