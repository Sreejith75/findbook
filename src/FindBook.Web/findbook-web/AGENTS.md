# AGENTS.md — Next.js Enterprise Agent Rules

> **MANDATORY READ-FIRST PROTOCOL**
> Before writing **any** code, the agent MUST read `node_modules/next/dist/docs/` for breaking-change notices.
> APIs, file conventions, and rendering behaviours differ from training data.
> Treat every Next.js API call as potentially deprecated until verified.

---

## 0. Non-Negotiable Ground Rules

1. **TypeScript strict mode only.** No `any`, no `@ts-ignore` unless paired with a comment block explaining exactly why it is unavoidable.
2. **App Router only.** The Pages Router is legacy. Do not create `pages/` directories.
3. **Server Components are the default.** Add `"use client"` only when browser APIs, event handlers, or React state/effects are genuinely needed.
4. **Zod for all validation.** Every form schema, API input, and environment variable must be validated through a Zod schema.
5. **React Hook Form for all forms.** Never use uncontrolled raw HTML forms or manual `useState` for form fields.
6. **No barrel-import anti-patterns.** Each `index.ts` barrel must re-export only from its own folder, never from sibling feature folders.
7. **Absolute imports via `@/`.** Never use relative `../../` paths beyond one level.
8. **Kebab-case file names, PascalCase component names.** (`user-profile.tsx` exports `UserProfile`).

---

## 1. Folder Structure

```
my-app/
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── release.yml
├── .husky/
│   ├── pre-commit            # lint-staged
│   └── commit-msg            # commitlint
├── public/
│   ├── fonts/
│   ├── images/
│   └── icons/
├── src/
│   ├── app/                  # Next.js App Router — routing only
│   │   ├── layout.tsx
│   │   ├── page.tsx
│   │   ├── not-found.tsx
│   │   ├── error.tsx
│   │   ├── loading.tsx
│   │   ├── (auth)/           # Route group — no URL segment
│   │   │   ├── login/
│   │   │   │   └── page.tsx
│   │   │   └── register/
│   │   │       └── page.tsx
│   │   ├── (dashboard)/
│   │   │   ├── layout.tsx
│   │   │   ├── dashboard/
│   │   │   │   └── page.tsx
│   │   │   └── settings/
│   │   │       └── page.tsx
│   │   └── api/              # Route Handlers
│   │       └── v1/
│   │           └── users/
│   │               └── route.ts
│   │
│   ├── components/           # All React components
│   │   ├── ui/               # Primitive, unstyled/base design-system atoms
│   │   │   ├── button/
│   │   │   │   ├── button.tsx
│   │   │   │   └── index.ts
│   │   │   ├── input/
│   │   │   │   ├── input.tsx
│   │   │   │   └── index.ts
│   │   │   └── modal/
│   │   │       ├── modal.tsx
│   │   │       └── index.ts
│   │   ├── shared/           # Cross-feature reusable components
│   │   │   ├── data-table/
│   │   │   │   ├── data-table.tsx
│   │   │   │   ├── data-table-columns.tsx
│   │   │   │   └── index.ts
│   │   │   ├── page-header/
│   │   │   │   ├── page-header.tsx
│   │   │   │   └── index.ts
│   │   │   └── empty-state/
│   │   │       ├── empty-state.tsx
│   │   │       └── index.ts
│   │   └── layouts/          # Shell layouts used inside app/
│   │       ├── root-layout/
│   │       │   ├── root-layout.tsx
│   │       │   └── index.ts
│   │       ├── auth-layout/
│   │       │   ├── auth-layout.tsx
│   │       │   └── index.ts
│   │       └── dashboard-layout/
│   │           ├── dashboard-layout.tsx
│   │           └── index.ts
│   │
│   ├── features/             # Feature-scoped modules (vertical slices)
│   │   └── users/
│   │       ├── components/   # Components used only by this feature
│   │       │   ├── user-form.tsx
│   │       │   └── user-card.tsx
│   │       ├── hooks/        # Feature-scoped hooks
│   │       │   └── use-user.ts
│   │       ├── services/     # Feature-scoped API calls
│   │       │   └── user.service.ts
│   │       ├── schemas/      # Zod schemas for this feature
│   │       │   └── user.schema.ts
│   │       ├── types/        # Feature-scoped TypeScript types
│   │       │   └── user.types.ts
│   │       └── index.ts      # Public API of this feature (barrel)
│   │
│   ├── hooks/                # Global, reusable custom hooks
│   │   ├── use-debounce.ts
│   │   ├── use-local-storage.ts
│   │   ├── use-media-query.ts
│   │   └── use-outside-click.ts
│   │
│   ├── lib/                  # Third-party SDK initialisation & wrappers
│   │   ├── axios.ts          # Axios instance with interceptors
│   │   ├── query-client.ts   # TanStack Query client
│   │   └── auth.ts           # Auth provider setup (e.g. NextAuth)
│   │
│   ├── services/             # Global data-access / API service layer
│   │   ├── api.service.ts    # Base HTTP service class
│   │   └── index.ts
│   │
│   ├── schemas/              # Global shared Zod schemas
│   │   ├── common.schema.ts  # e.g. pagination, id, date
│   │   └── env.schema.ts     # Environment variable validation
│   │
│   ├── types/                # Global TypeScript type declarations
│   │   ├── api.types.ts      # Generic API response shapes
│   │   ├── common.types.ts   # Shared primitives (ID, Nullable, etc.)
│   │   └── index.ts
│   │
│   ├── stores/               # Global state (Zustand / Redux Toolkit)
│   │   ├── auth.store.ts
│   │   └── ui.store.ts
│   │
│   ├── providers/            # React context providers
│   │   ├── query-provider.tsx
│   │   ├── theme-provider.tsx
│   │   └── app-providers.tsx # Composes all providers
│   │
│   ├── utils/                # Pure utility functions (no side-effects)
│   │   ├── format.ts
│   │   ├── cn.ts             # className merger (clsx + tailwind-merge)
│   │   └── validation.ts
│   │
│   ├── constants/            # App-wide constants
│   │   ├── routes.ts
│   │   └── config.ts
│   │
│   ├── styles/               # Global CSS / Tailwind base
│   │   └── globals.css
│   │
│   └── middleware.ts         # Next.js middleware (auth guards, redirects)
│
├── .env.example
├── .eslintrc.json
├── .prettierrc
├── commitlint.config.ts
├── jest.config.ts
├── next.config.ts
├── tailwind.config.ts
└── tsconfig.json
```

