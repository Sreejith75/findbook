@AGENTS.md

# CLAUDE.md — Claude-Specific Coding Instructions

> These rules are additive to `AGENTS.md`. Where both files cover the same topic, `AGENTS.md` takes precedence on architecture; this file governs **how Claude writes and refactors code** in practice.

---

## 1. Before Writing Any Code

1. **Read `node_modules/next/dist/docs/`** for the exact version in use. APIs change across minor releases.
2. **Read `AGENTS.md`** fully. All architecture, folder, and naming rules defined there are binding.
3. **Confirm the task scope.** Ask clarifying questions before generating large code blocks if the requirements are ambiguous.
4. **Check existing patterns first.** If a similar hook, service, or component already exists, extend or reuse it rather than creating a duplicate.

---

## 2. Code Generation Principles

### Always Produce Complete Files

- Output the **full file**, not code snippets. Partial files cause merge errors.
- Include every import. Do not use `// ... rest of imports` placeholders.
- Include every type annotation. No implicit `any`.

### Respect the Folder Contract

When creating a new file, place it exactly according to the rules in `AGENTS.md §1`:

| What you're creating | Correct location |
|---|---|
| Reusable UI atom | `src/components/ui/<name>/` |
| Cross-feature composite | `src/components/shared/<name>/` |
| Feature component | `src/features/<feature>/components/` |
| Global custom hook | `src/hooks/use-<name>.ts` |
| Feature hook | `src/features/<feature>/hooks/use-<name>.ts` |
| Feature service | `src/features/<feature>/services/<name>.service.ts` |
| Feature Zod schema | `src/features/<feature>/schemas/<name>.schema.ts` |
| Feature types | `src/features/<feature>/types/<name>.types.ts` |
| Route page | `src/app/<route-group>/<segment>/page.tsx` |
| Route layout | `src/app/<route-group>/<segment>/layout.tsx` |
| API handler | `src/app/api/v1/<resource>/route.ts` |

---

## 3. Component Scaffolding Template

When asked to create a **new component**, always follow this exact template:

```tsx
// src/components/shared/page-header/page-header.tsx
// OR src/features/<feature>/components/<name>.tsx

import { cn } from "@/utils/cn";
// ... other imports

// ── Types ─────────────────────────────────────────────
type PageHeaderProps = {
  title: string;
  description?: string;
  className?: string;
  children?: React.ReactNode;
};

// ── Component ─────────────────────────────────────────
export function PageHeader({
  title,
  description,
  className,
  children,
}: PageHeaderProps) {
  return (
    <header className={cn("mb-6 flex items-center justify-between", className)}>
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
        {description && (
          <p className="mt-1 text-sm text-muted-foreground">{description}</p>
        )}
      </div>
      {children}
    </header>
  );
}

// ── Barrel export ──────────────────────────────────────
// Add to: src/components/shared/page-header/index.ts
// export { PageHeader } from "./page-header";
```

---

## 4. Hook Scaffolding Template

```ts
// src/hooks/use-<name>.ts
// OR src/features/<feature>/hooks/use-<name>.ts

import { useState, useEffect, useCallback } from "react";

// ── Types ─────────────────────────────────────────────
type UseDebounceReturn<T> = T;

// ── Hook ──────────────────────────────────────────────
/**
 * Debounces a value by the given delay.
 * @param value - The value to debounce.
 * @param delay - Debounce delay in ms. Defaults to 300.
 */
export function useDebounce<T>(value: T, delay = 300): UseDebounceReturn<T> {
  const [debounced, setDebounced] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debounced;
}
```

---

## 5. Service Scaffolding Template

```ts
// src/features/<feature>/services/<resource>.service.ts

import { apiService } from "@/services/api.service";
import type { ApiResponse, PaginatedResponse } from "@/types/api.types";
import type { CreateXInput, UpdateXInput } from "@/features/<feature>/schemas/<resource>.schema";
import type { X } from "@/features/<feature>/types/<resource>.types";

const BASE = "/v1/<resources>";

export const xService = {
  /**
   * Fetch paginated list of X.
   */
  getAll: (params?: { page?: number; pageSize?: number }) =>
    apiService.get<PaginatedResponse<X>>(BASE, { params }),

  /**
   * Fetch a single X by ID.
   */
  getById: (id: string) =>
    apiService.get<ApiResponse<X>>(`${BASE}/${id}`),

  /**
   * Create a new X.
   */
  create: (data: CreateXInput) =>
    apiService.post<ApiResponse<X>>(BASE, data),

  /**
   * Update an existing X.
   */
  update: (id: string, data: UpdateXInput) =>
    apiService.put<ApiResponse<X>>(`${BASE}/${id}`, data),

  /**
   * Delete an X by ID.
   */
  remove: (id: string) =>
    apiService.delete<ApiResponse<void>>(`${BASE}/${id}`),
};
```

