"use client";

import {
  forwardRef,
  type TextareaHTMLAttributes,
} from "react";

import { cn } from "@/utils/cn";

type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
  error?: string;
  label?: string;
};

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  function Textarea({ className, error, id, label, ...props }, ref) {
    return (
      <label className="field">
        {label ? <span className="field-label">{label}</span> : null}
        <textarea
          className={cn("field-input field-textarea", error && "field-input-error", className)}
          id={id}
          ref={ref}
          {...props}
        />
        {error ? <span className="field-error">{error}</span> : null}
      </label>
    );
  },
);
