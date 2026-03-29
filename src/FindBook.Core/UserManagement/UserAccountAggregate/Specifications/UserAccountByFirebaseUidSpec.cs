namespace FindBook.Core.UserManagement.UserAccountAggregate.Specifications;

public sealed class UserAccountByFirebaseUidSpec : Specification<UserAccount>, ISingleResultSpecification<UserAccount>
{
  public UserAccountByFirebaseUidSpec(FirebaseUid firebaseUid)
  {
    Query.Where(x => x.FirebaseUid == firebaseUid);
  }
}
