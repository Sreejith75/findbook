import { envSchema, type Env } from "@/schemas/env.schema";

let cachedEnv: Env | null = null;

export function getEnv(): Env {
  if (cachedEnv) {
    return cachedEnv;
  }

  cachedEnv = envSchema.parse({
    NEXT_PUBLIC_FINDBOOK_API_BASE_URL:
      process.env.NEXT_PUBLIC_FINDBOOK_API_BASE_URL,
  });

  return cachedEnv;
}
