"use client";

import type {
  AddressFormInput,
  AdminUserFormInput,
  BookFormInput,
  CategoryFormInput,
  DeliveryTaskFormInput,
  LibraryFormInput,
  ProfileFormInput,
  ReviewFormInput,
} from "@/features/book-rental/schemas/book-rental-forms.schema";
import type { ApiAuthSession, ApiSavedAddress, ApiUser } from "@/features/book-rental/types/book-rental.types";

let cachedSession: ApiAuthSession | null = null;

export async function createRental(input: {
  address: ApiSavedAddress;
  bookId: number;
  libraryId: number;
}) {
  const session = await getAuthenticatedSession();

  return requestJson("/api/v1/rentals", {
    deliveryAddress: {
      city: input.address.city,
      country: input.address.country,
      postalCode: input.address.postalCode,
      state: input.address.state,
      street: input.address.street,
    },
    bookId: input.bookId,
    libraryId: input.libraryId,
    rentedOn: new Date().toISOString().slice(0, 10),
    userAccountId: session.id,
  });
}

export async function requestRentalReturn(rentalId: number) {
  return requestJson(`/api/v1/rentals/${rentalId}/request-return`, {
    requestedOn: new Date().toISOString().slice(0, 10),
  });
}

export async function createReview(rentalBookId: number, input: ReviewFormInput) {
  const session = await getAuthenticatedSession();

  return requestJson("/api/v1/reviews", {
    bookId: rentalBookId,
    content: input.content,
    createdOn: new Date().toISOString(),
    rating: input.rating,
    reviewerAccountId: session.id,
    title: input.title,
  });
}

export async function updateProfile(input: ProfileFormInput & { managedLibraryId?: number | null }) {
  const session = await getAuthenticatedSession();

  return requestJson(`/api/v1/users/${session.id}`, {
    email: input.email,
    fullName: input.fullName,
    managedLibraryId: input.managedLibraryId ?? null,
    phoneNumber: input.phoneNumber,
    role: input.role,
  });
}

export async function createUser(input: AdminUserFormInput) {
  return requestJson("/api/v1/users", {
    email: input.email,
    fullName: input.fullName,
    managedLibraryId: input.managedLibraryId ?? null,
    passwordHash: input.passwordHash ?? "hash-user",
    phoneNumber: input.phoneNumber,
    role: input.role,
  });
}

export async function updateUser(userId: number, input: AdminUserFormInput) {
  return requestJson(`/api/v1/users/${userId}`, {
    email: input.email,
    fullName: input.fullName,
    managedLibraryId: input.managedLibraryId ?? null,
    phoneNumber: input.phoneNumber,
    role: input.role,
  }, "PUT");
}

export async function addAddress(input: AddressFormInput): Promise<ApiUser> {
  const session = await getAuthenticatedSession();
  return requestJson(`/api/v1/users/${session.id}/addresses`, input);
}

export async function updateAddress(addressId: number, input: AddressFormInput): Promise<ApiUser> {
  const session = await getAuthenticatedSession();
  return requestJson(`/api/v1/users/${session.id}/addresses/${addressId}`, input, "PUT");
}

export async function deleteAddress(addressId: number) {
  const session = await getAuthenticatedSession();
  return requestJson(`/api/v1/users/${session.id}/addresses/${addressId}`, undefined, "DELETE");
}

export async function setDefaultAddress(addressId: number): Promise<ApiUser> {
  const session = await getAuthenticatedSession();
  return requestJson(`/api/v1/users/${session.id}/addresses/${addressId}/default`, {}, "POST");
}

export async function createCategory(input: CategoryFormInput) {
  return requestJson("/api/v1/categories", input);
}

export async function updateCategory(categoryId: number, input: CategoryFormInput) {
  return requestJson(`/api/v1/categories/${categoryId}`, input, "PUT");
}

export async function createLibrary(input: LibraryFormInput) {
  return requestJson("/api/v1/libraries", input);
}

export async function updateLibrary(libraryId: number, input: LibraryFormInput) {
  return requestJson(`/api/v1/libraries/${libraryId}`, input, "PUT");
}

export async function createBook(input: BookFormInput) {
  return requestJson("/api/v1/books", normalizeBookPayload(input));
}

export async function updateBook(bookId: number, input: BookFormInput) {
  return requestJson(`/api/v1/books/${bookId}`, normalizeBookPayload(input), "PUT");
}

export async function uploadBookCover(bookId: number, file: File) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await fetch(`/api/v1/books/${bookId}/cover`, {
    body: formData,
    method: "POST",
  });

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  return response.json();
}

export async function createDeliveryTask(input: {
  deliveryPartnerAccountId: number;
  rentalId: number;
  type: string;
}) {
  return requestJson("/api/v1/delivery-tasks", {
    assignedAt: new Date().toISOString(),
    deliveryPartnerAccountId: input.deliveryPartnerAccountId,
    rentalId: input.rentalId,
    type: input.type,
  });
}

export async function markDeliveryTaskInTransit(taskId: number) {
  return requestJson(`/api/v1/delivery-tasks/${taskId}/mark-in-transit`, {});
}

export async function completeDeliveryTask(taskId: number) {
  return requestJson(`/api/v1/delivery-tasks/${taskId}/complete`, {
    completedAt: new Date().toISOString(),
  });
}

export async function failDeliveryTask(taskId: number) {
  return requestJson(`/api/v1/delivery-tasks/${taskId}/fail`, {});
}

export function resetAuthenticatedSessionCache() {
  cachedSession = null;
}

async function getAuthenticatedSession() {
  if (cachedSession) {
    return cachedSession;
  }

  const response = await fetch("/api/auth/session");

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  cachedSession = await response.json() as ApiAuthSession;
  return cachedSession;
}

async function requestJson(path: string, body?: unknown, method: "POST" | "PUT" | "DELETE" = "POST") {
  const response = await fetch(path, {
    body: typeof body === "undefined" ? undefined : JSON.stringify(body),
    headers: {
      "Content-Type": "application/json",
    },
    method,
  });

  if (!response.ok) {
    throw new Error(await extractErrorMessage(response));
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

async function extractErrorMessage(response: Response): Promise<string> {
  const text = await response.text();

  try {
    const parsed = JSON.parse(text) as {
      errors?: Record<string, string[]>;
      detail?: string;
      errorsTitle?: string;
      message?: string;
      title?: string;
    };

    const validationErrors = parsed.errors
      ? Object.values(parsed.errors)
          .flat()
          .join(", ")
      : null;

    return (
      validationErrors ??
      parsed.detail ??
      parsed.message ??
      parsed.title ??
      `Request failed with status ${response.status}`
    );
  } catch {
    return text || `Request failed with status ${response.status}`;
  }
}

function normalizeBookPayload(input: BookFormInput) {
  return {
    author: input.author,
    availableCopies: input.availableCopies,
    categoryId: input.categoryId,
    coverImageReference: input.coverImageReference || null,
    description: input.description || null,
    isbn: input.isbn.trim(),
    libraryId: input.libraryId,
    title: input.title,
    totalCopies: input.totalCopies,
  };
}
