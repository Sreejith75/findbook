import { LibrariesOverview } from "@/features/book-rental";
import { getLibrariesOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

export default async function LibrariesPage() {
  const data = await getLibrariesOverviewData();
  return <LibrariesOverview data={data} />;
}
