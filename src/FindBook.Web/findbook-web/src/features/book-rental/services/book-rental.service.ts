import "server-only";

import { cache } from "react";
import { z } from "zod";

import {
  adminOverviewSchema,
  booksSchema,
  categoriesSchema,
  deliveryTasksSchema,
  librariesSchema,
  rentalsSchema,
  reviewsSchema,
  userSchema,
  usersSchema,
} from "@/features/book-rental/schemas/book-rental.schema";
import type {
  ActivityItem,
  AdminOverviewData,
  AdminTopLibrary,
  AdminTransaction,
  ApiAdminOverview,
  ApiAuthSession,
  ApiBook,
  ApiBookReview,
  ApiCategory,
  ApiDeliveryTask,
  ApiLibrary,
  ApiRental,
  ApiUser,
  Book,
  CatalogOverviewData,
  DeliveriesOverviewData,
  DeliveryHistoryItem,
  DeliveryStep,
  GenreBreakdown,
  HomeOverviewData,
  HomeStat,
  LibrariesOverviewData,
  LibraryPartner,
  NotificationItem,
  NotificationSetting,
  ProfileOverviewData,
  RentalRecord,
  RentalsOverviewData,
  ShellData,
} from "@/features/book-rental/types/book-rental.types";
import { requestBackend } from "@/lib/backend-api";

const PRIMARY_ACCENTS = ["#e8f0fe", "#e8f5e9", "#fff3e0", "#fce4ec", "#e8eaf6", "#fffde7"];
const PRIMARY_EMOJIS = ["📘", "📗", "📙", "📕", "📚", "🪄", "🧠", "🌍"];
const NOTIFICATION_SETTINGS: NotificationSetting[] = [
  {
    id: 1,
    description: "Receive reminders before the due date.",
    enabled: true,
    label: "Due Date Alerts",
  },
  {
    id: 2,
    description: "Know when the delivery partner starts the trip.",
    enabled: true,
    label: "Delivery Updates",
  },
  {
    id: 3,
    description: "Get notified when reviews are published.",
    enabled: false,
    label: "Review Activity",
  },
];

export async function getShellData(): Promise<ShellData> {
  const session = await getCurrentSession();
  const [user, rentals, deliveryTasks, reviews, books] = await Promise.all([
    getCurrentUser(),
    listRentals(session.id),
    listDeliveryTasks(),
    listReviews(undefined, session.id),
    listBooks(),
  ]);

  const bookLookup = createLookup(books);
  const rentalLookup = createLookup(rentals);

  const activeTask = deliveryTasks.find((task) => {
    const rental = rentalLookup.get(task.rentalId);
    return rental?.userAccountId === session.id && task.status !== "Completed";
  });

  const dueRental = rentals
    .filter((rental) => rental.status !== "Returned")
    .sort((left, right) => left.dueOn.localeCompare(right.dueOn))[0];

  const notifications: NotificationItem[] = [
    ...(activeTask
      ? [
          {
            id: 1,
            isRead: false,
            message: `${bookLookup.get(rentalLookup.get(activeTask.rentalId)?.bookId ?? 0)?.title ?? "Your book"} is ${activeTask.status === "InTransit" ? "out for delivery" : "scheduled for delivery"}.`,
            timeAgo: formatTimeAgo(activeTask.assignedAt),
          },
        ]
      : []),
    ...(dueRental
      ? [
          {
            id: 2,
            isRead: false,
            message: `${bookLookup.get(dueRental.bookId)?.title ?? "A rented title"} is due on ${formatDateOnly(dueRental.dueOn)}.`,
            timeAgo: `Due in ${Math.max(daysUntil(dueRental.dueOn), 0)} days`,
          },
        ]
      : []),
    {
      id: 3,
      isRead: true,
      message: `You have written ${reviews.length} review${reviews.length === 1 ? "" : "s"} so far.`,
      timeAgo: "Account summary",
    },
  ];

  return {
    notificationCount: notifications.filter((item) => !item.isRead).length,
    notifications,
    userInitials: getInitials(user.fullName),
    userName: user.fullName,
  };
}

