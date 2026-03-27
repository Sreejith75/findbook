namespace FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

public sealed class UserAccountByIdSpec : Specification<UserAccount>, ISingleResultSpecification<UserAccount>
{
  public UserAccountByIdSpec(UserAccountId id)
  {
    Query.Where(x => x.Id == id);
  }
}
