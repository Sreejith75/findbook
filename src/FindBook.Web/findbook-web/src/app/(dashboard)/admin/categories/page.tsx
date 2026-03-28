import { PageHeader } from "@/components/shared/page-header";
import { AdminCategoryManagementPanel } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminCategoriesPage() {
  const data = await getAdminOverviewData();

  return (
    <div>
      <PageHeader
        description="Keep the catalog structure tidy by creating and refining categories in a dedicated workspace."
        title="Admin Categories"
      />
      <AdminCategoryManagementPanel categories={data.categories} />
    </div>
  );
}