export async function getHomeOverviewData(): Promise<HomeOverviewData> {
  const session = await getCurrentSession();
  const [user, books, categories, rentals, deliveryTasks, reviews, libraries] = await Promise.all([
    getCurrentUser(),
    listBooks(),
    listCategories(),
    listRentals(session.id),
    listDeliveryTasks(),
    listReviews(undefined, session.id),
    listLibraries(),
  ]);

  const bookLookup = createLookup(books);
  const categoryLookup = createLookup(categories);
  const libraryLookup = createLookup(libraries);
  const rentalsById = createLookup(rentals);

  const featuredBooks = books
    .slice()
    .sort((left, right) => {
      if (right.ratingCount !== left.ratingCount) {
        return right.ratingCount - left.ratingCount;
      }

      return right.averageRating - left.averageRating;
    })
    .slice(0, 6)
    .map((book) => mapBookView(book, categoryLookup.get(book.categoryId)?.name));

  const currentRental = rentals.find((rental) =>
    ["Active", "Overdue", "ReturnRequested", "PendingDelivery"].includes(rental.status),
  );

  const currentRentalRecord = currentRental
    ? mapRentalRecord(currentRental, bookLookup.get(currentRental.bookId))
    : null;

  const activeDeliveryTask = deliveryTasks.find((task) => {
    const rental = rentalsById.get(task.rentalId);
    return rental?.userAccountId === session.id && task.status !== "Completed";
  });

  const deliverySteps = buildDeliverySteps(activeDeliveryTask);

  const recentActivities = buildRecentActivities({
    books,
    libraries: libraryLookup,
    rentals,
    reviews,
  });

  const returnedCount = rentals.filter((rental) => rental.status === "Returned").length;
  const dueRental = rentals
    .filter((rental) => rental.status !== "Returned")
    .sort((left, right) => left.dueOn.localeCompare(right.dueOn))[0];
  const averageRating =
    reviews.length === 0
      ? "0.0"
      : (reviews.reduce((sum, review) => sum + review.rating, 0) / reviews.length).toFixed(1);

  const stats: HomeStat[] = [
    {
      accent: "gold",
      change: `${rentals.filter((rental) => rental.status !== "Returned").length} currently active`,
      icon: "BK",
      label: "Books Rented",
      value: String(rentals.length),
    },
    {
      accent: "forest",
      change: dueRental ? `${Math.max(daysUntil(dueRental.dueOn), 0)} day buffer` : "No pending returns",
      icon: "RT",
      label: "Books Returned",
      value: String(returnedCount),
    },
    {
      accent: "sky",
      change: `${reviews.length} reviews submitted`,
      icon: "RV",
      label: "Average Rating Given",
      value: averageRating,
    },
    {
      accent: "rust",
      change: dueRental ? bookLookup.get(dueRental.bookId)?.title ?? "Upcoming due date" : "No due dates",
      icon: "DU",
      label: "Days Until Due",
      value: dueRental ? String(Math.max(daysUntil(dueRental.dueOn), 0)) : "0",
    },
  ];

  return {
    currentUser: user,
    currentRental: currentRentalRecord,
    deliverySteps,
    featuredBooks,
    recentActivities,
    stats,
    userName: user.fullName.split(" ")[0] ?? user.fullName,
  };
}

export async function getCatalogOverviewData(searchQuery?: string): Promise<CatalogOverviewData> {
  const [books, categories, user] = await Promise.all([
    listBooks(searchQuery),
    listCategories(),
    getCurrentUser(),
  ]);

  return {
    books: books.map((book) => mapBookView(book, categories.find((category) => category.id === book.categoryId)?.name)),
    filterOptions: ["All Genres", ...categories.map((category) => category.name)],
    searchQuery,
    totalBooksLabel: `${books.length.toLocaleString()} books available across connected libraries.`,
    user,
  };
}

