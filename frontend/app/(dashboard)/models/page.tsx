import { Download } from "lucide-react";
import { Button } from "@/components/ui/button";

/**
 * Model Registry / Ollama Manager UI — see docs/07-local-llm.md. Backed by
 * GET/POST /api/v1/models once the Ollama Manager Application handlers ship in Phase 1
 * (docs/18-roadmap.md); this page is the layout placeholder that work lands into.
 */
export default function ModelsPage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Models</h1>
          <p className="text-sm text-muted-foreground">
            Download, benchmark, and switch local models across Ollama, LM Studio, and vLLM.
          </p>
        </div>
        <Button disabled>
          <Download className="size-4" />
          Download model
        </Button>
      </div>

      <div className="rounded-lg border border-dashed p-6 text-sm text-muted-foreground">
        The Ollama / LLM Manager backend (docs/07-local-llm.md) lands in Phase 1 — this page will
        list the Model Registry catalog and local pull state once that API exists.
      </div>
    </div>
  );
}
