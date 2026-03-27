using FindBook.Core.SharedKernel;

namespace FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

public sealed class UserAccountByEmailSpec : Specification<UserAccount>, ISingleResultSpecification<UserAccount>
{
  public UserAccountByEmailSpec(EmailAddress email)
  {
    Query.Where(x => x.Email == email);
  }
}
