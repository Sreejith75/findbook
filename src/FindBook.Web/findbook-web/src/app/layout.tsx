import type { Metadata } from "next";

import "primeicons/primeicons.css";
import "primereact/resources/themes/lara-light-amber/theme.css";
import "primereact/resources/primereact.min.css";

import { AppProviders } from "@/providers/app-providers";
import "@/styles/globals.css";

export const metadata: Metadata = {
  title: "FindBook",
  description: "Book rental and delivery platform.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>
        <AppProviders>{children}</AppProviders>
      </body>
    </html>
  );
}
