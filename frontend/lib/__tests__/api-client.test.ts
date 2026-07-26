import { afterEach, describe, expect, it, vi } from "vitest";
import { apiClient, ApiError } from "@/lib/api-client";

describe("apiClient", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("returns parsed JSON on a successful response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(JSON.stringify({ hello: "world" }), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      ),
    );

    const result = await apiClient.get<{ hello: string }>("/api/v1/anything");

    expect(result).toEqual({ hello: "world" });
  });

  it("throws ApiError with the parsed Problem Details on a non-2xx response", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            type: "https://docs.hzt.dev/errors/409",
            title: "Domain rule violated",
            status: 409,
            detail: "Instance is already running.",
            instance: "/api/v1/hermes/123/status",
          }),
          { status: 409, headers: { "Content-Type": "application/problem+json" } },
        ),
      ),
    );

    await expect(apiClient.get("/api/v1/hermes/123/status")).rejects.toBeInstanceOf(ApiError);
  });
});