---

## 2. App Router Conventions

### Special Files (inside `app/`)

| File | Purpose |
|---|---|
| `layout.tsx` | Persistent shell — persists across navigations |
| `page.tsx` | Public route leaf — only file that makes a segment accessible |
| `loading.tsx` | Suspense boundary (shown while page/layout suspends) |
| `error.tsx` | Error boundary — must be a Client Component |
| `not-found.tsx` | Rendered by `notFound()` from `next/navigation` |
| `route.ts` | API Route Handler (GET, POST, PUT, DELETE …) |
| `middleware.ts` | Global request interceptor (must be at `src/` root) |

### Route Groups

Wrap folder names in `()` to group routes **without** affecting the URL:

```
app/
├── (auth)/       →  no URL prefix
│   ├── login/    →  /login
│   └── register/ →  /register
└── (dashboard)/  →  no URL prefix
    └── overview/ →  /overview
```

- Different route groups can have **different root layouts**.
- Navigating between different root layouts triggers a **full page reload** — by design.

### Dynamic Routes

```
app/
├── blog/
│   └── [slug]/        →  /blog/:slug
│       ├── page.tsx
│       └── [...rest]/ →  /blog/:slug/* (catch-all)
```

### Parallel Routes & Intercepting Routes

Use `@folder` for parallel slots and `(.)folder` / `(..)folder` for interception:

```
app/
├── @modal/            # Parallel route slot
│   └── photo/[id]/
│       └── page.tsx
└── dashboard/
    └── page.tsx       # Consumes @modal slot via layout props
```

---

## 3. Component Rules

### Classification

| Folder | Rule |
|---|---|
| `components/ui/` | Atomic design-system primitives. No business logic. Props-driven only. |
| `components/shared/` | Cross-feature composites. May use hooks. No direct API calls. |
| `components/layouts/` | Page shells. Import from `shared/` and `ui/`. |
| `features/<name>/components/` | Components used exclusively within that feature. |