export async function getRentalsOverviewData(): Promise<RentalsOverviewData> {
  const session = await getCurrentSession();
  const [rentals, books, reviews] = await Promise.all([
    listRentals(session.id),
    listBooks(),
    listReviews(undefined, session.id),
  ]);
  const bookLookup = createLookup(books);

  return {
    rentals: rentals
      .slice()
      .sort((left, right) => right.rentedOn.localeCompare(left.rentedOn))
      .map((rental) => mapRentalRecord(rental, bookLookup.get(rental.bookId))),
    reviewedBookIds: reviews.map((review) => review.bookId),
  };
}

export async function getDeliveriesOverviewData(): Promise<DeliveriesOverviewData> {
  const session = await getCurrentSession();
  const [tasks, rentals, books, users] = await Promise.all([
    listDeliveryTasks(),
    listRentals(session.id),
    listBooks(),
    listUsers(),
  ]);

  const bookLookup = createLookup(books);
  const rentalsById = createLookup(rentals);
  const usersById = createLookup(users);
  const relevantTasks = tasks.filter((task) => rentalsById.has(task.rentalId));
  const activeTask = relevantTasks.find((task) => task.status !== "Completed") ?? null;
  const activeRental = activeTask ? rentalsById.get(activeTask.rentalId) ?? null : null;
  const activeBook = activeRental ? bookLookup.get(activeRental.bookId) ?? null : null;

  return {
    activeDelivery: activeTask ? mapDeliveryHistoryItem(activeTask, activeRental, activeBook, usersById.get(activeTask.deliveryPartnerAccountId)) : null,
    activeTitle: activeBook?.title ?? null,
    activeSubtitle: activeBook ? `${activeBook.author} · ISBN ${activeBook.isbn}` : null,
    address: activeTask?.deliveryAddress ?? null,
    history: relevantTasks
      .slice()
      .sort((left, right) => right.assignedAt.localeCompare(left.assignedAt))
      .map((task) => mapDeliveryHistoryItem(task, rentalsById.get(task.rentalId), bookLookup.get(rentalsById.get(task.rentalId)?.bookId ?? 0), usersById.get(task.deliveryPartnerAccountId))),
    steps: buildDeliverySteps(activeTask),
  };
}

export async function getLibrariesOverviewData(): Promise<LibrariesOverviewData> {
  const [libraries, books] = await Promise.all([listLibraries(), listBooks()]);

  const booksByLibrary = groupBy(books, (book) => book.libraryId);

  return {
    libraries: libraries.map((library, index) =>
      mapLibraryPartner(library, booksByLibrary.get(library.id) ?? [], index),
    ),
  };
}

export async function getProfileOverviewData(): Promise<ProfileOverviewData> {
  const session = await getCurrentSession();
  const [user, rentals, books, categories, reviews] = await Promise.all([
    getCurrentUser(),
    listRentals(session.id),
    listBooks(),
    listCategories(),
    listReviews(undefined, session.id),
  ]);

  const bookLookup = createLookup(books);
  const categoryLookup = createLookup(categories);
  const genreCounts = new Map<string, number>();

  for (const rental of rentals) {
    const book = bookLookup.get(rental.bookId);
    const categoryName = book ? categoryLookup.get(book.categoryId)?.name : undefined;

    if (categoryName) {
      genreCounts.set(categoryName, (genreCounts.get(categoryName) ?? 0) + 1);
    }
  }

  const totalGenreCount = Array.from(genreCounts.values()).reduce((sum, count) => sum + count, 0);
  const favoriteGenres: GenreBreakdown[] = Array.from(genreCounts.entries())
    .sort((left, right) => right[1] - left[1])
    .slice(0, 4)
    .map(([name, count], index) => ({
      id: index + 1,
      name,
      percentage: totalGenreCount === 0 ? 0 : Math.round((count / totalGenreCount) * 100),
      color: PRIMARY_ACCENTS[index % PRIMARY_ACCENTS.length],
    }));

  const onTimeReturns = rentals.filter((rental) => {
    if (rental.status !== "Returned" || !rental.returnedOn) {
      return false;
    }

    return rental.returnedOn <= rental.dueOn;
  }).length;

  const onTimeRate = rentals.filter((rental) => rental.status === "Returned").length;

  const profileStats: HomeStat[] = [
    {
      accent: "gold",
      change: `${rentals.filter((rental) => rental.status !== "Returned").length} currently on hand`,
      icon: "BK",
      label: "Total Books Rented",
      value: String(rentals.length),
    },
    {
      accent: "forest",
      change: `${onTimeReturns} punctual return${onTimeReturns === 1 ? "" : "s"}`,
      icon: "OK",
      label: "On-Time Returns",
      value: onTimeRate === 0 ? "0%" : `${Math.round((onTimeReturns / onTimeRate) * 100)}%`,
    },
    {
      accent: "sky",
      change: favoriteGenres[0]?.name ?? "No review history yet",
      icon: "RV",
      label: "Reviews Written",
      value: String(reviews.length),
    },
    {
      accent: "rust",
      change: "Derived from completed activity",
      icon: "PT",
      label: "Reading Points",
      value: String(rentals.length * 50 + reviews.length * 20),
    },
  ];

  return {
    favoriteGenres,
    joinedLabel: "Profile overview",
    profileStats,
    settings: NOTIFICATION_SETTINGS,
    user,
  };
}

