"use client";

import {
  forwardRef,
  type InputHTMLAttributes,
} from "react";

import { cn } from "@/utils/cn";

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  error?: string;
  label?: string;
};

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { className, error, id, label, ...props },
  ref,
) {
  return (
    <label className="field">
      {label ? <span className="field-label">{label}</span> : null}
      <input
        className={cn("field-input", error && "field-input-error", className)}
        id={id}
        ref={ref}
        {...props}
      />
      {error ? <span className="field-error">{error}</span> : null}
    </label>
  );
});