### Server vs Client Component Decision Tree

```
Does the component need any of the following?
  - useState / useReducer / useEffect / useRef / useContext
  - onClick / onChange / DOM event handlers
  - Browser-only APIs (window, document, localStorage)
  - Third-party libraries that use the above

YES → "use client"   (place as deep in the tree as possible)
NO  → Server Component (default — no directive needed)
```

### File Anatomy (order matters)

```tsx
// 1. Directive (if client)
"use client";

// 2. External imports (alphabetical)
import { useState } from "react";
import { z } from "zod";

// 3. Internal absolute imports (alphabetical)
import { Button } from "@/components/ui/button";
import { cn } from "@/utils/cn";

// 4. Types
type Props = {
  title: string;
  className?: string;
};

// 5. Component definition
export function MyComponent({ title, className }: Props) {
  // 6. Hooks at the top
  const [open, setOpen] = useState(false);

  // 7. Derived values / handlers
  const handleClick = () => setOpen((prev) => !prev);

  // 8. Early returns / guards
  if (!title) return null;

  // 9. JSX
  return (
    <div className={cn("base-class", className)}>
      <h1>{title}</h1>
    </div>
  );
}
```

### Naming Rules

- **File:** `kebab-case.tsx` → `user-profile.tsx`
- **Component export:** `PascalCase` → `export function UserProfile`
- **Props type:** `ComponentNameProps` → `UserProfileProps`
- **Default exports:** Allowed only for Next.js special files (`page.tsx`, `layout.tsx`, `error.tsx`, `loading.tsx`). All other components use **named exports**.

---

## 4. TypeScript Types

### Where to Put Types

| Location | What goes there |
|---|---|
| `src/types/api.types.ts` | Generic API response wrappers (`ApiResponse<T>`, `PaginatedResponse<T>`) |
| `src/types/common.types.ts` | Shared primitive aliases (`ID`, `Nullable<T>`, `Optional<T>`) |
| `src/features/<name>/types/` | Types used only within that feature |
| Inline in component file | Types for props that are used only in that file |

### Standard API Types

```ts
// src/types/api.types.ts
export type ApiResponse<T> = {
  data: T;
  message: string;
  success: boolean;
};

export type PaginatedResponse<T> = ApiResponse<{
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}>;

export type ApiError = {
  message: string;
  code: string;
  statusCode: number;
};
```

### Common Utility Types

```ts
// src/types/common.types.ts
export type ID = string;
export type Nullable<T> = T | null;
export type Optional<T> = T | undefined;
export type MaybePromise<T> = T | Promise<T>;
export type WithClassName = { className?: string };
```

### Rules

- Prefer `type` over `interface` for object shapes (use `interface` only when you need `extends` or `implements`).
- Always infer types from Zod schemas (`z.infer<typeof schema>`). Do not duplicate schemas as manual types.
- Never use `any`. Use `unknown` and narrow with type guards.
- Use `satisfies` operator to validate object literals against a type without widening.

---

## 5. Zod Schemas

### File Location

- Global / shared schemas → `src/schemas/`
- Feature-specific schemas → `src/features/<name>/schemas/`

### Schema-First Workflow

```ts
// src/features/users/schemas/user.schema.ts
import { z } from "zod";

export const createUserSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  email: z.string().email("Invalid email address"),
  role: z.enum(["admin", "user", "viewer"]),
  age: z.number().int().min(18).optional(),
});

// Derive TypeScript types from schema — never duplicate manually
export type CreateUserInput = z.infer<typeof createUserSchema>;

// Compose schemas for related shapes
export const updateUserSchema = createUserSchema.partial().extend({
  id: z.string().uuid(),
});
export type UpdateUserInput = z.infer<typeof updateUserSchema>;
```

### Environment Validation

```ts
// src/schemas/env.schema.ts
import { z } from "zod";

const envSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]),
  NEXT_PUBLIC_API_URL: z.string().url(),
  DATABASE_URL: z.string().min(1),
  NEXTAUTH_SECRET: z.string().min(32),
});

export const env = envSchema.parse(process.env);
```

Import `env` instead of `process.env` throughout the codebase.

---

## 6. Forms with React Hook Form + Zod

### Installation

```bash
pnpm add react-hook-form @hookform/resolvers zod
```