export async function getAdminOverviewData(): Promise<AdminOverviewData> {
  const [overview, rentals, users, books, libraries, categories, deliveryTasks] = await Promise.all([
    getAdminOverview(),
    listAllRentals(),
    listUsers(),
    listBooks(),
    listLibraries(),
    listCategories(),
    listDeliveryTasks(),
  ]);

  const usersById = createLookup(users);
  const booksById = createLookup(books);
  const librariesById = createLookup(libraries);
  const rentalsByMonth = new Map<string, number>();

  for (const rental of rentals) {
    const key = rental.rentedOn.slice(0, 7);
    rentalsByMonth.set(key, (rentalsByMonth.get(key) ?? 0) + 1);
  }

  const monthlyLabels = Array.from(rentalsByMonth.keys()).sort();
  const monthlyRentals = monthlyLabels.map((label) => rentalsByMonth.get(label) ?? 0);

  const rentalsByLibrary = groupBy(rentals, (rental) => rental.libraryId);
  const maxLibraryRentals = Math.max(
    1,
    ...Array.from(rentalsByLibrary.values()).map((items) => items.length),
  );

  const topLibraries: AdminTopLibrary[] = Array.from(rentalsByLibrary.entries())
    .map(([libraryId, items]) => ({
      id: libraryId,
      name: librariesById.get(libraryId)?.name ?? `Library ${libraryId}`,
      rentals: items.length,
      percentage: Math.round((items.length / maxLibraryRentals) * 100),
    }))
    .sort((left, right) => right.rentals - left.rentals)
    .slice(0, 5);

  const transactions: AdminTransaction[] = rentals
    .slice()
    .sort((left, right) => right.rentedOn.localeCompare(left.rentedOn))
    .slice(0, 6)
    .map((rental) => ({
      id: rental.id,
      user: usersById.get(rental.userAccountId)?.fullName ?? `User ${rental.userAccountId}`,
      book: booksById.get(rental.bookId)?.title ?? `Book ${rental.bookId}`,
      library: librariesById.get(rental.libraryId)?.name ?? `Library ${rental.libraryId}`,
      date: formatDateOnly(rental.rentedOn),
      status: mapAdminStatus(rental.status),
      statusLabel: mapRentalStatusLabel(rental.status),
    }));

  return {
    books,
    categories,
    deliveryPartners: users.filter((user) => user.role === "DeliveryPartner"),
    deliveryTasks,
    libraries,
    monthlyLabels,
    monthlyRentals,
    overview,
    rentals,
    topLibraries,
    transactions,
    users,
  };
}

async function getCurrentUser(): Promise<ApiUser> {
  const session = await getCurrentSession();
  return requestJson(`/api/users/${session.id}`, userSchema);
}

const getCurrentSession = cache(async (): Promise<ApiAuthSession> => {
  return requestJson("/api/auth/session", z.object({
    id: z.number().int().positive(),
    fullName: z.string(),
    email: z.string(),
    role: z.string(),
  }));
});

async function listUsers(): Promise<ApiUser[]> {
  return requestJson("/api/users", usersSchema);
}

