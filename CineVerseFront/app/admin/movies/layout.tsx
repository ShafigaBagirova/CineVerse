/** `/admin/movies` must not be statically cached (dynamic list UI). */
export const dynamic = "force-dynamic"
export const revalidate = 0

export default function AdminMoviesLayout({ children }: { children: React.ReactNode }) {
  return children
}
