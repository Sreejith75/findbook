import { RentalsOverview } from "@/features/book-rental";
import { getRentalsOverviewData } from "@/features/book-rental/services/book-rental.service";
import { requireReaderSession } from "@/lib/auth-session";

export const dynamic = "force-dynamic";

export default async function RentalsPage() {
  await requireReaderSession();
  const data = await getRentalsOverviewData();
  return <RentalsOverview data={data} />;
}
