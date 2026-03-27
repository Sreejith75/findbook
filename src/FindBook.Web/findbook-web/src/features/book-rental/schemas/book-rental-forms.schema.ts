import { z } from "zod";

export const addressFormSchema = z.object({
  city: z.string().min(2, "City is required"),
  country: z.string().min(2, "Country is required"),
  isDefault: z.boolean(),
  postalCode: z.string().min(3, "Postal code is required"),
  state: z.string().min(2, "State is required"),
  street: z.string().min(3, "Street is required"),
});

export const rentBookFormSchema = z.object({
  addressId: z.number().int().positive("Please choose a delivery address"),
});

export const reviewFormSchema = z.object({
  content: z.string().min(10, "Share a bit more detail"),
  rating: z.coerce.number().int().min(1).max(5),
  title: z.string().min(3, "Review title is required"),
});

export const profileFormSchema = z.object({
  email: z.string().email("Please enter a valid email address"),
  fullName: z.string().min(2, "Full name is required"),
  phoneNumber: z.string().min(7, "Phone number is required"),
  role: z.string().min(1),
});

export const adminUserFormSchema = profileFormSchema.extend({
  managedLibraryId: z.coerce.number().int().nonnegative().nullable().optional(),
  passwordHash: z.string().min(4, "Password hash is required for new users").optional(),
});

export const categoryFormSchema = z.object({
  name: z.string().min(2, "Category name is required"),
});

export const libraryFormSchema = z.object({
  address: addressFormSchema.omit({ isDefault: true }),
  contactEmail: z.string().email("A valid contact email is required"),
  contactPhone: z.string().min(7, "Contact phone is required"),
  name: z.string().min(2, "Library name is required"),
});

export const bookFormSchema = z.object({
  author: z.string().min(2, "Author is required"),
  availableCopies: z.coerce.number().int().min(0),
  categoryId: z.coerce.number().int().positive("Choose a category"),
  coverImageReference: z.string().optional(),
  description: z.string().optional(),
  isbn: z.string().min(10, "ISBN is required"),
  libraryId: z.coerce.number().int().positive("Choose a library"),
  title: z.string().min(2, "Title is required"),
  totalCopies: z.coerce.number().int().min(1),
}).refine((value) => value.availableCopies <= value.totalCopies, {
  message: "Available copies cannot exceed total copies",
  path: ["availableCopies"],
});

export const deliveryTaskFormSchema = z.object({
  deliveryPartnerAccountId: z.coerce.number().int().positive("Choose a delivery partner"),
  rentalId: z.coerce.number().int().positive("Choose a rental"),
  type: z.enum(["Delivery", "Pickup"]),
});

export type AddressFormInput = z.infer<typeof addressFormSchema>;
export type AdminUserFormInput = z.infer<typeof adminUserFormSchema>;
export type BookFormInput = z.infer<typeof bookFormSchema>;
export type CategoryFormInput = z.infer<typeof categoryFormSchema>;
export type DeliveryTaskFormInput = z.infer<typeof deliveryTaskFormSchema>;
export type LibraryFormInput = z.infer<typeof libraryFormSchema>;
export type ProfileFormInput = z.infer<typeof profileFormSchema>;
export type RentBookFormInput = z.infer<typeof rentBookFormSchema>;
export type ReviewFormInput = z.infer<typeof reviewFormSchema>;
