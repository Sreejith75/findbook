"use client";

import { useMemo, useState, useTransition } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { useAppForm } from "@/hooks/use-app-form";
import { rentBookFormSchema } from "@/features/book-rental/schemas/book-rental-forms.schema";
import { createRental } from "@/features/book-rental/services/book-rental.client";
import type { ApiSavedAddress, ApiUser, Book } from "@/features/book-rental/types/book-rental.types";

type RentBookDialogProps = {
  book: Book | null;
  onClose: () => void;
  onSuccess: (bookId: number) => void;
  user: ApiUser;
};

export function RentBookDialog({
  book,
  onClose,
  onSuccess,
  user,
}: RentBookDialogProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [serverError, setServerError] = useState<string | null>(null);

  const defaultAddress = useMemo(
    () => user.addresses.find((address) => address.isDefault) ?? user.addresses[0] ?? null,
    [user.addresses],
  );

  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useAppForm({
    defaultValues: {
      addressId: defaultAddress?.id,
    },
    schema: rentBookFormSchema,
  });

  if (!book) {
    return null;
  }

  const isBusy = isSubmitting || isPending;
  const hasSavedAddresses = user.addresses.length > 0;

  const onSubmit = handleSubmit(async (data) => {
    const selectedAddress = user.addresses.find((address) => address.id === Number(data.addressId));

    if (!selectedAddress) {
      setServerError("Please choose a valid delivery address.");
      return;
    }

    setServerError(null);

    try {
      await createRental({
        address: selectedAddress,
        bookId: book.id,
        libraryId: book.libraryId ?? 0,
      });

      startTransition(() => {
        onSuccess(book.id);
        onClose();
        router.push("/rentals");
        router.refresh();
      });
    } catch (error) {
      setServerError(error instanceof Error ? error.message : "Unable to create rental.");
    }
  });

  return (
    <div className="dialog-backdrop" onClick={onClose}>
      <div className="dialog-card" onClick={(event) => event.stopPropagation()}>
        <div className="dialog-header">
          <div>
            <div className="dialog-title">Rent {book.title}</div>
            <div className="dialog-subtitle">
              Confirm the delivery address for this rental.
            </div>
          </div>
          <button className="dialog-close" onClick={onClose} type="button">
            Close
          </button>
        </div>

        <form className="dialog-form" onSubmit={onSubmit}>
          {!hasSavedAddresses ? (
            <div className="form-alert">
              Add a delivery address in your profile before creating a rental.
            </div>
          ) : null}
          <label className="field">
            <span className="field-label">Delivery Address</span>
            <select className="field-input" disabled={isBusy || !hasSavedAddresses} {...register("addressId", { valueAsNumber: true })}>
              {user.addresses.map((address) => (
                <option key={address.id} value={address.id}>
                  {formatAddressOption(address)}
                </option>
              ))}
            </select>
            {errors.addressId?.message ? <span className="field-error">{errors.addressId.message}</span> : null}
          </label>

          {serverError ? <div className="form-alert">{serverError}</div> : null}

          <div className="dialog-actions">
            <Button disabled={isBusy} onClick={onClose} variant="outline">
              Cancel
            </Button>
            {hasSavedAddresses ? (
              <Button disabled={isBusy || !book.isAvailable} type="submit">
                {isBusy ? "Submitting..." : "Confirm Rental"}
              </Button>
            ) : (
              <Button
                disabled={isBusy}
                onClick={() => {
                  onClose();
                  router.push("/profile");
                }}
                type="button"
              >
                Go To Profile
              </Button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
}

function formatAddressOption(address: ApiSavedAddress) {
  return `${address.street}, ${address.city}, ${address.state} ${address.postalCode}${address.isDefault ? " · Default" : ""}`;
}
