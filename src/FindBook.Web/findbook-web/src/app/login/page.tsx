import { LoginForm } from "@/features/auth";

export default function LoginPage() {
  return (
    <main className="auth-page-shell">
      <section className="auth-page-layout">
        <div className="auth-brand-panel">
          <div className="auth-brand-kicker">FindBook</div>
          <h1 className="auth-brand-title">Borrow your next book with less waiting and less clutter.</h1>
          <p className="auth-brand-copy">
            Browse curated titles, manage your rentals, and keep every delivery and return in one calm, simple space.
          </p>
          <div className="auth-brand-grid">
            <article className="auth-brand-card">
              <div className="auth-brand-card-label">Browse</div>
              <div className="auth-brand-card-value">Explore titles picked for everyday readers.</div>
            </article>
            <article className="auth-brand-card">
              <div className="auth-brand-card-label">Borrow</div>
              <div className="auth-brand-card-value">Request a book and follow it from shelf to doorstep.</div>
            </article>
            <article className="auth-brand-card">
              <div className="auth-brand-card-label">Return</div>
              <div className="auth-brand-card-value">Keep due dates, pickups, and reviews easy to manage.</div>
            </article>
          </div>
          <div className="auth-brand-note">
            Built to keep the reading journey clear, warm, and distraction-free.
          </div>
        </div>

        <section className="auth-page-panel">
          <div className="hero-panel-eyebrow">Secure Sign In</div>
          <h2 className="hero-panel-title">Sign in to continue.</h2>
          <p className="hero-panel-copy">
            Enter your account details to access your books, rentals, and reading activity.
          </p>
          <LoginForm />
        </section>
      </section>
    </main>
  );
}
