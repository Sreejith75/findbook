import { z } from "zod";

export const addressSchema = z.object({
  street: z.string(),
  city: z.string(),
  state: z.string(),
  postalCode: z.string(),
  country: z.string(),
});

export const bookSchema = z.object({
  id: z.number().int().positive(),
  libraryId: z.number().int().positive(),
  categoryId: z.number().int().positive(),
  title: z.string(),
  author: z.string(),
  isbn: z.string(),
  description: z.string().nullable(),
  coverImageReference: z.string().nullable(),
  totalCopies: z.number().int().nonnegative(),
  availableCopies: z.number().int().nonnegative(),
  isAvailable: z.boolean(),
  averageRating: z.number(),
  ratingCount: z.number().int().nonnegative(),
  coverImageUrl: z.string(),
});

export const categorySchema = z.object({
  id: z.number().int().positive(),
  name: z.string(),
});

export const librarySchema = z.object({
  id: z.number().int().positive(),
  name: z.string(),
  contactEmail: z.string(),
  contactPhone: z.string(),
  address: addressSchema,
});

export const rentalSchema = z.object({
  id: z.number().int().positive(),
  userAccountId: z.number().int().positive(),
  bookId: z.number().int().positive(),
  libraryId: z.number().int().positive(),
  status: z.string(),
  rentedOn: z.string(),
  dueOn: z.string(),
  returnRequestedOn: z.string().nullable(),
  returnedOn: z.string().nullable(),
  deliveryAddress: addressSchema,
});

export const deliveryTaskSchema = z.object({
  id: z.number().int().positive(),
  rentalId: z.number().int().positive(),
  deliveryPartnerAccountId: z.number().int().positive(),
  type: z.string(),
  status: z.string(),
  assignedAt: z.string(),
  completedAt: z.string().nullable(),
  deliveryAddress: addressSchema,
});

export const savedAddressSchema = z.object({
  id: z.number().int().positive(),
  street: z.string(),
  city: z.string(),
  state: z.string(),
  postalCode: z.string(),
  country: z.string(),
  isDefault: z.boolean(),
});

export const userSchema = z.object({
  id: z.number().int().positive(),
  fullName: z.string(),
  email: z.string(),
  phoneNumber: z.string().nullable(),
  role: z.string(),
  managedLibraryId: z.number().int().positive().nullable(),
  addresses: z.array(savedAddressSchema),
});

export const bookReviewSchema = z.object({
  id: z.number().int().positive(),
  bookId: z.number().int().positive(),
  reviewerAccountId: z.number().int().positive(),
  rating: z.number().int().min(1).max(5),
  title: z.string(),
  content: z.string(),
  createdOn: z.string(),
  updatedOn: z.string().nullable(),
});

export const adminOverviewSchema = z.object({
  totalUsers: z.number().int().nonnegative(),
  totalLibraries: z.number().int().nonnegative(),
  totalCategories: z.number().int().nonnegative(),
  totalBooks: z.number().int().nonnegative(),
  availableBooks: z.number().int().nonnegative(),
  activeRentals: z.number().int().nonnegative(),
  overdueRentals: z.number().int().nonnegative(),
  openDeliveryTasks: z.number().int().nonnegative(),
  completedDeliveryTasks: z.number().int().nonnegative(),
  totalReviews: z.number().int().nonnegative(),
});

export const booksSchema = z.array(bookSchema);
export const categoriesSchema = z.array(categorySchema);
export const librariesSchema = z.array(librarySchema);
export const rentalsSchema = z.array(rentalSchema);
export const deliveryTasksSchema = z.array(deliveryTaskSchema);
export const usersSchema = z.array(userSchema);
export const reviewsSchema = z.array(bookReviewSchema);
