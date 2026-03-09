import { Navigate, Route, Routes } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { BuilderPage } from "../pages/BuilderPage";
import { DashboardPage } from "../pages/DashboardPage";
import { DocsPage } from "../pages/DocsPage";

export function App() {
  return (
    <AppShell>
      <Routes>
        <Route path="/" element={<DashboardPage />} />
        <Route path="/builder" element={<BuilderPage />} />
        <Route path="/docs" element={<DocsPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </AppShell>
  );
}
