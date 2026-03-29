"use client";

import { useState } from "react";

import { Button } from "@/components/ui/button";
import type { Book } from "@/features/book-rental/types/book-rental.types";

type BookCardProps = {
  book: Book;
  buttonLabel?: string;
  disabled?: boolean;
  onOpen: (book: Book) => void;
  onRent: (book: Book) => void;
};

export function BookCard({
  book,
  buttonLabel,
  disabled = false,
  onOpen,
  onRent,
}: BookCardProps) {
  const [hasImageError, setHasImageError] = useState(false);
  const resolvedLabel = buttonLabel ?? (book.isAvailable ? "Rent Now" : "Unavailable");
  const showCoverImage = Boolean(book.coverImageUrl) && !hasImageError;
  const availabilityLabel =
    typeof book.availableCopies === "number"
      ? `${book.availableCopies} left`
      : book.isAvailable
        ? "Available"
        : "Unavailable";

  return (
    <article className="book-card">
      <button
        className="book-cover"
        onClick={() => onOpen(book)}
        style={{ background: book.accentColor }}
        type="button"
      >
        <div className="book-cover-topline">
          <span className="book-genre">{book.genre}</span>
          <span className={`book-availability ${book.isAvailable ? "book-availability-open" : "book-availability-closed"}`}>
            {availabilityLabel}
          </span>
        </div>
        {showCoverImage ? (
          <img
            alt={`Cover of ${book.title}`}
            className="book-cover-image"
            onError={() => setHasImageError(true)}
            src={book.coverImageUrl ?? undefined}
          />
        ) : (
          <span className="book-cover-emoji">{book.emoji}</span>
        )}
        <div className="book-cover-sheen" />
      </button>
      <div className="book-body">
        <div className="book-kicker">Shelf highlight</div>
        <h3 className="book-title">{book.title}</h3>
        <div className="book-author">{book.author}</div>
        <p className="book-blurb">{book.description}</p>
        <div className="book-meta">
          <div className="book-meta-item">
            <span className="book-meta-label">Rental</span>
            <div className="book-price">
              {book.weeklyPrice} <span className="book-price-note">/ week</span>
            </div>
          </div>
          <div className="book-meta-item book-meta-item-right">
            <span className="book-meta-label">Reader score</span>
            <div className="book-rating">Star {book.rating}</div>
          </div>
        </div>
        <div className="book-tags-row">
          {book.tags.slice(0, 2).map((tag) => (
            <span className="book-chip" key={tag}>
              {tag}
            </span>
          ))}
        </div>
        <div className="book-actions">
          <Button className="book-secondary-button" onClick={() => onOpen(book)} variant="outline">
            Details
          </Button>
          <Button
            className="book-button"
            disabled={disabled || !book.isAvailable}
            onClick={() => onRent(book)}
            variant={book.isAvailable ? "ghost" : "outline"}
          >
            {resolvedLabel}
          </Button>
        </div>
      </div>
    </article>
  );
}
