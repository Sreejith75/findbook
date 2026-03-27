import { z } from "zod";

export const envSchema = z.object({
  NEXT_PUBLIC_FINDBOOK_API_BASE_URL: z
    .string()
    .url("NEXT_PUBLIC_FINDBOOK_API_BASE_URL must be a valid URL")
    .default("https://localhost:57679"),
});

export type Env = z.infer<typeof envSchema>;
