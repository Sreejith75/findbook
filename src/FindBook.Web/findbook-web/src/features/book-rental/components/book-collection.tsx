"use client";

import { useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";

import { FilterChips } from "@/components/shared/filter-chips";
import { BookCard } from "@/features/book-rental/components/book-card";
import { BookDetailDialog } from "@/features/book-rental/components/book-detail-dialog";
import { RentBookDialog } from "@/features/book-rental/components/rent-book-dialog";
import type { ApiUser, Book } from "@/features/book-rental/types/book-rental.types";

type BookCollectionProps = {
  books: Book[];
  filterOptions: string[];
  searchMode?: boolean;
  user?: ApiUser;
};

export function BookCollection({
  books,
  filterOptions,
  searchMode = false,
  user,
}: BookCollectionProps) {
  const searchParams = useSearchParams();
  const [activeFilter, setActiveFilter] = useState(filterOptions[0] ?? "All");
  const [selectedBook, setSelectedBook] = useState<Book | null>(null);
  const [requestedBookIds, setRequestedBookIds] = useState<number[]>([]);
  const [bookToRent, setBookToRent] = useState<Book | null>(null);

  const activeQuery = searchMode ? searchParams.get("q")?.trim().toLowerCase() : "";

  const visibleBooks = useMemo(() => {
    const filteredByCategory =
      activeFilter === "All" || activeFilter === "All Genres"
        ? books
        : books.filter((book) => book.genre === activeFilter);

    if (!activeQuery) {
      return filteredByCategory;
    }

    return filteredByCategory.filter((book) => {
      const title = book.title.toLowerCase();
      const author = book.author.toLowerCase();
      return title.includes(activeQuery) || author.includes(activeQuery);
    });
  }, [activeFilter, activeQuery, books]);

  const handleRequest = (book: Book) => {
    setBookToRent(book);
  };

  const highlightedBook = visibleBooks[0] ?? null;
  const availableCount = visibleBooks.filter((book) => book.isAvailable).length;

  return (
    <>
      {highlightedBook ? (
        <section className="catalog-stage">
          <div className="catalog-stage-copy">
            <div className="catalog-stage-kicker">Curated spotlight</div>
            <h2 className="catalog-stage-title">{highlightedBook.title}</h2>
            <div className="catalog-stage-author">{highlightedBook.author}</div>
            <p className="catalog-stage-description">{highlightedBook.description}</p>
            <div className="catalog-stage-meta">
              <div className="catalog-stage-stat">
                <span className="catalog-stage-stat-label">Available now</span>
                <strong>{availableCount}</strong>
              </div>
              <div className="catalog-stage-stat">
                <span className="catalog-stage-stat-label">Genres in view</span>
                <strong>{new Set(visibleBooks.map((book) => book.genre)).size}</strong>
              </div>
              <div className="catalog-stage-stat">
                <span className="catalog-stage-stat-label">Rental plan</span>
                <strong>{highlightedBook.weeklyPrice}/week</strong>
              </div>
            </div>
            <div className="catalog-stage-tags">
              {highlightedBook.tags.map((tag) => (
                <span className="catalog-stage-tag" key={tag}>
                  {tag}
                </span>
              ))}
            </div>
            <div className="catalog-stage-actions">
              <button
                className="catalog-stage-button catalog-stage-button-primary"
                onClick={() => handleRequest(highlightedBook)}
                type="button"
              >
                Rent Spotlight Book
              </button>
              <button
                className="catalog-stage-button catalog-stage-button-secondary"
                onClick={() => setSelectedBook(highlightedBook)}
                type="button"
              >
                View Details
              </button>
            </div>
          </div>
          <button
            className="catalog-stage-visual"
            onClick={() => setSelectedBook(highlightedBook)}
            style={{ background: highlightedBook.accentColor }}
            type="button"
          >
            {highlightedBook.coverImageUrl ? (
              <img
                alt={`Cover of ${highlightedBook.title}`}
                className="catalog-stage-image"
                src={highlightedBook.coverImageUrl}
              />
            ) : (
              <div className="catalog-stage-emoji">{highlightedBook.emoji}</div>
            )}
            <div className="catalog-stage-badge">{highlightedBook.genre}</div>
          </button>
        </section>
      ) : null}

      <div className="catalog-toolbar">
        <div>
          <div className="catalog-toolbar-title">Browse shelves</div>
          <div className="catalog-toolbar-subtitle">
            {visibleBooks.length} titles in view, {availableCount} ready for delivery
            {activeQuery ? `, matching "${activeQuery}"` : ""}.
          </div>
        </div>
        <div className="catalog-toolbar-pill">{activeFilter}</div>
      </div>
      <FilterChips items={filterOptions} onChange={setActiveFilter} />
      <div className="books-grid">
        {visibleBooks.map((book) => {
          const isRequested = requestedBookIds.includes(book.id);

          return (
            <BookCard
              book={book}
              buttonLabel={isRequested ? "Requested" : undefined}
              disabled={isRequested}
              key={book.id}
              onOpen={setSelectedBook}
              onRent={handleRequest}
            />
          );
        })}
      </div>
      <BookDetailDialog
        book={selectedBook}
        isRequested={selectedBook ? requestedBookIds.includes(selectedBook.id) : false}
        onClose={() => setSelectedBook(null)}
        onRent={handleRequest}
      />
      {user ? (
        <RentBookDialog
          book={bookToRent}
          onClose={() => setBookToRent(null)}
          onSuccess={(bookId) =>
            setRequestedBookIds((current) =>
              current.includes(bookId) ? current : [...current, bookId],
            )
          }
          user={user}
        />
      ) : null}
    </>
  );
}
