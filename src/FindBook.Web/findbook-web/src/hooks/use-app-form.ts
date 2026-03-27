"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import {
  useForm,
  type DefaultValues,
  type FieldValues,
  type UseFormProps,
} from "react-hook-form";
import type { z } from "zod";

type UseAppFormOptions<
  TFieldValues extends FieldValues,
> = Omit<
  UseFormProps<TFieldValues>,
  "resolver"
> & {
  defaultValues?: DefaultValues<TFieldValues>;
  schema: z.ZodTypeAny;
};

export function useAppForm<
  TFieldValues extends FieldValues,
>({
  schema,
  ...options
}: UseAppFormOptions<TFieldValues>) {
  return useForm<TFieldValues>({
    ...options,
    resolver: zodResolver(schema as never) as never,
  });
}
