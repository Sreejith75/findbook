using FindBook.Core.Delivery.DeliveryTaskAggregate;
using FindBook.Core.Feedback.BookReviewAggregate;
using FindBook.Core.LibraryInventory.BookAggregate;
using FindBook.Core.LibraryInventory.CategoryAggregate;
using FindBook.Core.LibraryInventory.LibraryAggregate;
using FindBook.Core.Rental.RentalAggregate;
using FindBook.Core.SharedKernel;
using FindBook.Core.UserManagement.UserAccountAggregate;
using Vogen;

namespace FindBook.Infrastructure.Data.Config;

// Vogen EF Core source-generated converters for all strongly-typed ID and value object types.
// Note: ContributorAggregate is intentionally excluded from FindBook.Core compilation.

[EfCoreConverter<UserAccountId>]
[EfCoreConverter<SavedAddressId>]
[EfCoreConverter<LibraryId>]
[EfCoreConverter<LibraryName>]
[EfCoreConverter<CategoryId>]
[EfCoreConverter<CategoryName>]
[EfCoreConverter<BookId>]
[EfCoreConverter<BookTitle>]
[EfCoreConverter<AuthorName>]
[EfCoreConverter<Isbn>]
[EfCoreConverter<BookDescription>]
[EfCoreConverter<CoverImageReference>]
[EfCoreConverter<RentalId>]
[EfCoreConverter<BookReviewId>]
[EfCoreConverter<ReviewTitle>]
[EfCoreConverter<ReviewContent>]
[EfCoreConverter<StarRating>]
[EfCoreConverter<DeliveryTaskId>]
[EfCoreConverter<PersonName>]
[EfCoreConverter<EmailAddress>]
[EfCoreConverter<PasswordHash>]
internal partial class VogenEfCoreConverters;

