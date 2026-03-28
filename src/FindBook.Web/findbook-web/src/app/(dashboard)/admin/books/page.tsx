import { PageHeader } from "@/components/shared/page-header";
import { AdminBookManagementPanel } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminBooksPage() {
  const data = await getAdminOverviewData();

  return (
    <div>
      <PageHeader
        description="Manage book records, stock levels, categories, and library assignments without the rest of the admin tools crowding the page."
        title="Admin Books"
      />
      <AdminBookManagementPanel
        books={data.books}
        categories={data.categories}
        libraries={data.libraries}
      />
    </div>
  );
}