async function listBooks(query?: string): Promise<ApiBook[]> {
  if (!query?.trim()) {
    return requestJson("/api/books", booksSchema);
  }

  const searchParams = new URLSearchParams();
  searchParams.set("q", query.trim());
  return requestJson(`/api/books?${searchParams.toString()}`, booksSchema);
}

async function listCategories(): Promise<ApiCategory[]> {
  return requestJson("/api/categories", categoriesSchema);
}

async function listLibraries(): Promise<ApiLibrary[]> {
  return requestJson("/api/libraries", librariesSchema);
}

async function listRentals(userAccountId: number): Promise<ApiRental[]> {
  return requestJson(`/api/rentals?userAccountId=${userAccountId}`, rentalsSchema);
}

async function listAllRentals(): Promise<ApiRental[]> {
  return requestJson("/api/rentals", rentalsSchema);
}

async function listDeliveryTasks(): Promise<ApiDeliveryTask[]> {
  return requestJson("/api/delivery-tasks", deliveryTasksSchema);
}

async function listReviews(bookId?: number, reviewerAccountId?: number): Promise<ApiBookReview[]> {
  const searchParams = new URLSearchParams();

  if (typeof bookId === "number") {
    searchParams.set("bookId", String(bookId));
  }

  if (typeof reviewerAccountId === "number") {
    searchParams.set("reviewerAccountId", String(reviewerAccountId));
  }

  const query = searchParams.toString();
  return requestJson(query ? `/api/reviews?${query}` : "/api/reviews", reviewsSchema);
}

async function getAdminOverview(): Promise<ApiAdminOverview> {
  return requestJson("/api/admin/overview", adminOverviewSchema);
}

async function requestJson<T>(path: string, schema: z.ZodType<T>): Promise<T> {
  const response = await requestBackend(path, { method: "GET" });
  const bodyText = response.body.toString("utf8");

  if (response.status < 200 || response.status >= 300) {
    throw new Error(`Request to ${path} failed with status ${response.status}: ${bodyText}`);
  }

  let parsedBody: unknown;
  try {
    parsedBody = JSON.parse(bodyText);
  } catch (error) {
    throw new Error(`Invalid JSON received from ${path}: ${String(error)}`);
  }

  return schema.parse(parsedBody);
}

function mapBookView(book: ApiBook, categoryName?: string): Book {
  const visualIndex = book.id % PRIMARY_ACCENTS.length;

  return {
    id: book.id,
    title: book.title,
    author: book.author,
    genre: categoryName ?? "General",
    accentColor: PRIMARY_ACCENTS[visualIndex],
    emoji: PRIMARY_EMOJIS[book.id % PRIMARY_EMOJIS.length],
    weeklyPrice: `INR ${20 + book.id * 5}`,
    pages: 220 + book.id * 48,
    rating: book.averageRating > 0 ? book.averageRating.toFixed(1) : "New",
    isAvailable: book.isAvailable,
    description: book.description ?? "No summary is available for this title yet.",
    tags: [categoryName ?? "Catalog", `${book.availableCopies}/${book.totalCopies} available`, `ISBN ${book.isbn.slice(-4)}`],
    availableCopies: book.availableCopies,
    libraryId: book.libraryId,
  };
}

function mapRentalRecord(rental: ApiRental, book?: ApiBook): RentalRecord {
  const bookView = book ? mapBookView(book) : null;
  const progress = calculateRentalProgress(rental.rentedOn, rental.dueOn, rental.returnedOn);

  return {
    id: rental.id,
    bookId: rental.bookId,
    title: book?.title ?? `Book ${rental.bookId}`,
    author: book?.author ?? "Unknown author",
    emoji: bookView?.emoji ?? "📘",
    accentColor: bookView?.accentColor ?? PRIMARY_ACCENTS[rental.id % PRIMARY_ACCENTS.length],
    rentedOn: formatDateOnly(rental.rentedOn),
    dueOn: formatDateOnly(rental.dueOn),
    status: mapRentalStatus(rental.status),
    progress,
  };
}

