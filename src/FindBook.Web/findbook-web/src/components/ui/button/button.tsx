import type { ButtonHTMLAttributes } from "react";

import { cn } from "@/utils/cn";

type ButtonVariant = "primary" | "outline" | "ghost";

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: ButtonVariant;
};

const variantClassNames: Record<ButtonVariant, string> = {
  primary: "button button-primary",
  outline: "button button-outline",
  ghost: "button button-ghost",
};

export function Button({
  className,
  variant = "primary",
  type = "button",
  ...props
}: ButtonProps) {
  return (
    <button
      className={cn(variantClassNames[variant], className)}
      type={type}
      {...props}
    />
  );
}
