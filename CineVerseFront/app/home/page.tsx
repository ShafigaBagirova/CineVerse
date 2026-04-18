import { redirect } from "next/navigation"

/** Logged-in user landing alias; primary user home is `/profile`. */
export default function HomeAliasPage() {
  redirect("/profile")
}
