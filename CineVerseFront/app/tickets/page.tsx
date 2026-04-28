"use client"

import { useEffect, useMemo, useState } from "react"
import { useSearchParams } from "next/navigation"
import { MapPin, Clock, Armchair, QrCode, Ticket } from "lucide-react"
import { useAuth } from "@/components/providers/auth-provider"
import { ApiError } from "@/lib/api/types"
import { getMyTickets, getTicketById, type GetMyTicketsResponse } from "@/lib/api/tickets"

export default function TicketsPage() {
  const searchParams = useSearchParams()
  const focusedTicketId = Number(searchParams.get("ticketId")) || 0
  const { status } = useAuth()
  const [tickets, setTickets] = useState<GetMyTicketsResponse[]>([])
  const [focusedTicket, setFocusedTicket] = useState<GetMyTicketsResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [authRequired, setAuthRequired] = useState(false)

  useEffect(() => {
    const load = async () => {
      if (status !== "authenticated") {
        setLoading(false)
        setTickets([])
        setFocusedTicket(null)
        setAuthRequired(true)
        setError(null)
        return
      }
      try {
        setLoading(true)
        setAuthRequired(false)
        setError(null)
        const [myTickets, maybeFocused] = await Promise.all([
          getMyTickets({ pageNumber: 1, pageSize: 50 }),
          focusedTicketId ? getTicketById(focusedTicketId) : Promise.resolve(null),
        ])
        setTickets(myTickets.items)
        setFocusedTicket(maybeFocused)
      } catch (err) {
        setTickets([])
        setFocusedTicket(null)
        if (err instanceof ApiError && (err.status === 401 || err.status === 403)) {
          setAuthRequired(true)
        }
        setError(err instanceof Error ? err.message : "Failed to load tickets.")
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [focusedTicketId, status])

  const groupedTickets = useMemo(() => {
    const map = new Map<string, GetMyTicketsResponse[]>()
    tickets.forEach((ticket) => {
      const key = `${ticket.movieId}-${ticket.screeningId}-${ticket.cinemaId}-${ticket.hallId}`
      if (!map.has(key)) map.set(key, [])
      map.get(key)!.push(ticket)
    })
    return Array.from(map.values())
  }, [tickets])

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 lg:px-8">
      <div className="mb-8">
        <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
          Your Bookings
        </p>
        <h1 className="font-serif text-3xl font-bold text-foreground md:text-4xl">
          My Tickets
        </h1>
      </div>

      {loading ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <p className="text-sm text-muted-foreground">Loading tickets...</p>
        </div>
      ) : authRequired ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <Ticket className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <p className="text-lg font-medium text-foreground">Sign in required</p>
          <p className="mt-1 text-sm text-muted-foreground">Sign in to view your purchased tickets.</p>
        </div>
      ) : error ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <p className="text-sm text-muted-foreground">{error}</p>
        </div>
      ) : groupedTickets.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-20 text-center">
          <Ticket className="mb-4 h-12 w-12 text-muted-foreground/50" />
          <p className="text-lg font-medium text-foreground">No tickets yet</p>
          <p className="mt-1 text-sm text-muted-foreground">Book a movie to see your tickets here!</p>
        </div>
      ) : (
        <div className="flex flex-col gap-5">
          {focusedTicket && (
            <div className="rounded-xl border border-primary/30 bg-primary/5 p-4">
              <p className="text-xs font-medium uppercase tracking-wider text-primary">Booking Confirmed</p>
              <p className="mt-1 text-sm text-foreground">
                Ticket #{focusedTicket.id} - {focusedTicket.movieTitle} - Row {focusedTicket.seatRow}, Seat {focusedTicket.seatNumber}
              </p>
            </div>
          )}
          {groupedTickets.map((group) => {
            const ticket = group[0]
            const seatLabels = group
              .map((item) => `${item.seatRow}-${item.seatNumber}`)
              .sort((a, b) => a.localeCompare(b))
            return (
            <div
              key={`${ticket.screeningId}-${ticket.id}`}
              className="overflow-hidden rounded-2xl border border-border/50 bg-card transition-colors hover:border-border"
            >
              <div className="flex flex-col sm:flex-row">
                {/* Details */}
                <div className="flex flex-1 flex-col justify-between p-5">
                  <div>
                    <div className="flex items-start justify-between">
                      <div>
                        <h2 className="font-serif text-lg font-bold text-foreground">{ticket.movieTitle}</h2>
                        <p className="mt-0.5 text-xs font-medium text-primary">Ticket IDs: {group.map((item) => item.id).join(", ")}</p>
                      </div>
                    </div>

                    <div className="mt-3 flex flex-wrap gap-4 text-sm text-muted-foreground">
                      <span className="flex items-center gap-1.5">
                        <MapPin className="h-3.5 w-3.5 text-primary" />
                        {ticket.cinemaName} - {ticket.hallName}
                      </span>
                      <span className="flex items-center gap-1.5">
                        <Clock className="h-3.5 w-3.5" />
                        {new Date(ticket.screeningStartTime).toLocaleString()}
                      </span>
                      <span className="flex items-center gap-1.5">
                        <Armchair className="h-3.5 w-3.5" />
                        Seats: {seatLabels.join(", ")}
                      </span>
                    </div>
                  </div>
                </div>

                {/* QR Code Placeholder */}
                <div className="flex items-center justify-center border-t border-border/50 p-5 sm:border-l sm:border-t-0">
                  <div className="flex flex-col items-center gap-2">
                    <div className="flex h-20 w-20 items-center justify-center rounded-xl bg-secondary">
                      <QrCode className="h-10 w-10 text-muted-foreground" />
                    </div>
                    <p className="text-[10px] font-medium text-muted-foreground">{ticket.status}</p>
                  </div>
                </div>
              </div>
            </div>
          )})}
        </div>
      )}
    </div>
  )
}
