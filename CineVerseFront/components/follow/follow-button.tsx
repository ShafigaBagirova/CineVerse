"use client"

import { useState } from "react"
import { Loader2, UserCheck, UserPlus } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { useFollow } from "@/components/providers/follow-provider"
import { cn } from "@/lib/utils"
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button"

export type FollowButtonProps = {
  userId: string
  /** Used in the unfollow confirmation copy */
  displayLabel?: string
  className?: string
  variant?: "default" | "compact" | "full"
  /** Called after a successful follow or unfollow */
  onRelationshipChange?: () => void
}

export function FollowButton({
  userId,
  displayLabel,
  className,
  variant = "default",
  onRelationshipChange,
}: FollowButtonProps) {
  const { status } = useAuth()
  const { followingByUserId, loadingByUserId, followUser, unfollowUser } = useFollow()
  const [confirmOpen, setConfirmOpen] = useState(false)

  const isFollowing = followingByUserId[userId] ?? false
  const loading = loadingByUserId[userId] ?? false

  async function handleClick() {
    if (status !== "authenticated" || loading) return
    if (!isFollowing) {
      await followUser(userId)
      onRelationshipChange?.()
      return
    }
    setConfirmOpen(true)
  }

  async function handleConfirmUnfollow() {
    await unfollowUser(userId)
    onRelationshipChange?.()
    setConfirmOpen(false)
  }

  const showAuth = status !== "authenticated"
  const disabled = showAuth || loading

  const sizeClasses =
    variant === "full"
      ? "w-full justify-center gap-2 rounded-lg px-3 py-2 text-sm font-medium"
      : variant === "compact"
        ? "gap-1 rounded-full px-2.5 py-1 text-xs font-medium"
        : "shrink-0 gap-2 rounded-lg px-6 py-2.5 text-sm font-semibold"

  return (
    <>
      <button
        type="button"
        onClick={() => void handleClick()}
        disabled={disabled}
        className={cn(
          "inline-flex items-center transition-colors disabled:cursor-not-allowed disabled:opacity-60",
          sizeClasses,
          !isFollowing && "bg-primary text-primary-foreground hover:bg-primary/90",
          isFollowing &&
            "group border border-border bg-muted text-foreground hover:border-destructive/50 hover:bg-destructive/10",
          className
        )}
      >
        {loading ? (
          <>
            <Loader2 className={cn("animate-spin text-current", variant === "compact" ? "h-3 w-3" : "h-4 w-4")} />
            <span>{variant === "compact" ? "…" : "Please wait…"}</span>
          </>
        ) : showAuth ? (
          <>
            <UserPlus className={variant === "compact" ? "h-3 w-3" : "h-4 w-4"} />
            <span>{variant === "compact" ? "Sign in" : "Sign in to follow"}</span>
          </>
        ) : !isFollowing ? (
          <>
            <UserPlus className={variant === "compact" ? "h-3 w-3" : "h-4 w-4"} />
            <span>Follow</span>
          </>
        ) : (
          <>
            <UserCheck className={cn("shrink-0 text-current group-hover:hidden", variant === "compact" ? "h-3 w-3" : "h-4 w-4")} aria-hidden />
            <span className="group-hover:hidden">Following</span>
            <span className="hidden text-destructive group-hover:inline">Unfollow</span>
          </>
        )}
      </button>

      <AlertDialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Unfollow user?</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to unfollow this user?
              {displayLabel?.trim() ? (
                <>
                  {" "}
                  <span className="font-medium text-foreground">({displayLabel.trim()})</span>
                </>
              ) : null}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel type="button">Cancel</AlertDialogCancel>
            <Button type="button" variant="destructive" disabled={loading} onClick={() => void handleConfirmUnfollow()}>
              {loading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Unfollowing…
                </>
              ) : (
                "Unfollow"
              )}
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </>
  )
}
