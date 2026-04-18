"use client"

import { FormEvent, useEffect, useMemo, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { Film } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { confirmRegistration, resendVerificationCode } from "@/lib/api/auth"
import { cn } from "@/lib/utils"

const PENDING_EMAIL_KEY = "cineverse.pendingVerificationEmail"

function formatCooldown(totalSeconds: number): string {
  const m = Math.floor(totalSeconds / 60)
  const s = totalSeconds % 60
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}`
}

function readStoredEmail(): string | null {
  if (typeof window === "undefined") return null
  try {
    const v = sessionStorage.getItem(PENDING_EMAIL_KEY)?.trim()
    return v && v.length > 0 ? v : null
  } catch {
    return null
  }
}

export default function VerifyPage() {
  const router = useRouter()
  const [email, setEmail] = useState<string | null>(null)
  const [code, setCode] = useState("")
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [cooldownUntil, setCooldownUntil] = useState<number | null>(null)
  const [tick, setTick] = useState(0)
  const [fromRegister, setFromRegister] = useState(false)

  useEffect(() => {
    const params = new URLSearchParams(window.location.search)
    const emailParam = params.get("email")?.trim()
    if (emailParam) {
      try {
        sessionStorage.setItem(PENDING_EMAIL_KEY, decodeURIComponent(emailParam))
      } catch {
        /* ignore */
      }
    }
    const isFromRegister = params.get("from") === "register"
    setFromRegister(isFromRegister)
    setEmail(readStoredEmail())
    if (isFromRegister) setCooldownUntil(Date.now() + 60_000)
  }, [])

  useEffect(() => {
    if (!email) return
    const t = window.setTimeout(() => document.getElementById("verify-code")?.focus(), 120)
    return () => window.clearTimeout(t)
  }, [email])

  useEffect(() => {
    if (cooldownUntil === null || Date.now() >= cooldownUntil) return
    const id = window.setInterval(() => setTick((x) => x + 1), 1000)
    return () => window.clearInterval(id)
  }, [cooldownUntil])

  const cooldownSecondsLeft = useMemo(() => {
    if (cooldownUntil === null || cooldownUntil <= Date.now()) return 0
    return Math.max(0, Math.ceil((cooldownUntil - Date.now()) / 1000))
  }, [cooldownUntil, tick])

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    const em = email?.trim() ?? ""
    const c = code.trim()
    if (!em) {
      setError("Start from Register to receive a verification code.")
      return
    }
    if (!c) {
      setError("Enter the verification code.")
      return
    }
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      await confirmRegistration({ email: em, code: c })
      try {
        sessionStorage.removeItem(PENDING_EMAIL_KEY)
      } catch {
        /* ignore */
      }
      router.replace(`/auth?login=${encodeURIComponent(em)}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Verification failed.")
    } finally {
      setBusy(false)
    }
  }

  async function onResend() {
    const em = email?.trim() ?? ""
    if (!em || cooldownSecondsLeft > 0 || busy) return
    setBusy(true)
    setError(null)
    setSuccess(null)
    try {
      await resendVerificationCode(em)
      setSuccess("We sent a new verification code to your email.")
      setCooldownUntil(Date.now() + 60_000)
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not resend code.")
    } finally {
      setBusy(false)
    }
  }

  const noEmailContext = !email

  return (
    <div className="relative min-h-[calc(100vh-4rem)] overflow-hidden">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_120%_80%_at_50%_-20%,rgba(107,168,155,0.18),transparent_55%),radial-gradient(ellipse_80%_50%_at_100%_50%,rgba(126,187,176,0.06),transparent_50%),radial-gradient(ellipse_60%_40%_at_0%_80%,rgba(107,168,155,0.08),transparent_45%)]"
        aria-hidden
      />
      <div className="pointer-events-none absolute inset-0 bg-[linear-gradient(180deg,rgba(10,10,10,0.3)_0%,var(--background)_100%)]" aria-hidden />

      <div className="relative z-10 mx-auto flex max-w-6xl flex-col gap-10 px-4 py-12 lg:flex-row lg:items-center lg:gap-16 lg:py-16 lg:px-8">
        <div className="flex flex-1 flex-col gap-6 text-center lg:max-w-md lg:text-left">
          <div className="flex flex-col items-center gap-4 lg:items-start">
            <div className="flex h-14 w-14 items-center justify-center rounded-2xl border border-primary/25 bg-primary/10 shadow-[0_0_40px_-8px_rgba(107,168,155,0.4)]">
              <Film className="h-7 w-7 text-primary" strokeWidth={1.75} />
            </div>
            <div>
              <p className="font-serif text-3xl font-bold tracking-tight text-foreground md:text-4xl">CineVerse</p>
              <p className="mt-2 text-sm leading-relaxed text-muted-foreground md:text-base">
                Verify your email to unlock your account and continue.
              </p>
            </div>
          </div>
        </div>

        <div className="flex w-full flex-1 justify-center lg:justify-end">
          <div className="w-full max-w-md">
            <div
              className={cn(
                "rounded-2xl border border-primary/20 bg-card/90 p-1 shadow-2xl shadow-black/40 backdrop-blur-xl",
                "ring-1 ring-primary/10",
              )}
            >
              <div className="rounded-[calc(var(--radius)+4px)] bg-gradient-to-b from-card to-card/95 p-6 sm:p-8">
                <div className="mb-6 space-y-1">
                  <h1 className="font-serif text-2xl font-bold tracking-tight text-foreground">Verify your account</h1>
                  <p className="text-sm text-muted-foreground">Enter the verification code sent to your email.</p>
                </div>

                {fromRegister && (
                  <Alert className="mb-4 border-primary/30 bg-primary/5 text-primary">
                    <AlertDescription className="text-primary">We sent a verification code to your email.</AlertDescription>
                  </Alert>
                )}

                {error && (
                  <Alert variant="destructive" className="mb-4 border-destructive/40 bg-destructive/5">
                    <AlertDescription>{error}</AlertDescription>
                  </Alert>
                )}
                {success && !error && (
                  <Alert className="mb-4 border-primary/30 bg-primary/5 text-primary">
                    <AlertDescription className="text-primary">{success}</AlertDescription>
                  </Alert>
                )}

                {noEmailContext ? (
                  <div className="space-y-4">
                    <p className="text-sm text-muted-foreground">
                      No verification session found. Create an account first, then you’ll land here automatically with a code.
                    </p>
                    <Button asChild className="h-11 w-full font-semibold">
                      <Link href="/auth?tab=register">Go to Register</Link>
                    </Button>
                    <p className="text-center text-sm text-muted-foreground">
                      Already have a code?{" "}
                      <Link href="/auth" className="font-medium text-primary underline-offset-4 hover:underline">
                        Sign in
                      </Link>
                    </p>
                  </div>
                ) : (
                  <form className="space-y-5" onSubmit={onSubmit}>
                    <div className="space-y-2">
                      <Label htmlFor="verify-code" className="text-base font-semibold text-foreground">
                        Verification code
                      </Label>
                      <Input
                        id="verify-code"
                        className="h-14 border-primary/20 bg-background/50 text-center font-mono text-2xl tracking-[0.35em] text-foreground shadow-[inset_0_0_0_1px_rgba(107,168,155,0.12)] placeholder:tracking-normal placeholder:text-muted-foreground/50"
                        placeholder="••••••"
                        inputMode="numeric"
                        autoComplete="one-time-code"
                        maxLength={12}
                        value={code}
                        onChange={(e) => setCode(e.target.value.replace(/\s/g, ""))}
                        required
                      />
                    </div>

                    <Button disabled={busy} className="h-11 w-full text-base font-semibold shadow-lg shadow-primary/15" type="submit">
                      {busy ? "Verifying…" : "Verify email"}
                    </Button>

                    <div className="flex flex-col items-center gap-2 border-t border-border/40 pt-4">
                      {cooldownSecondsLeft > 0 ? (
                        <p className="text-sm tabular-nums text-muted-foreground">Resend code in {formatCooldown(cooldownSecondsLeft)}</p>
                      ) : (
                        <button
                          type="button"
                          disabled={busy || !email}
                          onClick={() => void onResend()}
                          className={cn(
                            "text-sm font-medium text-primary underline-offset-4 transition-colors hover:underline",
                            (busy || !email) && "pointer-events-none opacity-50",
                          )}
                        >
                          Resend code
                        </button>
                      )}
                    </div>
                  </form>
                )}

                <p className="mt-6 text-center text-xs text-muted-foreground">
                  <Link href="/auth" className="font-medium text-primary transition-colors hover:underline">
                    Back to sign in
                  </Link>
                  <span className="mx-1.5 text-border">·</span>
                  <Link href="/movies" className="font-medium text-muted-foreground transition-colors hover:text-foreground">
                    Browse movies
                  </Link>
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
