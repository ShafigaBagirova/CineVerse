"use client"

import { type FormEvent, useEffect, useMemo, useState } from "react"
import { Star, Heart, MessageCircle, Share2 } from "lucide-react"
import { cn } from "@/lib/utils"
import {
  createReview,
  deleteReview,
  getMyReview,
  getReviewsByMovie,
  updateReview,
  type ReviewDto,
} from "@/lib/api/reviews"
import { createMovieRating, getMovieRatingSummary } from "@/lib/api/ratings"
import { useAuth } from "@/components/providers/auth-provider"
import { FollowButton } from "@/components/follow/follow-button"
import { useFollow } from "@/components/providers/follow-provider"
import { isOptionalAbsenceError } from "@/lib/api/optional-absence"

interface ReviewViewModel {
  id: number
  userId: string
  user: string
  avatar: string
  rating?: number
  text: string
  date: string
  likes?: number
}

export function MovieReviews({ movieId }: { movieId: number }) {
  const { status } = useAuth()
  const [likedReviews, setLikedReviews] = useState<Set<number>>(new Set())
  const [reviews, setReviews] = useState<ReviewDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)
  const [success, setSuccess] = useState<string | null>(null)
  const [form, setForm] = useState({ content: "", isSpoiler: false, rating: "8" })
  const [hasMyReview, setHasMyReview] = useState(false)
  const { ensureFollowStatus } = useFollow()

  const normalizedReviews: ReviewViewModel[] = useMemo(
    () =>
      reviews.map((review) => {
        const user = review.userName || "User"
        return {
          id: review.id,
          userId: review.userId,
          user,
          avatar: user.slice(0, 2).toUpperCase(),
          rating: undefined,
          text: review.isSpoiler ? `Spoiler: ${review.content}` : review.content,
          date: new Date(review.createdAt).toLocaleDateString(),
          likes: 0,
        }
      }),
    [reviews]
  )
  const loadReviews = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await getReviewsByMovie(movieId, 1, 20, { quiet: true })
      setReviews(Array.isArray(data?.items) ? data.items : [])
    } catch (err) {
      if (isOptionalAbsenceError(err, { allowNoContent: true })) {
        setReviews([])
        setError(null)
        return
      }
      setReviews([])
      setError(err instanceof Error ? err.message : "Failed to load reviews.")
      console.error("[movie-reviews] loadReviews failed", {
        movieId,
        status: err instanceof ApiError ? err.status : undefined,
        message: err instanceof Error ? err.message : String(err),
        path: err instanceof ApiError ? err.path : undefined,
        method: err instanceof ApiError ? err.method : undefined,
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadReviews()
  }, [movieId])

  useEffect(() => {
    const loadMine = async () => {
      if (status !== "authenticated") {
        setHasMyReview(false)
        setForm({ content: "", isSpoiler: false, rating: "8" })
        return
      }
      let next = { content: "", isSpoiler: false, rating: "8" }
      try {
        const mine = await getMyReview(movieId, { quiet: true })
        if (!mine || typeof mine !== "object") {
          setHasMyReview(false)
        } else {
          next = { ...next, content: mine.content, isSpoiler: mine.isSpoiler }
          setHasMyReview(true)
        }
      } catch (err) {
        void err
        next = { ...next, content: "", isSpoiler: false }
        setHasMyReview(false)
      }
      try {
        const summary = await getMovieRatingSummary(movieId, { quiet: true })
        if (summary && typeof summary === "object") {
          next = { ...next, rating: String(summary.myRating ?? 8) }
        }
      } catch (err) {
        void err
        next = { ...next, rating: "8" }
      }
      setForm(next)
    }
    void loadMine()
  }, [movieId, status])

  useEffect(() => {
    normalizedReviews.forEach((review) => {
      void ensureFollowStatus(review.userId)
    })
  }, [ensureFollowStatus, normalizedReviews])

  async function onSubmitReview(e: FormEvent) {
    e.preventDefault()
    if (status !== "authenticated") {
      setError("Sign in to write a review.")
      return
    }
    const ratingNum = Number(form.rating)
    if (!Number.isFinite(ratingNum) || ratingNum < 1 || ratingNum > 10) {
      setError("Rating must be between 1 and 10.")
      return
    }
    const wasUpdate = hasMyReview
    try {
      setBusy(true)
      setError(null)
      setSuccess(null)
      const { rating: _r, ...reviewPayload } = form
      if (wasUpdate) {
        await updateReview(movieId, reviewPayload)
      } else {
        await createReview(movieId, reviewPayload)
      }
      try {
        await createMovieRating(movieId, { rating: ratingNum })
      } catch (ratingErr) {
        setError(
          ratingErr instanceof Error
            ? `${ratingErr.message} (Your review was saved.)`
            : "Rating could not be saved. Your review was saved."
        )
        await loadReviews()
        setHasMyReview(true)
        return
      }
      setSuccess(wasUpdate ? "Review updated successfully." : "Review posted successfully.")
      await loadReviews()
      setHasMyReview(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save review.")
    } finally {
      setBusy(false)
    }
  }

  async function onDeleteReview() {
    if (status !== "authenticated") {
      setError("Sign in to delete your review.")
      return
    }
    try {
      setBusy(true)
      setError(null)
      setSuccess(null)
      await deleteReview(movieId)
      let ratingStr = "8"
      try {
        const summary = await getMovieRatingSummary(movieId, { quiet: true })
        ratingStr = String(summary.myRating ?? 8)
      } catch {
        // Summary unavailable (e.g. no ratings yet); keep default.
      }
      setForm({ content: "", isSpoiler: false, rating: ratingStr })
      setHasMyReview(false)
      setSuccess("Review deleted successfully.")
      await loadReviews()
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to delete review.")
    } finally {
      setBusy(false)
    }
  }

  const toggleLike = (index: number) => {
    setLikedReviews(prev => {
      const next = new Set(prev)
      if (next.has(index)) next.delete(index)
      else next.add(index)
      return next
    })
  }

  return (
    <div>
      <h2 className="mb-5 font-serif text-xl font-bold text-foreground">Community Reviews</h2>
      <div className="mb-5 rounded-2xl border border-border/20 bg-card p-4">
        <form onSubmit={onSubmitReview} className="space-y-3">
          <textarea
            value={form.content}
            onChange={(e) => setForm((prev) => ({ ...prev, content: e.target.value }))}
            maxLength={1000}
            placeholder="Write your review..."
            className="min-h-24 w-full rounded-lg border border-border/50 bg-secondary px-3 py-2 text-sm text-foreground"
          />
          <label className="block text-xs text-muted-foreground">
            Your rating (1–10)
            <input
              min={1}
              max={10}
              step={0.1}
              type="number"
              value={form.rating}
              onChange={(e) => setForm((prev) => ({ ...prev, rating: e.target.value }))}
              className="mt-1 w-full max-w-[8rem] rounded-lg border border-border/50 bg-secondary px-3 py-2 text-sm text-foreground"
            />
          </label>
          <label className="flex items-center gap-2 text-xs text-muted-foreground">
            <input
              type="checkbox"
              checked={form.isSpoiler}
              onChange={(e) => setForm((prev) => ({ ...prev, isSpoiler: e.target.checked }))}
            />
            Mark as spoiler
          </label>
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={busy}
              className="rounded-lg bg-primary px-4 py-2 text-xs font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
            >
              {busy ? "Saving..." : hasMyReview ? "Update Review" : "Post Review"}
            </button>
            {hasMyReview && (
              <button
                type="button"
                onClick={onDeleteReview}
                disabled={busy}
                className="rounded-lg border border-border/40 px-4 py-2 text-xs font-semibold text-muted-foreground transition-colors hover:bg-secondary disabled:opacity-60"
              >
                Delete Review
              </button>
            )}
          </div>
          {error && <p className="text-xs text-destructive">{error}</p>}
          {success && <p className="text-xs text-primary">{success}</p>}
        </form>
      </div>

      {loading ? (
        <div className="rounded-2xl border border-border/50 bg-card p-8 text-center">
          <p className="text-sm text-muted-foreground">Loading reviews...</p>
        </div>
      ) : normalizedReviews.length === 0 ? (
        <div className="rounded-2xl border border-border/50 bg-card p-8 text-center">
          <p className="text-sm text-muted-foreground">No reviews yet. Be the first to share your thoughts!</p>
        </div>
      ) : (
        <div className="flex flex-col gap-4">
          {normalizedReviews.map((review, i) => {
            const isLiked = likedReviews.has(i)
            return (
              <div
                key={i}
                className="rounded-2xl border border-border/20 bg-card p-5 transition-colors hover:bg-surface"
              >
                <div className="flex items-start justify-between mb-3">
                  <div className="flex items-center gap-3">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-sm font-bold text-primary">
                      {review.avatar}
                    </div>
                    <div>
                      <p className="text-sm font-semibold text-foreground">{review.user}</p>
                      <p className="text-xs text-muted-foreground">{review.date}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    {review.rating != null && (
                      <div className="flex items-center gap-1 rounded-full bg-primary/10 px-2.5 py-1">
                        <Star className="h-3.5 w-3.5 fill-primary text-primary" />
                        <span className="text-xs font-bold text-primary">{review.rating}/10</span>
                      </div>
                    )}
                    <FollowButton
                      userId={review.userId}
                      displayLabel={review.user}
                      variant="compact"
                    />
                  </div>
                </div>
                <p className="text-sm leading-relaxed text-muted-foreground">{review.text}</p>
                <div className="mt-3 flex items-center gap-4">
                  <button
                    onClick={() => toggleLike(i)}
                    className={cn(
                      "flex items-center gap-1.5 text-xs font-medium transition-colors",
                      isLiked
                        ? "text-primary"
                        : "text-muted-foreground hover:text-primary"
                    )}
                  >
                    <Heart className={cn("h-4 w-4", isLiked && "fill-primary")} />
                    <span>{isLiked ? review.likes ? review.likes + 1 : 1 : review.likes ? review.likes : 0}</span>
                  </button>
                  <button className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground hover:text-foreground transition-colors">
                    <MessageCircle className="h-4 w-4" />
                    <span>Reply</span>
                  </button>
                  <button className="flex items-center gap-1.5 text-xs font-medium text-muted-foreground hover:text-foreground transition-colors">
                    <Share2 className="h-4 w-4" />
                    <span>Share</span>
                  </button>
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
