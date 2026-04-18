import { HeroSection } from "@/components/landing/hero-section"
import { TrendingSection } from "@/components/landing/trending-section"
import { FeedSection } from "@/components/landing/feed-section"
import { HowItWorks } from "@/components/landing/how-it-works"

export default function LandingPage() {
  return (
    <>
      <HeroSection />
      <TrendingSection />
      <FeedSection />
      <HowItWorks />
    </>
  )
}
