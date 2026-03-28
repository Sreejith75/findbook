import { PageHeader } from "@/components/shared/page-header";
import { AdminUserManagementPanel } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminUsersPage() {
  const data = await getAdminOverviewData();

  return (
    <div>
      <PageHeader
        description="Create new reader, admin, and delivery partner accounts or update existing users without the rest of the admin dashboard competing for attention."
        title="Admin Users"
      />
      <AdminUserManagementPanel libraries={data.libraries} users={data.users} />
    </div>
  );
}
