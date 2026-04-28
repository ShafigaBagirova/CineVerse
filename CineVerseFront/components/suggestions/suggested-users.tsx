"use client"

import { useEffect } from "react"
import { Crown } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { FollowButton } from "@/components/follow/follow-button"
import { useFollow } from "@/components/providers/follow-provider"
import { suggestedUserTasteSimilarityPercent } from "@/lib/api/follow"

export function SuggestedUsers() {
  const { status } = useAuth()
  const { suggestedUsers, suggestedLoading, suggestedError, suggestedAuthRequired, ensureFollowStatus } = useFollow()

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
                      Taste {suggestedUserTasteSimilarityPercent(user)}%
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
                <p className="font-semibold text-foreground">{suggestedUserTasteSimilarityPercent(user)}%</p>
                <p className="text-muted-foreground">Taste</p>
              </div>
            </div>

            <FollowButton
              userId={user.userId}
              displayLabel={user.userName ? `@${user.userName}` : user.fullName ?? undefined}
              variant="full"
            />
          </div>
        ))}
      </div>
    </div>
  )
}
