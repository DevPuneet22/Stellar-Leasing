import type { PropsWithChildren } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../../features/auth/AuthContext";

const links = [
  { to: "/", label: "Dashboard" },
  { to: "/workflows", label: "Workflows" },
  { to: "/docs", label: "Docs" },
];

export function AppShell({ children }: PropsWithChildren) {
  const { session, signOut } = useAuth();

  return (
    <div className="shell">
      <aside className="shell__sidebar">
        <div>
          <p className="shell__eyebrow">Workflow Platform</p>
          <h1 className="shell__title">Stellar Leasing</h1>
          <p className="shell__subtitle">
            Qntrl-style workflow orchestration with a modular monolith backend.
          </p>
        </div>

        <nav className="shell__nav" aria-label="Primary">
          {links.map((link) => (
            <NavLink
              key={link.to}
              className={({ isActive }) =>
                isActive ? "shell__nav-link shell__nav-link--active" : "shell__nav-link"
              }
              to={link.to}
            >
              {link.label}
            </NavLink>
          ))}
        </nav>

        {session ? (
          <div className="shell__account">
            <div>
              <strong>{session.displayName}</strong>
              <div className="shell__account-meta">
                {session.role} · {session.email}
              </div>
            </div>
            <button className="button button--secondary" onClick={signOut} type="button">
              Sign out
            </button>
          </div>
        ) : null}
      </aside>

      <main className="shell__content">{children}</main>
    </div>
  );
}
