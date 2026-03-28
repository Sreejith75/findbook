import { PageHeader } from "@/components/shared/page-header";
import { AdminDeliveryTaskManagementPanel } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminDeliveryPage() {
  const data = await getAdminOverviewData();

  return (
    <div>
      <PageHeader
        description="Assign delivery work, move tasks through the workflow, and resolve failures from a delivery-specific control panel."
        title="Admin Delivery Tasks"
      />
      <AdminDeliveryTaskManagementPanel data={data} />
    </div>
  );
}
