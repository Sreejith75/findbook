import { DeliveriesOverview } from "@/features/book-rental";
import { getDeliveriesOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function DeliveriesPage() {
  const data = await getDeliveriesOverviewData();
  return <DeliveriesOverview data={data} />;
}
