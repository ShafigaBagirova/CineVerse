import { Search, Ticket, Popcorn, Users } from "lucide-react"

const steps = [
  {
    icon: Search,
    title: "Discover",
    description: "Browse trending films, get personalized recommendations, and explore curated collections.",
  },
  {
    icon: Ticket,
    title: "Book",
    description: "Find showtimes near you, pick your seats, and pre-order snacks for a seamless experience.",
  },
  {
    icon: Popcorn,
    title: "Experience",
    description: "Enjoy the show with reserved seats, food ready on arrival, and a premium cinema experience.",
  },
  {
    icon: Users,
    title: "Connect",
    description: "Rate films, share reviews, follow friends, and discover your taste compatibility.",
  },
]

export function HowItWorks() {
  return (
    <section className="border-t border-border/50 bg-secondary/30 py-20">
      <div className="mx-auto max-w-7xl px-4 lg:px-8">
        <div className="mb-14 text-center">
          <p className="mb-2 text-xs font-medium uppercase tracking-[0.2em] text-primary">
            How It Works
          </p>
          <h2 className="font-serif text-3xl font-bold text-foreground md:text-4xl">
            Your Cinema, Simplified
          </h2>
        </div>

        <div className="grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
          {steps.map((step, index) => {
            const Icon = step.icon
            return (
              <div
                key={step.title}
                className="group relative rounded-2xl border border-border/50 bg-card p-6 transition-all hover:border-primary/30 hover:shadow-lg hover:shadow-primary/5"
              >
                <div className="mb-1 text-xs font-bold text-primary/60">
                  {String(index + 1).padStart(2, "0")}
                </div>
                <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-primary/10">
                  <Icon className="h-6 w-6 text-primary" />
                </div>
                <h3 className="mb-2 text-lg font-semibold text-foreground">{step.title}</h3>
                <p className="text-sm leading-relaxed text-muted-foreground">{step.description}</p>
              </div>
            )
          })}
        </div>
      </div>
    </section>
  )
}