### Global `useAppForm` Hook (wrap once, reuse everywhere)

```ts
// src/hooks/use-app-form.ts
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, type UseFormProps } from "react-hook-form";
import type { z, ZodTypeAny } from "zod";

type UseAppFormOptions<TSchema extends ZodTypeAny> = {
  schema: TSchema;
} & Omit<UseFormProps<z.infer<TSchema>>, "resolver">;

export function useAppForm<TSchema extends ZodTypeAny>({
  schema,
  ...rest
}: UseAppFormOptions<TSchema>) {
  return useForm<z.infer<TSchema>>({
    resolver: zodResolver(schema),
    mode: "onTouched",   // validate on blur, re-validate on change after first blur
    ...rest,
  });
}
```

### Form Component Pattern

```tsx
// src/features/users/components/user-form.tsx
"use client";

import { useAppForm } from "@/hooks/use-app-form";
import { createUserSchema, type CreateUserInput } from "@/features/users/schemas/user.schema";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

type UserFormProps = {
  defaultValues?: Partial<CreateUserInput>;
  onSubmit: (data: CreateUserInput) => Promise<void>;
};

export function UserForm({ defaultValues, onSubmit }: UserFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useAppForm({
    schema: createUserSchema,
    defaultValues,
  });

  const handleFormSubmit = handleSubmit(async (data) => {
    await onSubmit(data);
    reset();
  });

  return (
    <form onSubmit={handleFormSubmit} noValidate>
      <Input
        id="name"
        label="Full Name"
        error={errors.name?.message}
        {...register("name")}
      />
      <Input
        id="email"
        type="email"
        label="Email"
        error={errors.email?.message}
        {...register("email")}
      />
      <Button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Saving…" : "Save"}
      </Button>
    </form>
  );
}
```

### Server Actions with RHF

```ts
// src/features/users/actions/create-user.ts
"use server";

import { createUserSchema } from "@/features/users/schemas/user.schema";

export async function createUserAction(formData: FormData) {
  const rawData = Object.fromEntries(formData.entries());
  const parsed = createUserSchema.safeParse(rawData);

  if (!parsed.success) {
    return { success: false, errors: parsed.error.flatten().fieldErrors };
  }

  // call service layer
  // await userService.create(parsed.data);
  return { success: true };
}
```

### Rules

- Always pass `noValidate` on `<form>` to let RHF / Zod own validation.
- Use `mode: "onTouched"` globally — validates on blur, re-validates on change once touched.
- Never store form data in global state. Form state lives in RHF.
- For multi-step forms, use a single `useForm` at the top-level and pass `control` down.
- Surface server-side errors with `form.setError("fieldName", { message: "…" })`.

---

## 7. Custom Hooks

### Location

- Reusable across features → `src/hooks/`
- Feature-specific → `src/features/<name>/hooks/`

### Naming Convention

- Always prefix with `use`: `useDebounce`, `useUser`, `usePagination`
- File name matches hook name: `use-debounce.ts`

### Rules & Patterns

```ts
// src/hooks/use-debounce.ts
import { useEffect, useState } from "react";

export function useDebounce<T>(value: T, delay = 300): T {
  const [debounced, setDebounced] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);

  return debounced;
}
```

```ts
// src/hooks/use-outside-click.ts
import { type RefObject, useEffect } from "react";

export function useOutsideClick<T extends HTMLElement>(
  ref: RefObject<T>,
  handler: () => void,
): void {
  useEffect(() => {
    const listener = (event: MouseEvent | TouchEvent) => {
      if (!ref.current || ref.current.contains(event.target as Node)) return;
      handler();
    };

    document.addEventListener("mousedown", listener);
    document.addEventListener("touchstart", listener);
    return () => {
      document.removeEventListener("mousedown", listener);
      document.removeEventListener("touchstart", listener);
    };
  }, [ref, handler]);
}
```

- Hooks must be **pure functions with no side-effects outside `useEffect`**.
- Return an object `{}` for multiple values, a single value for simple hooks.
- Document complex hooks with a JSDoc comment block.

---

## 8. Services (Data Access Layer)

### Purpose

Services are responsible for **all HTTP communication and data-fetching logic**. Components and hooks never call `fetch` directly.

### Base Service

