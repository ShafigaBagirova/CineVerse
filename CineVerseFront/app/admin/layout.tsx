import { AdminPanelLayout } from "@/components/admin/admin-panel-layout"

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return <AdminPanelLayout>{children}</AdminPanelLayout>
}