function mapDeliveryHistoryItem(
  task: ApiDeliveryTask,
  rental: ApiRental | null | undefined,
  book: ApiBook | null | undefined,
  user: ApiUser | null | undefined,
): DeliveryHistoryItem {
  const status = task.status === "Completed" ? "returned" : task.status === "Failed" ? "failed" : "transit";
  const riderLabel = user?.fullName ?? `Partner ${task.deliveryPartnerAccountId}`;

  return {
    id: task.id,
    title: `${book?.title ?? `Book ${rental?.bookId ?? task.rentalId}`} — ${mapDeliveryTaskLabel(task.type, task.status)}`,
    subtitle: `${riderLabel} · ${task.type === "Pickup" ? "Pickup flow" : "Doorstep delivery"}`,
    icon: task.type === "Pickup" ? "RT" : "DV",
    status,
    date: formatDateTime(task.completedAt ?? task.assignedAt),
  };
}

function mapLibraryPartner(library: ApiLibrary, books: ApiBook[], index: number): LibraryPartner {
  const averageRatings = books.filter((book) => book.ratingCount > 0).map((book) => book.averageRating);
  const averageRating =
    averageRatings.length === 0
      ? "New"
      : (averageRatings.reduce((sum, rating) => sum + rating, 0) / averageRatings.length).toFixed(1);

  return {
    id: library.id,
    name: library.name,
    location: `${library.address.city}, ${library.address.state}`,
    totalBooks: books.length,
    rating: averageRating,
    emoji: PRIMARY_EMOJIS[index % PRIMARY_EMOJIS.length],
    accentColor: PRIMARY_ACCENTS[index % PRIMARY_ACCENTS.length],
  };
}

function buildRecentActivities(input: {
  books: ApiBook[];
  libraries: Map<number, ApiLibrary>;
  rentals: ApiRental[];
  reviews: ApiBookReview[];
}): ActivityItem[] {
  const booksById = createLookup(input.books);

  const rentalActivities = input.rentals.map<ActivityItem>((rental) => {
    const book = booksById.get(rental.bookId);
    const library = input.libraries.get(rental.libraryId);

    return {
      id: rental.id,
      icon: rental.status === "Returned" ? "OK" : "BX",
      iconTone: rental.status === "Returned" ? "forest" : "gold",
      meta: formatDateOnly(rental.rentedOn),
      status: rental.status === "Returned" ? "returned" : rental.status === "PendingDelivery" ? "transit" : "active",
      subtitle: `${library?.name ?? "Library"} · ${mapRentalStatusLabel(rental.status)}`,
      title: `${rental.status === "Returned" ? "Returned" : "Rented"} — ${book?.title ?? `Book ${rental.bookId}`}`,
    };
  });

  const reviewActivities = input.reviews.map<ActivityItem>((review) => ({
    id: review.id + 10_000,
    icon: "RV",
    iconTone: "sky",
    meta: formatDateTime(review.createdOn),
    status: "active",
    subtitle: `Rated ${review.rating} stars — ${review.title}`,
    title: `Reviewed — ${booksById.get(review.bookId)?.title ?? `Book ${review.bookId}`}`,
  }));

  return [...rentalActivities, ...reviewActivities].slice(0, 5);
}

function buildDeliverySteps(task: ApiDeliveryTask | null | undefined): DeliveryStep[] {
  const taskStatus = task?.status ?? "Assigned";

  return [
    {
      id: 1,
      label: "Order Placed",
      marker: "OK",
      status: "complete",
      time: task ? formatDateTime(task.assignedAt) : "Queued",
    },
    {
      id: 2,
      label: task?.type === "Pickup" ? "Pickup Scheduled" : "Picked from Library",
      marker: "PK",
      status: taskStatus === "Assigned" ? "current" : "complete",
      time: taskStatus === "Assigned" ? "Processing" : task ? formatDateTime(task.assignedAt) : "Pending",
    },
    {
      id: 3,
      label: task?.type === "Pickup" ? "On the way back" : "Out for Delivery",
      marker: "DV",
      status: taskStatus === "InTransit" ? "current" : taskStatus === "Completed" ? "complete" : "upcoming",
      time: taskStatus === "InTransit" ? "In transit" : taskStatus === "Completed" ? formatDateTime(task?.completedAt ?? task?.assignedAt ?? new Date().toISOString()) : "Pending",
    },
    {
      id: 4,
      label: task?.type === "Pickup" ? "Returned to Library" : "Delivered",
      marker: "HM",
      status: taskStatus === "Completed" ? "current" : "upcoming",
      time: taskStatus === "Completed" ? formatDateTime(task?.completedAt ?? task?.assignedAt ?? new Date().toISOString()) : "Pending",
    },
  ];
}