---

## 6. Schema + Type Scaffolding Template

```ts
// src/features/<feature>/schemas/<resource>.schema.ts
import { z } from "zod";

export const create<Resource>Schema = z.object({
  // Define fields with descriptive error messages
  name: z.string().min(2, "Name must be at least 2 characters"),
  email: z.string().email("Please enter a valid email"),
});

export const update<Resource>Schema = create<Resource>Schema.partial().extend({
  id: z.string().uuid("ID must be a valid UUID"),
});

// Always export types inferred from schema — NEVER define them manually
export type Create<Resource>Input = z.infer<typeof create<Resource>Schema>;
export type Update<Resource>Input = z.infer<typeof update<Resource>Schema>;
```

```ts
// src/features/<feature>/types/<resource>.types.ts
import type { ID } from "@/types/common.types";

// Types that are NOT derivable from a Zod schema (e.g. server response shapes)
export type <Resource> = {
  id: ID;
  name: string;
  email: string;
  createdAt: string;
  updatedAt: string;
};
```

---

## 7. Form Scaffolding Template

```tsx
// src/features/<feature>/components/<resource>-form.tsx
"use client";

import { useAppForm } from "@/hooks/use-app-form";
import {
  create<Resource>Schema,
  type Create<Resource>Input,
} from "@/features/<feature>/schemas/<resource>.schema";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

// ── Types ─────────────────────────────────────────────
type <Resource>FormProps = {
  defaultValues?: Partial<Create<Resource>Input>;
  onSubmit: (data: Create<Resource>Input) => Promise<void>;
  isLoading?: boolean;
};

// ── Component ─────────────────────────────────────────
export function <Resource>Form({
  defaultValues,
  onSubmit,
  isLoading,
}: <Resource>FormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useAppForm({
    schema: create<Resource>Schema,
    defaultValues,
  });

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
    reset();
  });

  const isPending = isSubmitting || isLoading;

  return (
    <form onSubmit={handleFormSubmit} noValidate className="space-y-4">
      <Input
        id="name"
        label="Name"
        error={errors.name?.message}
        disabled={isPending}
        {...register("name")}
      />
      <Input
        id="email"
        type="email"
        label="Email"
        error={errors.email?.message}
        disabled={isPending}
        {...register("email")}
      />
      <Button type="submit" disabled={isPending}>
        {isPending ? "Saving…" : "Save"}
      </Button>
    </form>
  );
}
```

---

## 8. Page Scaffolding Template

```tsx
// src/app/(dashboard)/<segment>/page.tsx

import { Suspense } from "react";
import { PageHeader } from "@/components/shared/page-header";
import { <Resource>List } from "@/features/<feature>/components/<resource>-list";
import { <Resource>ListSkeleton } from "@/features/<feature>/components/<resource>-list-skeleton";
import { <resource>Service } from "@/features/<feature>/services/<resource>.service";
import type { Metadata } from "next";

// ── Metadata ───────────────────────────────────────────
export const metadata: Metadata = {
  title: "<Page Title>",
  description: "<Page description for SEO>",
};

// ── Page ───────────────────────────────────────────────
export default async function <Resource>Page() {
  // Data fetching belongs in Server Components — never in useEffect
  const { data } = await <resource>Service.getAll();

  return (
    <main className="container py-6">
      <PageHeader
        title="<Title>"
        description="<Subtitle>"
      />
      <Suspense fallback={<<Resource>ListSkeleton />}>
        <<Resource>List items={data.data.items} />
      </Suspense>
    </main>
  );
}
```

---

## 9. TanStack Query Hook Template

