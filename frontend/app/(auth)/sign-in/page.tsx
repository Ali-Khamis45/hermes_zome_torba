import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

/**
 * Sign-in placeholder. Full JWT + refresh-token flow lands in Phase 1 per
 * docs/11-security.md#authn--authz and docs/18-roadmap.md.
 */
export default function SignInPage() {
  return (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Sign in to Hermes Zone Torba</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <p className="text-sm text-muted-foreground">
          Authentication (JWT + refresh tokens, RBAC) is implemented in Phase 1 — see
          docs/11-security.md.
        </p>
        <Button className="w-full" disabled>
          Continue
        </Button>
      </CardContent>
    </Card>
  );
}
