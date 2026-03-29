"use client";

import { useEffect } from "react";

import { Button } from "@/components/ui/button";

type ErrorPageProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function GlobalError({ error, reset }: ErrorPageProps) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  const isBackendUnavailable =
    error.message.includes("ECONNREFUSED") ||
    error.message.includes("Request to") ||
    error.message.includes("fetch failed");

  return (
    <main className="auth-page-shell">
      <section className="auth-page-panel">
        <div className="hero-panel-eyebrow">Application Error</div>
        <h1 className="hero-panel-title">
          {isBackendUnavailable
            ? "The FindBook API is not reachable right now."
            : "Something went wrong while loading the application."}
        </h1>
        <p className="hero-panel-copy">
          {isBackendUnavailable
            ? "Start the ASP.NET backend on https://localhost:57679 and try again."
            : "Refresh the page or retry once the underlying issue is resolved."}
        </p>
        <div className="hero-panel-actions">
          <Button onClick={reset} type="button">
            Try Again
          </Button>
        </div>
      </section>
    </main>
  );
}
