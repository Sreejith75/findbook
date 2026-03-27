# FindBook API Report

## Scope
This document defines the required API surface for the current FindBook MVP based on the bounded contexts already modeled in `FindBook.Core`.

The current implementation targets:

- User management
- Library inventory
- Rentals
- Delivery
- Feedback
- Admin overview
- Book cover image upload/download

Deferred from this version:

- JWT authentication and refresh tokens
- recommendation engine APIs
- notifications APIs
- advanced analytics/report exports
- review edit/delete with full rating-summary recomputation

## Design Principles
- Routes are grouped under `/api`
- Endpoints are organized by bounded context ownership
- Aggregates are mutated only through behavior already defined in Core
- Cross-context updates happen inside the API use case when one workflow touches multiple aggregates
- Request and response payloads use primitives; domain value objects stay inside the server boundary

## Context Map

### UserManagement
Owns:

- `UserAccount`
- `SavedAddress`

Endpoints:

- `GET /api/users`
- `GET /api/users/{id}`
- `POST /api/users`
- `PUT /api/users/{id}`
- `POST /api/users/{id}/addresses`
- `PUT /api/users/{id}/addresses/{addressId}`
- `POST /api/users/{id}/addresses/{addressId}/default`
- `DELETE /api/users/{id}/addresses/{addressId}`

Rules:

- email must be valid and unique
- role must be one of `User`, `Admin`, `SuperAdmin`, `DeliveryPartner`
- `ManagedLibraryId` is allowed only for admin-capable roles
- only one default address may exist per user

### LibraryInventory
Owns:

- `Library`
- `Category`
- `Book`
- `BookImage`

Endpoints:

- `GET /api/libraries`
- `GET /api/libraries/{id}`
- `POST /api/libraries`
- `PUT /api/libraries/{id}`
- `GET /api/categories`
- `GET /api/categories/{id}`
- `POST /api/categories`
- `PUT /api/categories/{id}`
- `GET /api/books`
- `GET /api/books/{id}`
- `POST /api/books`
- `PUT /api/books/{id}`
- `POST /api/books/{bookId}/cover`
- `GET /api/books/{bookId}/cover`

Book query parameters:

- `q`
- `libraryId`
- `categoryId`
- `availableOnly`

Rules:

- library and category must exist before a book can be created
- ISBN must be valid and unique
- inventory must be internally consistent
- cover upload accepts JPEG, PNG, WEBP

### Rental
Owns:

- `Rental`

Endpoints:

- `GET /api/rentals`
- `GET /api/rentals/{id}`
- `POST /api/rentals`
- `POST /api/rentals/{id}/mark-delivered`
- `POST /api/rentals/{id}/request-return`
- `POST /api/rentals/{id}/complete-return`
- `POST /api/rentals/{id}/mark-overdue`

Rental query parameters:

- `userAccountId`
- `bookId`
- `status`

Rules:

- user, book, and library must exist
- creating a rental reserves one copy from the selected book
- completing a return restocks the book
- address is captured as a delivery snapshot

### Delivery
Owns:

- `DeliveryTask`

Endpoints:

- `GET /api/delivery-tasks`
- `GET /api/delivery-tasks/{id}`
- `POST /api/delivery-tasks`
- `POST /api/delivery-tasks/{id}/mark-in-transit`
- `POST /api/delivery-tasks/{id}/complete`
- `POST /api/delivery-tasks/{id}/fail`

Delivery query parameters:

- `deliveryPartnerAccountId`
- `status`
- `type`

Rules:

- rental and delivery partner must exist
- delivery partner should use `DeliveryPartner` role
- completing a `Delivery` task marks the rental delivered
- completing a `Pickup` task completes the rental return and restocks inventory if not already returned

### Feedback
Owns:

- `BookReview`

Endpoints:

- `GET /api/reviews`
- `GET /api/reviews/{id}`
- `POST /api/reviews`

Review query parameters:

- `bookId`
- `reviewerAccountId`

Rules:

- user and book must exist
- one review per user per book
- creating a review updates the book rating summary

### Admin
Owns:

- no aggregate in MVP

Endpoints:

- `GET /api/admin/overview`

Purpose:

- read-only reporting snapshot across contexts

## Request and Response Summary

### User payloads
`CreateUserRequest`

- `fullName`
- `email`
- `passwordHash`
- `role`
- `phoneNumber`
- `managedLibraryId`

`AddressRequest`

- `street`
- `city`
- `state`
- `postalCode`
- `country`
- `isDefault`

`UserResponse`

- `id`
- `fullName`
- `email`
- `phoneNumber`
- `role`
- `managedLibraryId`
- `addresses`

### Inventory payloads
`CreateLibraryRequest`

- `name`
- `contactEmail`
- `contactPhone`
- `address`

`CreateCategoryRequest`

- `name`

`CreateBookRequest`

- `libraryId`
- `categoryId`
- `title`
- `author`
- `isbn`
- `description`
- `coverImageReference`
- `totalCopies`
- `availableCopies`

`BookResponse`

- `id`
- `libraryId`
- `categoryId`
- `title`
- `author`
- `isbn`
- `description`
- `coverImageReference`
- `totalCopies`
- `availableCopies`
- `isAvailable`
- `averageRating`
- `ratingCount`
- `coverImageUrl`

### Rental payloads
`CreateRentalRequest`

- `userAccountId`
- `bookId`
- `libraryId`
- `rentedOn`
- `deliveryAddress`

Action payloads:

- `RequestReturnRequest`
- `CompleteReturnRequest`
- `MarkOverdueRequest`

`RentalResponse`

- `id`
- `userAccountId`
- `bookId`
- `libraryId`
- `status`
- `rentedOn`
- `dueOn`
- `returnRequestedOn`
- `returnedOn`
- `deliveryAddress`

### Delivery payloads
`CreateDeliveryTaskRequest`

- `rentalId`
- `deliveryPartnerAccountId`
- `type`
- `assignedAt`

`CompleteDeliveryTaskRequest`

- `completedAt`

`DeliveryTaskResponse`

- `id`
- `rentalId`
- `deliveryPartnerAccountId`
- `type`
- `status`
- `assignedAt`
- `completedAt`
- `deliveryAddress`

### Review payloads
`CreateBookReviewRequest`

- `bookId`
- `reviewerAccountId`
- `rating`
- `title`
- `content`
- `createdOn`

`BookReviewResponse`

- `id`
- `bookId`
- `reviewerAccountId`
- `rating`
- `title`
- `content`
- `createdOn`
- `updatedOn`

### Admin payloads
`AdminOverviewResponse`

- `totalUsers`
- `totalLibraries`
- `totalCategories`
- `totalBooks`
- `availableBooks`
- `activeRentals`
- `overdueRentals`
- `openDeliveryTasks`
- `completedDeliveryTasks`
- `totalReviews`

## Status Codes
- `200 OK` for reads and successful actions
- `201 Created` for successful creates
- `400 Bad Request` for invalid request shape or invalid domain transitions
- `404 Not Found` when target aggregate is missing
- `409 Conflict` for duplicate email, duplicate ISBN, duplicate review, or other uniqueness violations
- `422 Validation Problem` when request values fail domain/value-object validation

## Implementation Notes
- Current implementation uses Minimal APIs in `FindBook.Web`
- Data access goes through `AppDbContext`
- The contributor template endpoints are not part of the product API
- Seed data is included for development so the frontend and Swagger can exercise the endpoints immediately
