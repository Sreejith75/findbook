import { PageHeader } from "@/components/shared/page-header";
import { AdminLibraryManagementPanel } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminLibrariesPage() {
  const data = await getAdminOverviewData();

  return (
    <div>
      <PageHeader
        description="Update partner library details, contact information, and addresses from a single focused view."
        title="Admin Libraries"
      />
      <AdminLibraryManagementPanel libraries={data.libraries} />
    </div>
  );
}
