"use client";

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
  if (!book) {
    return null;
  }

  return (
    <div className="book-dialog" onClick={onClose}>
      <div className="book-dialog-card" onClick={(event) => event.stopPropagation()}>
        <div className="book-dialog-cover" style={{ background: book.accentColor }}>
          <button className="book-dialog-close" onClick={onClose} type="button">
            X
          </button>
          <div className="book-dialog-emoji">{book.emoji}</div>
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
          <Button
            className="book-button"
            disabled={isRequested || !book.isAvailable}
            onClick={() => onRent(book)}
          >
            {isRequested
              ? "Rental Requested"
              : book.isAvailable
                ? "Rent This Book - Free Delivery"
                : "Currently Unavailable"}
          </Button>
        </div>
      </div>
    </div>
  );
}
