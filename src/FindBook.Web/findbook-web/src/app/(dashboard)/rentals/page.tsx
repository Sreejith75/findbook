import { RentalsOverview } from "@/features/book-rental";
import { getRentalsOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function RentalsPage() {
  const data = await getRentalsOverviewData();
  return <RentalsOverview data={data} />;
}