```ts
// src/services/api.service.ts
import axios, { type AxiosInstance, type AxiosRequestConfig } from "axios";
import { env } from "@/schemas/env.schema";

class ApiService {
  private readonly http: AxiosInstance;

  constructor() {
    this.http = axios.create({
      baseURL: env.NEXT_PUBLIC_API_URL,
      headers: { "Content-Type": "application/json" },
    });

    // Request interceptor — attach auth token
    this.http.interceptors.request.use((config) => {
      // attach Bearer token from session here if needed
      return config;
    });

    // Response interceptor — normalise errors
    this.http.interceptors.response.use(
      (response) => response,
      (error) => Promise.reject(error?.response?.data ?? error),
    );
  }

  get<T>(url: string, config?: AxiosRequestConfig) {
    return this.http.get<T>(url, config);
  }

  post<T>(url: string, data: unknown, config?: AxiosRequestConfig) {
    return this.http.post<T>(url, data, config);
  }

  put<T>(url: string, data: unknown, config?: AxiosRequestConfig) {
    return this.http.put<T>(url, data, config);
  }

  patch<T>(url: string, data: unknown, config?: AxiosRequestConfig) {
    return this.http.patch<T>(url, data, config);
  }

  delete<T>(url: string, config?: AxiosRequestConfig) {
    return this.http.delete<T>(url, config);
  }
}

export const apiService = new ApiService();
```

### Feature Service

```ts
// src/features/users/services/user.service.ts
import { apiService } from "@/services/api.service";
import type { ApiResponse, PaginatedResponse } from "@/types/api.types";
import type { CreateUserInput, UpdateUserInput } from "@/features/users/schemas/user.schema";
import type { User } from "@/features/users/types/user.types";

const BASE = "/v1/users";

export const userService = {
  getAll: (params?: { page?: number; pageSize?: number }) =>
    apiService.get<PaginatedResponse<User>>(BASE, { params }),

  getById: (id: string) =>
    apiService.get<ApiResponse<User>>(`${BASE}/${id}`),

  create: (data: CreateUserInput) =>
    apiService.post<ApiResponse<User>>(BASE, data),

  update: (id: string, data: UpdateUserInput) =>
    apiService.put<ApiResponse<User>>(`${BASE}/${id}`, data),

  remove: (id: string) =>
    apiService.delete<ApiResponse<void>>(`${BASE}/${id}`),
};
```

### Rules

- Services must be **stateless plain objects or classes with no internal state**.
- Services live in `src/features/<name>/services/` or `src/services/` for cross-feature concerns.
- Do not call services directly in Server Components — pass their results as props or use them in Server Actions.
- For Client Components, consume services through TanStack Query hooks.

---

## 9. Data Fetching Patterns

### Server Component (preferred for initial data)

```tsx
// app/(dashboard)/users/page.tsx
import { userService } from "@/features/users/services/user.service";
import { UserList } from "@/features/users/components/user-list";

export default async function UsersPage() {
  const { data } = await userService.getAll();
  return <UserList users={data.data.items} />;
}
```

### TanStack Query (client-side / mutations)

```ts
// src/features/users/hooks/use-users.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { userService } from "@/features/users/services/user.service";
import type { CreateUserInput } from "@/features/users/schemas/user.schema";

// Query keys — centralise to avoid typos
export const userKeys = {
  all: ["users"] as const,
  list: (params?: object) => [...userKeys.all, "list", params] as const,
  detail: (id: string) => [...userKeys.all, "detail", id] as const,
};

export function useUsers(params?: { page: number; pageSize: number }) {
  return useQuery({
    queryKey: userKeys.list(params),
    queryFn: () => userService.getAll(params).then((r) => r.data),
  });
}

export function useCreateUser() {
  const client = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateUserInput) =>
      userService.create(data).then((r) => r.data),
    onSuccess: () => client.invalidateQueries({ queryKey: userKeys.all }),
  });
}
```

---

## 10. Environment Variables

| Prefix | Accessible in |
|---|---|
| `NEXT_PUBLIC_*` | Client + Server |
| *(no prefix)* | Server only |

