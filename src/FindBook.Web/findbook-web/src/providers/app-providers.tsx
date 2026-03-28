"use client";

import type { ReactNode } from "react";

import { FirebaseProvider } from "@/providers/firebase-provider";

type AppProvidersProps = {
  children: ReactNode;
};

export function AppProviders({ children }: AppProvidersProps) {
  return <FirebaseProvider>{children}</FirebaseProvider>;
}
