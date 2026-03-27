import { AdminOverview } from "@/features/book-rental";
import { getAdminOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function AdminPage() {
  const data = await getAdminOverviewData();
  return <AdminOverview data={data} />;
}