- Validate all env vars with the `env.schema.ts` Zod schema at startup.
- Import `env` from `@/schemas/env.schema`, never from `process.env` directly.
- Never expose secret keys through `NEXT_PUBLIC_` variables.
- Commit `.env.example` with placeholder values. Never commit `.env`.

---

## 11. API Route Handlers

```ts
// src/app/api/v1/users/route.ts
import { NextRequest, NextResponse } from "next/server";
import { createUserSchema } from "@/features/users/schemas/user.schema";
import { userService } from "@/features/users/services/user.service";

export async function GET(request: NextRequest) {
  const { searchParams } = request.nextUrl;
  const page = Number(searchParams.get("page") ?? 1);
  const { data } = await userService.getAll({ page });
  return NextResponse.json(data);
}

export async function POST(request: NextRequest) {
  const body = await request.json();
  const parsed = createUserSchema.safeParse(body);

  if (!parsed.success) {
    return NextResponse.json(
      { message: "Validation failed", errors: parsed.error.flatten() },
      { status: 422 },
    );
  }

  const { data } = await userService.create(parsed.data);
  return NextResponse.json(data, { status: 201 });
}
```

- Always validate request bodies with `schema.safeParse()`.
- Return consistent `{ message, data?, errors? }` shapes.
- Use HTTP status codes correctly: `200`, `201`, `400`, `401`, `403`, `404`, `422`, `500`.

---

## 12. Middleware

```ts
// src/middleware.ts
import { NextRequest, NextResponse } from "next/server";

const PUBLIC_ROUTES = ["/login", "/register", "/"];

export function middleware(request: NextRequest) {
  const token = request.cookies.get("auth-token");
  const { pathname } = request.nextUrl;

  const isPublic = PUBLIC_ROUTES.some((r) => pathname.startsWith(r));
  if (!isPublic && !token) {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico|public/).*)"],
};
```

---

## 13. State Management

- **Server state** → TanStack Query (`@tanstack/react-query`)
- **Client / UI state** → Zustand (`zustand`)
- **Form state** → React Hook Form (never put form data in a store)
- **URL state** → `useSearchParams` / `useRouter`

```ts
// src/stores/ui.store.ts
import { create } from "zustand";

type UIStore = {
  sidebarOpen: boolean;
  toggleSidebar: () => void;
};

export const useUIStore = create<UIStore>((set) => ({
  sidebarOpen: false,
  toggleSidebar: () =>
    set((state) => ({ sidebarOpen: !state.sidebarOpen })),
}));
```

---

## 14. Code Quality & Tooling

### ESLint (`.eslintrc.json`)

Extend:
- `next/core-web-vitals`
- `@typescript-eslint/recommended-type-checked`
- `import/recommended`

### Prettier (`.prettierrc`)

```json
{
  "semi": true,
  "singleQuote": false,
  "trailingComma": "all",
  "printWidth": 100,
  "tabWidth": 2
}
```

### Husky + lint-staged

Run ESLint + Prettier + TypeScript type-check on every commit.

### Commitlint

Follow Conventional Commits: `feat:`, `fix:`, `chore:`, `refactor:`, `docs:`, `test:`.

---

## 15. Testing Strategy

| Layer | Tool |
|---|---|
| Unit (utils, hooks) | Jest + React Testing Library |
| Integration (pages, features) | Jest + MSW (mock service worker) |
| E2E | Playwright |

- Co-locate test files: `user-form.test.tsx` next to `user-form.tsx`.
- Mock API calls with MSW, never with `jest.mock("axios")`.
- Aim for 80%+ coverage on `services/`, `hooks/`, and `utils/`.

---

## 16. Performance Checklist

- [ ] Use `next/image` for all images — never raw `<img>`.
- [ ] Use `next/font` for all fonts — eliminates FOUT.
- [ ] Use `next/link` for all internal navigation — never `<a>`.
- [ ] Add `loading.tsx` to routes with async data fetching.
- [ ] Add `Suspense` boundaries around Client Component islands.
- [ ] Use `React.lazy` + `dynamic` with `{ ssr: false }` for heavy client-only components.
- [ ] Set correct `Cache-Control` headers on Route Handlers.
- [ ] Use `generateStaticParams` for known dynamic routes.
- [ ] Validate all images have `alt` text for accessibility.

---

*Last updated: 2025 — verify against `node_modules/next/dist/docs/` before acting.*