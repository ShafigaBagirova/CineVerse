"use client"

import { useEffect, useState } from "react"
import { Star, Users } from "lucide-react"
import { createMovieRating, getMovieRatingSummary, type GetMovieRatingSummaryResponse } from "@/lib/api/ratings"
import { useAuth } from "@/components/providers/auth-provider"
import { isOptionalAbsenceError } from "@/lib/api/optional-absence"

interface MovieAnalysisProps {
  movieId: number
}

export function MovieAnalysisSection({ movieId }: MovieAnalysisProps) {
  const { status } = useAuth()
  const [analysis, setAnalysis] = useState<GetMovieRatingSummaryResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [ratingValue, setRatingValue] = useState("8")
  const [submitting, setSubmitting] = useState(false)
  const [submitMessage, setSubmitMessage] = useState<string | null>(null)

  const loadSummary = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await getMovieRatingSummary(movieId, { quiet: true })
      if (
        !data ||
        typeof data !== "object" ||
        ("ratingCount" in data && !Number.isFinite(Number(data.ratingCount)))
      ) {
        setAnalysis(null)
        setError(null)
        setRatingValue("8")
        return
      }
      setAnalysis(data)
      setRatingValue(String(data.myRating ?? 8))
    } catch (err) {
      void err
      setAnalysis(null)
      setError(null)
      setRatingValue("8")
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const load = async () => {
      await loadSummary()
    }
    void load()
  }, [movieId])

  async function onSubmitRating() {
    if (status !== "authenticated") {
      setSubmitMessage("Sign in to submit your rating.")
      return
    }
    try {
      setSubmitting(true)
      setSubmitMessage(null)
      await createMovieRating(movieId, { rating: Number(ratingValue) })
      setSubmitMessage("Rating saved successfully.")
      await loadSummary()
    } catch (err) {
      setSubmitMessage(err instanceof Error ? err.message : "Failed to save rating.")
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="rounded-lg border border-border/20 bg-surface p-6">
        <p className="text-sm text-muted-foreground">Loading movie insights...</p>
      </div>
    )
  }

  return (
    <div className="space-y-6 mt-8">
      <h3 className="text-xl font-semibold text-foreground">Movie Analysis & Insights</h3>
      {error || !analysis ? (
        <div className="rounded-lg border border-border/20 bg-surface p-6">
          <p className="text-sm text-muted-foreground">
            {error ?? "No rating insights available yet."}
          </p>
        </div>
      ) : (
        <div className="grid gap-6 lg:grid-cols-3">
          <div className="rounded-lg border border-border/20 bg-surface p-6">
            <h4 className="text-sm font-semibold text-foreground mb-3">Average Rating</h4>
            <div className="flex items-center gap-2">
              <Star className="h-5 w-5 fill-primary text-primary" />
              <p className="text-2xl font-bold text-foreground">
                {Number(analysis.userAverageRating ?? 0).toFixed(1)}
              </p>
            </div>
          </div>
          <div className="rounded-lg border border-border/20 bg-surface p-6">
            <h4 className="text-sm font-semibold text-foreground mb-3">Total Ratings</h4>
            <div className="flex items-center gap-2">
              <Users className="h-5 w-5 text-primary" />
              <p className="text-2xl font-bold text-foreground">{analysis.ratingCount}</p>
            </div>
          </div>
          <div className="rounded-lg border border-border/20 bg-surface p-6">
            <h4 className="text-sm font-semibold text-foreground mb-3">My Rating</h4>
            <p className="text-2xl font-bold text-foreground">
              {analysis.myRating ? Number(analysis.myRating).toFixed(1) : "N/A"}
            </p>
            <div className="mt-4 space-y-2">
              <input
                min={1}
                max={10}
                step={0.1}
                type="number"
                value={ratingValue}
                onChange={(e) => setRatingValue(e.target.value)}
                className="w-full rounded-lg border border-border/50 bg-secondary px-3 py-2 text-sm text-foreground"
              />
              <button
                onClick={onSubmitRating}
                disabled={submitting}
                className="w-full rounded-lg bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:opacity-60"
              >
                {submitting ? "Saving..." : "Submit Rating"}
              </button>
              {submitMessage && (
                <p className="text-xs text-muted-foreground">{submitMessage}</p>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
