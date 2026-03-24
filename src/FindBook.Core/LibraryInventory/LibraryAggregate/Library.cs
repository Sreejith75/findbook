using FindBook.Core.SharedKernel;

namespace FindBook.Core.LibraryInventory.LibraryAggregate;

public class Library : EntityBase<Library, LibraryId>, IAggregateRoot
{
  private Library() { }

  public Library(LibraryName name, PostalAddress address, EmailAddress contactEmail, PhoneNumber contactPhone)
  {
    Name = name;
    Address = address;
    ContactEmail = contactEmail;
    ContactPhone = contactPhone;
  }

  public LibraryName Name { get; private set; }
  public PostalAddress Address { get; private set; } = null!;
  public EmailAddress ContactEmail { get; private set; }
  public PhoneNumber ContactPhone { get; private set; }

  public void UpdateDetails(LibraryName name, PostalAddress address, EmailAddress contactEmail, PhoneNumber contactPhone)
  {
    Name = name;
    Address = address;
    ContactEmail = contactEmail;
    ContactPhone = contactPhone;
  }
}
