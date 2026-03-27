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

  return (
    <>
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
