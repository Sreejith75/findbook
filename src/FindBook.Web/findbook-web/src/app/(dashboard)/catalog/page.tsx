import { CatalogOverview } from "@/features/book-rental";
import { getCatalogOverviewData } from "@/features/book-rental/services/book-rental.service";

export const dynamic = "force-dynamic";

type CatalogPageProps = {
  searchParams: Promise<{
    q?: string;
  }>;
};

export default async function CatalogPage({ searchParams }: CatalogPageProps) {
  const params = await searchParams;
  const data = await getCatalogOverviewData(params.q);
  return <CatalogOverview data={data} />;
}
