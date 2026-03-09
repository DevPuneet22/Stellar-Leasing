import type { PropsWithChildren } from "react";
import { NavLink } from "react-router-dom";

const links = [
  { to: "/", label: "Dashboard" },
  { to: "/builder", label: "Builder" },
  { to: "/docs", label: "Docs" },
];

export function AppShell({ children }: PropsWithChildren) {
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
      </aside>

      <main className="shell__content">{children}</main>
    </div>
  );
}
