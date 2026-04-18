/** @type {import('next').NextConfig} */
// Proxy API calls from the Next origin to the backend so browser requests stay same-origin
// and are not blocked by missing CORS headers on the API (matches NEXT_PUBLIC_API_BASE_URL in .env.local).
const backendBase = (process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://127.0.0.1:5073").replace(/\/$/, "")

const nextConfig = {
  typescript: {
    ignoreBuildErrors: true,
  },
  images: {
    unoptimized: true,
  },
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${backendBase}/api/:path*`,
      },
    ]
  },
}

export default nextConfig