function calculateRentalProgress(rentedOn: string, dueOn: string, returnedOn: string | null): number {
  if (returnedOn) {
    return 100;
  }

  const rentedDate = dateOnlyToUtcDate(rentedOn).getTime();
  const dueDate = dateOnlyToUtcDate(dueOn).getTime();
  const total = Math.max(dueDate - rentedDate, 1);
  const elapsed = Date.now() - rentedDate;

  return Math.max(5, Math.min(100, Math.round((elapsed / total) * 100)));
}

function mapRentalStatus(status: string): RentalRecord["status"] {
  if (status === "Returned") {
    return "returned";
  }

  if (status === "PendingDelivery") {
    return "transit";
  }

  if (status === "Overdue") {
    return "overdue";
  }

  return "active";
}

function mapAdminStatus(status: string): AdminTransaction["status"] {
  if (status === "Returned") {
    return "returned";
  }

  if (status === "PendingDelivery") {
    return "transit";
  }

  return "active";
}

function mapRentalStatusLabel(status: string): string {
  switch (status) {
    case "PendingDelivery":
      return "In Transit";
    case "Returned":
      return "Returned";
    case "Overdue":
      return "Overdue";
    case "ReturnRequested":
      return "Return Requested";
    default:
      return "Active";
  }
}

function mapDeliveryTaskLabel(type: string, status: string): string {
  if (status === "Completed") {
    return type === "Pickup" ? "Returned" : "Delivered";
  }

  if (status === "Failed") {
    return "Failed";
  }

  return type === "Pickup" ? "Pickup In Progress" : "Out for Delivery";
}

function formatDateOnly(value: string): string {
  const date = dateOnlyToUtcDate(value);
  return new Intl.DateTimeFormat("en-IN", {
    day: "numeric",
    month: "short",
    year: "numeric",
    timeZone: "UTC",
  }).format(date);
}

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat("en-IN", {
    day: "numeric",
    month: "short",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatTimeAgo(value: string): string {
  const diffInHours = Math.max(1, Math.round((Date.now() - new Date(value).getTime()) / (1000 * 60 * 60)));

  if (diffInHours < 24) {
    return `${diffInHours} hour${diffInHours === 1 ? "" : "s"} ago`;
  }

  const days = Math.round(diffInHours / 24);
  return `${days} day${days === 1 ? "" : "s"} ago`;
}

function daysUntil(value: string): number {
  const today = new Date();
  const midnightToday = Date.UTC(today.getUTCFullYear(), today.getUTCMonth(), today.getUTCDate());
  const target = dateOnlyToUtcDate(value).getTime();
  return Math.ceil((target - midnightToday) / (1000 * 60 * 60 * 24));
}

function dateOnlyToUtcDate(value: string): Date {
  const [year, month, day] = value.split("-").map(Number);
  return new Date(Date.UTC(year, month - 1, day));
}

function getInitials(fullName: string): string {
  return fullName
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((segment) => segment[0]?.toUpperCase() ?? "")
    .join("");
}

function createLookup<T extends { id: number }>(items: T[]): Map<number, T> {
  return new Map(items.map((item) => [item.id, item]));
}

function groupBy<T>(items: T[], selector: (item: T) => number): Map<number, T[]> {
  const grouped = new Map<number, T[]>();

  for (const item of items) {
    const key = selector(item);
    const bucket = grouped.get(key);

    if (bucket) {
      bucket.push(item);
      continue;
    }

    grouped.set(key, [item]);
  }

  return grouped;
}
