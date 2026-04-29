import { redirect } from "next/navigation"

export default function LegacyChatRoute() {
  redirect("/messages")
}
