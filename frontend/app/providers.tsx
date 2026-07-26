"use client";

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { TooltipProvider } from "@/components/ui/tooltip";
import { useState } from "react";

/**
 * Client-side provider tree: React Query for server state (see lib/hooks/*) and the shadcn/ui
 * TooltipProvider. Kept as a single "use client" boundary at the top of the tree so the rest of
 * app/layout.tsx can stay a server component. See docs/02-folder-structure.md#frontend.
 */
export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 30_000,
            refetchOnWindowFocus: false,
          },
        },
      }),
  );

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider delay={150}>{children}</TooltipProvider>
    </QueryClientProvider>
  );
}
