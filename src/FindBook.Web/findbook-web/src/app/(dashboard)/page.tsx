import { HomeOverview } from "@/features/book-rental";
import { getHomeOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function HomePage() {
  const data = await getHomeOverviewData();
  return <HomeOverview data={data} />;
}
