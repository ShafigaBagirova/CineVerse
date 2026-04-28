import Link from "next/link"
import Image from "next/image"

export function HeroSection() {
  return (
    <section className="relative flex min-h-[85vh] items-center justify-center overflow-hidden">
      <Image
        src="/images/hero-bg.jpg"
        alt=""
        fill
        className="object-cover"
        priority
      />
      <div className="absolute inset-0 bg-background/70 backdrop-blur-[2px]" />
      <div className="absolute inset-0 bg-gradient-to-t from-background via-background/40 to-transparent" />
      {/* Pale moonlight green glow on right side */}
      <div className="absolute inset-0 bg-gradient-to-l from-[rgba(107,168,155,0.35)] via-[rgba(107,168,155,0.15)] to-transparent opacity-80" />
      <div className="absolute right-0 top-0 bottom-0 w-2/3 bg-gradient-to-r from-transparent via-transparent to-[rgba(107,168,155,0.4)] opacity-70" />
      {/* Shadow effect for depth */}
      <div className="absolute inset-0 bg-radial-shadow from-transparent via-transparent to-black/40" style={{backgroundImage: 'radial-gradient(ellipse at 80% 50%, transparent 0%, rgba(0,0,0,0.5) 100%)'}} />

      <div className="relative z-10 mx-auto max-w-4xl px-4 text-center">
        <p className="mb-4 text-sm font-medium uppercase tracking-[0.3em] text-primary">
          Your Cinematic Journey Starts Here
        </p>
        <h1 className="font-serif text-5xl font-bold leading-tight tracking-tight text-foreground md:text-7xl lg:text-8xl">
          <span className="text-balance">Discover. Watch. Experience.</span>
        </h1>
        <p className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-[#2C4A48]/80 md:text-xl">
          Track your favorite films, book cinema tickets, connect with fellow movie
          lovers, and unlock a premium cinematic experience.
        </p>
        <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
          <Link
            href="/movies"
            className="inline-flex h-12 items-center rounded-lg bg-primary px-8 text-sm font-semibold text-primary-foreground transition-all hover:bg-primary/90 hover:shadow-lg hover:shadow-primary/20"
          >
            Explore Movies
          </Link>
          <Link
            href="/profile"
            className="inline-flex h-12 items-center rounded-lg border border-border bg-secondary/50 px-8 text-sm font-semibold text-foreground transition-all hover:bg-secondary"
          >
            Sign Up Free
          </Link>
        </div>
      </div>
    </section>
  )
}
