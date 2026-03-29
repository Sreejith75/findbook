import type { Address, ID } from "@/types/common.types";

export type ApiBook = {
  id: ID;
  libraryId: ID;
  categoryId: ID;
  title: string;
  author: string;
  isbn: string;
  description: string | null;
  coverImageReference: string | null;
  totalCopies: number;
  availableCopies: number;
  isAvailable: boolean;
  averageRating: number;
  ratingCount: number;
  coverImageUrl: string;
};

export type ApiCategory = {
  id: ID;
  name: string;
};

export type ApiLibrary = {
  id: ID;
  name: string;
  contactEmail: string;
  contactPhone: string;
  address: Address;
};

export type ApiRental = {
  id: ID;
  userAccountId: ID;
  bookId: ID;
  libraryId: ID;
  status: string;
  rentedOn: string;
  dueOn: string;
  returnRequestedOn: string | null;
  returnedOn: string | null;
  deliveryAddress: Address;
};

export type ApiDeliveryTask = {
  id: ID;
  rentalId: ID;
  deliveryPartnerAccountId: ID;
  type: string;
  status: string;
  assignedAt: string;
  completedAt: string | null;
  deliveryAddress: Address;
};

export type ApiSavedAddress = Address & {
  id: ID;
  isDefault: boolean;
};

export type ApiUser = {
  id: ID;
  fullName: string;
  email: string;
  phoneNumber: string | null;
  role: string;
  managedLibraryId: ID | null;
  addresses: ApiSavedAddress[];
};

export type ApiBookReview = {
  id: ID;
  bookId: ID;
  reviewerAccountId: ID;
  rating: number;
  title: string;
  content: string;
  createdOn: string;
  updatedOn: string | null;
};

export type ApiAuthSession = {
  id: ID;
  fullName: string;
  email: string;
  role: string;
  managedLibraryId: ID | null;
  capabilities: {
    canAccessAdminWorkspace: boolean;
    canManageCatalog: boolean;
    canManageUsers: boolean;
    canManageDeliveries: boolean;
    canManageAdminRoles: boolean;
    canViewDispatchQueue: boolean;
    canOperateDeliveryTasks: boolean;
  };
};

export type ApiAdminOverview = {
  totalUsers: number;
  totalLibraries: number;
  totalCategories: number;
  totalBooks: number;
  availableBooks: number;
  activeRentals: number;
  overdueRentals: number;
  openDeliveryTasks: number;
  completedDeliveryTasks: number;
  totalReviews: number;
};

export type Book = {
  id: ID;
  title: string;
  author: string;
  genre: string;
  accentColor: string;
  emoji: string;
  coverImageUrl?: string | null;
  weeklyPrice: string;
  pages: number;
  rating: string;
  isAvailable: boolean;
  description: string;
  tags: string[];
  availableCopies?: number;
  libraryId?: ID;
};

export type NotificationItem = {
  id: ID;
  isRead: boolean;
  message: string;
  timeAgo: string;
};

export type HomeStat = {
  accent: "gold" | "forest" | "sky" | "rust";
  change: string;
  icon: string;
  label: string;
  value: string;
};

export type ActivityItem = {
  id: ID;
  icon: string;
  iconTone: "gold" | "forest" | "sky";
  meta: string;
  status: "active" | "returned" | "transit";
  subtitle: string;
  title: string;
};

export type RentalRecord = {
  id: ID;
  bookId: ID;
  title: string;
  author: string;
  emoji: string;
  accentColor: string;
  rentedOn: string;
  dueOn: string;
  status: "transit" | "active" | "returnRequested" | "returned" | "overdue";
  progress: number;
};

export type DeliveryHistoryItem = {
  id: ID;
  title: string;
  subtitle: string;
  icon: string;
  status: "transit" | "returned" | "failed";
  date: string;
};

export type DeliveryStep = {
  id: ID;
  label: string;
  status: "complete" | "current" | "upcoming";
  time: string;
  marker: string;
};

export type LibraryPartner = {
  id: ID;
  name: string;
  location: string;
  totalBooks: number;
  rating: string;
  emoji: string;
  accentColor: string;
};

export type GenreBreakdown = {
  id: ID;
  name: string;
  percentage: number;
  color: string;
};

export type NotificationSetting = {
  id: ID;
  description: string;
  enabled: boolean;
  label: string;
};

export type AdminTransaction = {
  id: ID;
  user: string;
  book: string;
  library: string;
  date: string;
  status: "transit" | "active" | "returned";
  statusLabel: string;
};

export type AdminTopLibrary = {
  id: ID;
  name: string;
  rentals: number;
  percentage: number;
};

export type HomeOverviewData = {
  currentUser: ApiUser;
  currentRental: RentalRecord | null;
  deliverySteps: DeliveryStep[];
  featuredBooks: Book[];
  recentActivities: ActivityItem[];
  stats: HomeStat[];
  userName: string;
};

export type CatalogOverviewData = {
  books: Book[];
  filterOptions: string[];
  searchQuery?: string;
  totalBooksLabel: string;
  user: ApiUser;
};

export type RentalsOverviewData = {
  rentals: RentalRecord[];
  reviewedBookIds: number[];
};

export type DeliveriesOverviewData = {
  viewMode: "reader" | "delivery" | "admin";
  activeDelivery: DeliveryHistoryItem | null;
  activeTitle: string | null;
  activeSubtitle: string | null;
  address: Address | null;
  history: DeliveryHistoryItem[];
  steps: DeliveryStep[];
};

export type LibrariesOverviewData = {
  libraries: LibraryPartner[];
};

export type ProfileOverviewData = {
  favoriteGenres: GenreBreakdown[];
  joinedLabel: string;
  profileStats: HomeStat[];
  settings: NotificationSetting[];
  user: ApiUser;
};

export type AdminOverviewData = {
  books: ApiBook[];
  categories: ApiCategory[];
  deliveryTasks: ApiDeliveryTask[];
  deliveryPartners: ApiUser[];
  libraries: ApiLibrary[];
  monthlyLabels: string[];
  monthlyRentals: number[];
  overview: ApiAdminOverview;
  rentals: ApiRental[];
  topLibraries: AdminTopLibrary[];
  transactions: AdminTransaction[];
  users: ApiUser[];
};

export type ShellData = {
  notificationCount: number;
  notifications: NotificationItem[];
  userInitials: string;
  userName: string;
};
