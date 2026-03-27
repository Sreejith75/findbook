"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import { Button } from "@/components/ui/button";
import { FilterChips } from "@/components/shared/filter-chips";
import { ProgressBar } from "@/components/shared/progress-bar";
import { ReviewBookDialog } from "@/features/book-rental/components/review-book-dialog";
import { requestRentalReturn } from "@/features/book-rental/services/book-rental.client";
import type {
  RentalRecord,
  RentalsOverviewData,
} from "@/features/book-rental/types/book-rental.types";

type RentalsOverviewProps = {
  data: RentalsOverviewData;
};

function createRentalFilters(records: RentalRecord[]): string[] {
  const activeCount = records.filter(
    (record) => record.status === "active" || record.status === "transit" || record.status === "overdue",
  ).length;
  const returnedCount = records.filter((record) => record.status === "returned").length;

  return [
    `All (${records.length})`,
    `Active (${activeCount})`,
    `Returned (${returnedCount})`,
  ];
}

export function RentalsOverview({ data }: RentalsOverviewProps) {
  const router = useRouter();
  const rentalFilters = createRentalFilters(data.rentals);
  const [activeFilter, setActiveFilter] = useState(rentalFilters[0]);
  const [returnRequests, setReturnRequests] = useState<number[]>([]);
  const [reviewTarget, setReviewTarget] = useState<RentalRecord | null>(null);
  const [pendingRentalId, setPendingRentalId] = useState<number | null>(null);

  const visibleRentals = useMemo(() => {
    if (activeFilter.startsWith("Active")) {
      return data.rentals.filter(
        (record) =>
          record.status === "active" ||
          record.status === "transit" ||
          record.status === "overdue",
      );
    }

    if (activeFilter.startsWith("Returned")) {
      return data.rentals.filter((record) => record.status === "returned");
    }

    return data.rentals;
  }, [activeFilter, data.rentals]);

  return (
    <div>
      <FilterChips items={rentalFilters} onChange={setActiveFilter} />
      <div className="activity-list">
        {visibleRentals.map((record) => {
          const renewalRequested = returnRequests.includes(record.id);

          return (
            <RentalCard
              key={record.id}
              alreadyReviewed={data.reviewedBookIds.includes(record.bookId)}
              onOpenReview={() => setReviewTarget(record)}
              onTrackDelivery={() => router.push("/deliveries")}
              onRequestRenewal={() =>
                void (async () => {
                  setPendingRentalId(record.id);

                  try {
                    await requestRentalReturn(record.id);
                    setReturnRequests((current) =>
                      current.includes(record.id) ? current : [...current, record.id],
                    );
                    router.refresh();
                  } finally {
                    setPendingRentalId(null);
                  }
                })()
              }
              record={record}
              renewalRequested={renewalRequested || pendingRentalId === record.id}
            />
          );
        })}
      </div>
      <ReviewBookDialog
        bookId={reviewTarget?.bookId ?? null}
        onClose={() => setReviewTarget(null)}
        rental={reviewTarget}
      />
    </div>
  );
}

type RentalCardProps = {
  alreadyReviewed: boolean;
  onOpenReview: () => void;
  onRequestRenewal: () => void;
  onTrackDelivery: () => void;
  record: RentalRecord;
  renewalRequested: boolean;
};

function RentalCard({
  alreadyReviewed,
  onOpenReview,
  onRequestRenewal,
  onTrackDelivery,
  record,
  renewalRequested,
}: RentalCardProps) {
  const statusLabel =
    record.status === "transit"
      ? "In Transit"
      : record.status === "active"
        ? "Active"
        : record.status === "overdue"
          ? "Overdue"
        : "Returned";

  return (
    <article className="rental-card">
      <div className="rental-cover" style={{ background: record.accentColor }}>
        {record.emoji}
      </div>
      <div>
        <div style={{ fontSize: "16px", fontWeight: 700 }}>{record.title}</div>
        <div className="subtle-text">{record.author}</div>
        <div style={{ display: "flex", gap: "20px", margin: "10px 0 12px" }}>
          <span className="subtle-text">
            Rented: <strong>{record.rentedOn}</strong>
          </span>
          <span className="subtle-text">
            Due: <strong>{record.dueOn}</strong>
          </span>
        </div>
        {record.status !== "returned" ? (
          <ProgressBar label={`Progress ${record.progress}%`} value={record.progress} />
        ) : null}
        <div style={{ marginTop: "12px" }}>
          <span className={`status-pill status-pill-${record.status}`}>{statusLabel}</span>
        </div>
        <div className="rental-actions">
          {record.status === "active" || record.status === "overdue" ? (
            <Button
              disabled={renewalRequested}
              onClick={onRequestRenewal}
              variant={renewalRequested ? "outline" : "primary"}
            >
              {renewalRequested ? "Return Requested" : "Request Return"}
            </Button>
          ) : null}
          {record.status === "transit" ? (
            <Button onClick={onTrackDelivery} variant="outline">
              Track Delivery
            </Button>
          ) : null}
          {record.status === "returned" ? (
            <Button disabled={alreadyReviewed} onClick={onOpenReview} variant="outline">
              {alreadyReviewed ? "Review Submitted" : "Write Review"}
            </Button>
          ) : null}
        </div>
      </div>
    </article>
  );
}
