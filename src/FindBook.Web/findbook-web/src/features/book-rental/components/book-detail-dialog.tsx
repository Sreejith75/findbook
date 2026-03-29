"use client";

import { useState } from "react";

import { Button } from "@/components/ui/button";
import type { Book } from "@/features/book-rental/types/book-rental.types";

type BookDetailDialogProps = {
  book: Book | null;
  isRequested: boolean;
  onClose: () => void;
  onRent: (book: Book) => void;
};

export function BookDetailDialog({
  book,
  isRequested,
  onClose,
  onRent,
}: BookDetailDialogProps) {
  const [hasImageError, setHasImageError] = useState(false);

  if (!book) {
    return null;
  }

  const showCoverImage = Boolean(book.coverImageUrl) && !hasImageError;
  const availabilityLabel =
    typeof book.availableCopies === "number"
      ? `${book.availableCopies} copies available`
      : book.isAvailable
        ? "Available for rent"
        : "Currently unavailable";

  return (
    <div className="book-dialog" onClick={onClose}>
      <div className="book-dialog-card" onClick={(event) => event.stopPropagation()}>
        <div className="book-dialog-cover" style={{ background: book.accentColor }}>
          <button className="book-dialog-close" onClick={onClose} type="button">
            X
          </button>
          {showCoverImage ? (
            <img
              alt={`Cover of ${book.title}`}
              className="book-dialog-image"
              onError={() => setHasImageError(true)}
              src={book.coverImageUrl ?? undefined}
            />
          ) : (
            <div className="book-dialog-emoji">{book.emoji}</div>
          )}
          <div className="book-dialog-overlay">
            <span className="book-dialog-genre">{book.genre}</span>
            <span className={`book-dialog-availability ${book.isAvailable ? "book-dialog-availability-open" : "book-dialog-availability-closed"}`}>
              {availabilityLabel}
            </span>
          </div>
        </div>
        <div className="book-dialog-body">
          <h2 className="book-dialog-title">{book.title}</h2>
          <div className="book-dialog-author">{book.author}</div>
          <div className="book-dialog-tags">
            {book.tags.map((tag) => (
              <span className="book-dialog-tag" key={tag}>
                {tag}
              </span>
            ))}
          </div>
          <p className="book-dialog-copy">{book.description}</p>
          <div className="book-dialog-stats">
            <div className="book-dialog-stat">
              <div className="book-dialog-stat-value">{book.pages}</div>
              <div className="book-dialog-stat-label">Pages</div>
            </div>
            <div className="book-dialog-stat">
              <div className="book-dialog-stat-value">Star {book.rating}</div>
              <div className="book-dialog-stat-label">Rating</div>
            </div>
            <div className="book-dialog-stat">
              <div className="book-dialog-stat-value">{book.weeklyPrice}</div>
              <div className="book-dialog-stat-label">Per Week</div>
            </div>
          </div>
          <div className="book-dialog-footer">
            <div className="book-dialog-note">
              Delivery details are confirmed before checkout, and active rentals appear in your workspace immediately.
            </div>
            <Button
              className="book-button"
              disabled={isRequested || !book.isAvailable}
              onClick={() => onRent(book)}
            >
              {isRequested
                ? "Rental Requested"
                : book.isAvailable
                  ? "Rent This Book"
                  : "Currently Unavailable"}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
