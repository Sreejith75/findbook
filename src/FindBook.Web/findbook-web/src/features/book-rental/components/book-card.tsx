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
  const resolvedLabel = buttonLabel ?? (book.isAvailable ? "Rent Now" : "Unavailable");

  return (
    <article className="book-card">
      <button
        className="book-cover"
        onClick={() => onOpen(book)}
        style={{ background: book.accentColor }}
        type="button"
      >
        <span className="book-genre">{book.genre}</span>
        <span className="book-cover-emoji">{book.emoji}</span>
      </button>
      <div className="book-body">
        <h3 className="book-title">{book.title}</h3>
        <div className="book-author">{book.author}</div>
        <div className="book-meta">
          <div className="book-price">
            {book.weeklyPrice} <span className="book-price-note">/ week</span>
          </div>
          <div className="book-rating">Star {book.rating}</div>
        </div>
        <Button
          className="book-button"
          disabled={disabled || !book.isAvailable}
          onClick={() => onRent(book)}
          variant={book.isAvailable ? "ghost" : "outline"}
        >
          {resolvedLabel}
        </Button>
      </div>
    </article>
  );
}
