/**
 * Auth route group — deliberately has no dashboard chrome (Sidebar/Topbar), per
 * docs/02-folder-structure.md#frontend.
 */
export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return <div className="flex min-h-dvh items-center justify-center bg-muted/30 p-6">{children}</div>;
}
