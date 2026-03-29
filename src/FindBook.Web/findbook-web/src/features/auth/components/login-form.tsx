"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAppForm } from "@/hooks/use-app-form";
import { loginFormSchema, type LoginFormInput } from "@/features/auth/schemas/login-form.schema";
import { useAuth } from "@/hooks/use-auth";

export function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { signIn } = useAuth();
  const [serverError, setServerError] = useState<string | null>(null);
  const form = useAppForm<LoginFormInput>({
    defaultValues: {
      email: "",
      password: "",
    },
    schema: loginFormSchema,
  });

  const submit = form.handleSubmit(async (values) => {
    setServerError(null);

    try {
      await signIn(values.email, values.password);
      router.replace(searchParams.get("returnTo") || "/");
      router.refresh();
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to sign in.");
    }
  });

  return (
    <form className="stack-form auth-form-card" onSubmit={submit}>
      <div className="auth-form-header">
        <div className="auth-form-title">Welcome back</div>
        <div className="auth-form-copy">Pick up where you left off.</div>
      </div>
      <Input
        autoComplete="email"
        error={form.formState.errors.email?.message}
        label="Email"
        {...form.register("email")}
      />
      <Input
        autoComplete="current-password"
        error={form.formState.errors.password?.message}
        label="Password"
        type="password"
        {...form.register("password")}
      />
      {serverError ? <div className="form-alert">{serverError}</div> : null}
      <Button disabled={form.formState.isSubmitting} type="submit">
        {form.formState.isSubmitting ? "Signing In..." : "Sign In"}
      </Button>
      <div className="auth-form-footnote">
        Access your account to continue reading, renting, and managing returns.
      </div>
    </form>
  );
}
