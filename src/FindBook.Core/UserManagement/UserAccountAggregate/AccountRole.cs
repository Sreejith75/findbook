namespace FindBook.Core.UserManagement.UserAccountAggregate;

public class AccountRole : SmartEnum<AccountRole>
{
  public static readonly AccountRole User = new(nameof(User), 1, false);
  public static readonly AccountRole Admin = new(nameof(Admin), 2, true);
  public static readonly AccountRole SuperAdmin = new(nameof(SuperAdmin), 3, true);
  public static readonly AccountRole DeliveryPartner = new(nameof(DeliveryPartner), 4, false);

  public bool CanManageLibrary { get; }

  private AccountRole(string name, int value, bool canManageLibrary) : base(name, value)
  {
    CanManageLibrary = canManageLibrary;
  }

  public bool IsAdminLike => this == Admin || this == SuperAdmin;

  public bool CanManageAdminRoles => this == SuperAdmin;

  public bool CanManageOperationalUsers => IsAdminLike;

  public bool CanAccessAdminWorkspace => IsAdminLike;

  public bool CanManageCatalog => IsAdminLike;

  public bool CanManageDeliveries => IsAdminLike;

  public bool CanManageUsers => IsAdminLike;

  public bool CanOperateDeliveryTasks => this == DeliveryPartner || IsAdminLike;

  public bool CanViewDispatchQueue => this == DeliveryPartner || IsAdminLike;
}