```ts
// src/features/<feature>/hooks/use-<resource>.ts
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { <resource>Service } from "@/features/<feature>/services/<resource>.service";
import type { Create<Resource>Input } from "@/features/<feature>/schemas/<resource>.schema";

// ── Query Keys ─────────────────────────────────────────
export const <resource>Keys = {
  all: ["<resources>"] as const,
  list: (params?: object) => [...<resource>Keys.all, "list", params] as const,
  detail: (id: string) => [...<resource>Keys.all, "detail", id] as const,
};

// ── Queries ────────────────────────────────────────────
export function use<Resource>s(params?: { page?: number; pageSize?: number }) {
  return useQuery({
    queryKey: <resource>Keys.list(params),
    queryFn: () => <resource>Service.getAll(params).then((r) => r.data),
  });
}

export function use<Resource>(id: string) {
  return useQuery({
    queryKey: <resource>Keys.detail(id),
    queryFn: () => <resource>Service.getById(id).then((r) => r.data),
    enabled: !!id,
  });
}

// ── Mutations ──────────────────────────────────────────
export function useCreate<Resource>() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data: Create<Resource>Input) =>
      <resource>Service.create(data).then((r) => r.data),
    onSuccess: () =>
      client.invalidateQueries({ queryKey: <resource>Keys.all }),
  });
}

export function useDelete<Resource>() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (id: string) =>
      <resource>Service.remove(id).then((r) => r.data),
    onSuccess: () =>
      client.invalidateQueries({ queryKey: <resource>Keys.all }),
  });
}
```

---

## 10. Feature Barrel Export Template

```ts
// src/features/<feature>/index.ts
// Public API of this feature — import ONLY from this file in other features or pages

// Components
export { <Resource>Form } from "./components/<resource>-form";
export { <Resource>List } from "./components/<resource>-list";
export { <Resource>Card } from "./components/<resource>-card";

// Hooks
export {
  use<Resource>s,
  use<Resource>,
  useCreate<Resource>,
  useDelete<Resource>,
  <resource>Keys,
} from "./hooks/use-<resource>";

// Service
export { <resource>Service } from "./services/<resource>.service";

// Schemas & Types
export {
  create<Resource>Schema,
  update<Resource>Schema,
  type Create<Resource>Input,
  type Update<Resource>Input,
} from "./schemas/<resource>.schema";

export type { <Resource> } from "./types/<resource>.types";
```

---

## 11. What Claude Must Never Do

| Anti-Pattern | Why |
|---|---|
| Use `any` type | Defeats TypeScript's purpose; use `unknown` + narrowing |
| Fetch inside `useEffect` | Use TanStack Query or Server Component data fetching |
| Put `"use client"` at layout level | Converts the entire subtree to client-side rendering |
| Define types that duplicate Zod schemas | Single source of truth; always use `z.infer<>` |
| Use relative imports `../../` beyond one level | Use `@/` absolute imports |
| Create components without exporting from barrel | Breaks the public API contract |
| Call `process.env` directly in code | Import from `@/schemas/env.schema` |
| Use default exports for shared components | Named exports are required for tree-shaking and discoverability |
| Mix business logic in `page.tsx` | Pages are thin — delegate to feature components and services |
| Skip `loading.tsx` on async routes | Always add a loading UI to avoid layout shift |
| Create a new `utils.ts` mega-file | Break utilities into purpose-named files: `format.ts`, `cn.ts`, etc. |
| Use `<img>` for images | Always use `next/image` |
| Use `<a>` for internal links | Always use `next/link` |

---

## 12. Refactoring Checklist

When refactoring existing code, verify:

- [ ] All types are inferred from Zod (`z.infer`), not hand-written duplicates.
- [ ] `"use client"` is moved as deep in the component tree as possible.
- [ ] Repeated logic is extracted to a hook in `src/hooks/` or `src/features/<n>/hooks/`.
- [ ] Repeated JSX patterns are extracted to a component in the appropriate folder.
- [ ] Services are stateless and all HTTP calls go through `apiService`.
- [ ] All environment variables are accessed via `env.*` from `env.schema.ts`.
- [ ] All forms use `useAppForm` + Zod schema.
- [ ] `index.ts` barrels are updated when new exports are added.
- [ ] No relative imports go more than one level up (`../` once is acceptable, `../../` is not).

---

## 13. Quick-Reference Commands

```bash
# Bootstrap a new Next.js project
pnpm create next-app@latest my-app --typescript --tailwind --eslint --app --src-dir --import-alias "@/*"

# Install core dependencies
pnpm add zod react-hook-form @hookform/resolvers
pnpm add @tanstack/react-query axios zustand
pnpm add clsx tailwind-merge

# Install dev dependencies
pnpm add -D @typescript-eslint/parser @typescript-eslint/eslint-plugin \
  eslint-plugin-import eslint-config-prettier prettier \
  husky lint-staged @commitlint/cli @commitlint/config-conventional \
  jest @testing-library/react @testing-library/jest-dom msw \
  @playwright/test
```

---

*This file is a companion to `AGENTS.md`. Both must be read before any code is generated.*