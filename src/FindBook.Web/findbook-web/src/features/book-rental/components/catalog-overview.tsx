import { PageHeader } from "@/components/shared/page-header";
import { BookCollection } from "@/features/book-rental/components/book-collection";
import type { CatalogOverviewData } from "@/features/book-rental/types/book-rental.types";

type CatalogOverviewProps = {
  data: CatalogOverviewData;
};

export function CatalogOverview({ data }: CatalogOverviewProps) {
  return (
    <div>
      <PageHeader
        description="A curated catalog built for renting, returning, and rediscovering strong reads across partner libraries."
        title="Discover Your Next Read"
      />
      <BookCollection
        books={data.books}
        filterOptions={data.filterOptions}
        searchMode
        user={data.user}
      />
    </div>
  );
}
