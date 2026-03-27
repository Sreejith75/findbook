"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAppForm } from "@/hooks/use-app-form";
import {
  addressFormSchema,
  profileFormSchema,
  type AddressFormInput,
  type ProfileFormInput,
} from "@/features/book-rental/schemas/book-rental-forms.schema";
import {
  addAddress,
  deleteAddress,
  setDefaultAddress,
  updateAddress,
  updateProfile,
} from "@/features/book-rental/services/book-rental.client";
import type { ApiSavedAddress, ApiUser } from "@/features/book-rental/types/book-rental.types";

type ProfileManagementPanelProps = {
  user: ApiUser;
};

export function ProfileManagementPanel({ user }: ProfileManagementPanelProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [profileError, setProfileError] = useState<string | null>(null);
  const [addressError, setAddressError] = useState<string | null>(null);
  const [editingAddress, setEditingAddress] = useState<ApiSavedAddress | null>(null);

  const profileForm = useAppForm({
    defaultValues: {
      email: user.email,
      fullName: user.fullName,
      phoneNumber: user.phoneNumber ?? "",
      role: user.role,
    },
    schema: profileFormSchema,
  });

  const addressForm = useAppForm({
    defaultValues: getAddressDefaults(user.addresses[0]),
    schema: addressFormSchema,
  });

  const isBusy = isPending || profileForm.formState.isSubmitting || addressForm.formState.isSubmitting;

  const submitProfile = profileForm.handleSubmit(async (data) => {
    setProfileError(null);

    try {
      await updateProfile(data as unknown as ProfileFormInput);
      startTransition(() => {
        router.refresh();
      });
    } catch (error) {
      setProfileError(error instanceof Error ? error.message : "Unable to update profile.");
    }
  });

  const submitAddress = addressForm.handleSubmit(async (data) => {
    setAddressError(null);

    try {
      if (editingAddress) {
        await updateAddress(editingAddress.id, data as unknown as AddressFormInput);
      } else {
        await addAddress(data as unknown as AddressFormInput);
      }

      startTransition(() => {
        setEditingAddress(null);
        addressForm.reset(getAddressDefaults(undefined));
        router.refresh();
      });
    } catch (error) {
      setAddressError(error instanceof Error ? error.message : "Unable to save address.");
    }
  });

  return (
    <section className="dual-column" style={{ marginTop: "28px" }}>
      <div className="chart-card">
        <div className="chart-title">Profile Details</div>
        <form className="stack-form" onSubmit={submitProfile}>
          <Input
            disabled={isBusy}
            error={profileForm.formState.errors.fullName?.message}
            label="Full Name"
            {...profileForm.register("fullName")}
          />
          <Input
            disabled={isBusy}
            error={profileForm.formState.errors.email?.message}
            label="Email"
            type="email"
            {...profileForm.register("email")}
          />
          <Input
            disabled={isBusy}
            error={profileForm.formState.errors.phoneNumber?.message}
            label="Phone Number"
            {...profileForm.register("phoneNumber")}
          />
          <Input disabled label="Role" {...profileForm.register("role")} />
          {profileError ? <div className="form-alert">{profileError}</div> : null}
          <div className="dialog-actions">
            <Button disabled={isBusy} type="submit">
              {isBusy ? "Saving..." : "Save Profile"}
            </Button>
          </div>
        </form>
      </div>

      <div className="chart-card">
        <div className="chart-title">Delivery Addresses</div>
        <div className="stack-list">
          {user.addresses.map((address) => (
            <AddressCard
              address={address}
              isBusy={isBusy}
              key={address.id}
              onDelete={async () => {
                setAddressError(null);

                try {
                  await deleteAddress(address.id);
                  startTransition(() => router.refresh());
                } catch (error) {
                  setAddressError(error instanceof Error ? error.message : "Unable to delete address.");
                }
              }}
              onEdit={() => {
                setEditingAddress(address);
                addressForm.reset(getAddressDefaults(address));
              }}
              onSetDefault={async () => {
                setAddressError(null);

                try {
                  await setDefaultAddress(address.id);
                  startTransition(() => router.refresh());
                } catch (error) {
                  setAddressError(error instanceof Error ? error.message : "Unable to set default address.");
                }
              }}
            />
          ))}
        </div>

        <form className="stack-form" onSubmit={submitAddress}>
          <div className="chart-title" style={{ fontSize: "16px", marginTop: "18px" }}>
            {editingAddress ? "Edit Address" : "Add Address"}
          </div>
          <Input
            disabled={isBusy}
            error={addressForm.formState.errors.street?.message}
            label="Street"
            {...addressForm.register("street")}
          />
          <Input
            disabled={isBusy}
            error={addressForm.formState.errors.city?.message}
            label="City"
            {...addressForm.register("city")}
          />
          <Input
            disabled={isBusy}
            error={addressForm.formState.errors.state?.message}
            label="State"
            {...addressForm.register("state")}
          />
          <Input
            disabled={isBusy}
            error={addressForm.formState.errors.postalCode?.message}
            label="Postal Code"
            {...addressForm.register("postalCode")}
          />
          <Input
            disabled={isBusy}
            error={addressForm.formState.errors.country?.message}
            label="Country"
            {...addressForm.register("country")}
          />
          <label className="checkbox-field">
            <input disabled={isBusy} type="checkbox" {...addressForm.register("isDefault")} />
            <span>Set as default address</span>
          </label>
          {addressError ? <div className="form-alert">{addressError}</div> : null}
          <div className="dialog-actions">
            {editingAddress ? (
              <Button
                disabled={isBusy}
                onClick={() => {
                  setEditingAddress(null);
                  addressForm.reset(getAddressDefaults(undefined));
                }}
                variant="outline"
              >
                Cancel Edit
              </Button>
            ) : null}
            <Button disabled={isBusy} type="submit">
              {editingAddress ? "Update Address" : "Add Address"}
            </Button>
          </div>
        </form>
      </div>
    </section>
  );
}

type AddressCardProps = {
  address: ApiSavedAddress;
  isBusy: boolean;
  onDelete: () => Promise<void>;
  onEdit: () => void;
  onSetDefault: () => Promise<void>;
};

function AddressCard({
  address,
  isBusy,
  onDelete,
  onEdit,
  onSetDefault,
}: AddressCardProps) {
  return (
    <div className="address-card">
      <div>
        <div style={{ fontWeight: 700 }}>
          {address.street}
          {address.isDefault ? <span className="inline-badge">Default</span> : null}
        </div>
        <div className="subtle-text">
          {address.city}, {address.state} {address.postalCode}
        </div>
        <div className="subtle-text">{address.country}</div>
      </div>
      <div className="inline-actions">
        <Button disabled={isBusy} onClick={onEdit} variant="outline">
          Edit
        </Button>
        {!address.isDefault ? (
          <Button disabled={isBusy} onClick={() => void onSetDefault()} variant="ghost">
            Make Default
          </Button>
        ) : null}
        <Button disabled={isBusy} onClick={() => void onDelete()} variant="ghost">
          Delete
        </Button>
      </div>
    </div>
  );
}

function getAddressDefaults(address?: ApiSavedAddress) {
  return {
    city: address?.city ?? "",
    country: address?.country ?? "India",
    isDefault: address?.isDefault ?? true,
    postalCode: address?.postalCode ?? "",
    state: address?.state ?? "",
    street: address?.street ?? "",
  } satisfies AddressFormInput;
}
