namespace FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

public sealed class ListUserAccountsSpec : Specification<UserAccount>
{
  public ListUserAccountsSpec()
  {
    Query.OrderBy(x => x.FullName);
  }
}
