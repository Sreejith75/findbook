"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useAppForm } from "@/hooks/use-app-form";
import { reviewFormSchema, type ReviewFormInput } from "@/features/book-rental/schemas/book-rental-forms.schema";
import { createReview } from "@/features/book-rental/services/book-rental.client";
import type { RentalRecord } from "@/features/book-rental/types/book-rental.types";

type ReviewBookDialogProps = {
  bookId: number | null;
  onClose: () => void;
  rental: RentalRecord | null;
};

export function ReviewBookDialog({
  bookId,
  onClose,
  rental,
}: ReviewBookDialogProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [serverError, setServerError] = useState<string | null>(null);
  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
    reset,
  } = useAppForm({
    defaultValues: {
      content: "",
      rating: 5,
      title: "",
    },
    schema: reviewFormSchema,
  });

  if (!rental || !bookId) {
    return null;
  }

  const isBusy = isPending || isSubmitting;

  const onSubmit = handleSubmit(async (data) => {
    setServerError(null);

    try {
      await createReview(bookId, data as unknown as ReviewFormInput);

      startTransition(() => {
        reset();
        router.refresh();
        onClose();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to save the review.");
    }
  });

  return (
    <div className="dialog-backdrop" onClick={onClose}>
      <div className="dialog-card" onClick={(event) => event.stopPropagation()}>
        <div className="dialog-header">
          <div>
            <div className="dialog-title">Review {rental.title}</div>
            <div className="dialog-subtitle">Share your experience with other readers.</div>
          </div>
          <button className="dialog-close" onClick={onClose} type="button">
            Close
          </button>
        </div>

        <form className="dialog-form" onSubmit={onSubmit}>
          <label className="field">
            <span className="field-label">Rating</span>
            <select className="field-input" disabled={isBusy} {...register("rating", { valueAsNumber: true })}>
              <option value={5}>5 - Excellent</option>
              <option value={4}>4 - Strong</option>
              <option value={3}>3 - Good</option>
              <option value={2}>2 - Fair</option>
              <option value={1}>1 - Poor</option>
            </select>
            {errors.rating?.message ? <span className="field-error">{errors.rating.message}</span> : null}
          </label>

          <Input
            disabled={isBusy}
            error={errors.title?.message}
            label="Title"
            placeholder="Short headline for your review"
            {...register("title")}
          />

          <Textarea
            disabled={isBusy}
            error={errors.content?.message}
            label="Review"
            placeholder="What stood out to you?"
            rows={5}
            {...register("content")}
          />

          {serverError ? <div className="form-alert">{serverError}</div> : null}

          <div className="dialog-actions">
            <Button disabled={isBusy} onClick={onClose} variant="outline">
              Cancel
            </Button>
            <Button disabled={isBusy} type="submit">
              {isBusy ? "Publishing..." : "Publish Review"}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
