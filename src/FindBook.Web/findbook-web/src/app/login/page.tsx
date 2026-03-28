import { LoginForm } from "@/features/auth";

export default function LoginPage() {
  return (
    <main className="auth-page-shell">
      <section className="auth-page-panel">
        <div className="hero-panel-eyebrow">FindBook Access</div>
        <h1 className="hero-panel-title">Sign in to manage rentals, deliveries, and catalog activity.</h1>
        <p className="hero-panel-copy">
          Use your Firebase account to continue into the FindBook dashboard.
        </p>
        <LoginForm />
      </section>
    </main>
  );
}
