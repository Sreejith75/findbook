import { PageHeader } from "@/components/shared/page-header";
import type { LibrariesOverviewData } from "@/features/book-rental/types/book-rental.types";

type LibrariesOverviewProps = {
  data: LibrariesOverviewData;
};

export function LibrariesOverview({ data }: LibrariesOverviewProps) {
  return (
    <div>
      <PageHeader
        description={`${data.libraries.length} partner libraries are connected to the live catalog and rental APIs.`}
        title="Partner Libraries"
      />
      <div className="library-list">
        {data.libraries.map((library) => (
          <article className="library-card" key={library.id}>
            <div className="library-mark" style={{ background: library.accentColor }}>
              {library.emoji}
            </div>
            <div>
              <div style={{ fontSize: "16px", fontWeight: 700 }}>{library.name}</div>
              <div className="subtle-text">{library.location}</div>
              <div className="subtle-text" style={{ marginTop: "8px" }}>
                Rating {library.rating} · Open 9AM–7PM
              </div>
            </div>
            <div style={{ textAlign: "right" }}>
              <div style={{ fontSize: "18px", fontWeight: 700 }}>
                {library.totalBooks.toLocaleString()}
              </div>
              <div className="subtle-text">Books</div>
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
