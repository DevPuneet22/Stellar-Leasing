import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import { AppShell } from "../components/layout/AppShell";
import { RequireAuth } from "../features/auth/RequireAuth";
import { DashboardPage } from "../pages/DashboardPage";
import { DocsPage } from "../pages/DocsPage";
import { LoginPage } from "../pages/LoginPage";
import { WorkflowBuilderPage } from "../pages/WorkflowBuilderPage";
import { WorkflowDetailPage } from "../pages/WorkflowDetailPage";
import { WorkflowDefinitionsPage } from "../pages/WorkflowDefinitionsPage";

function ProtectedShell() {
  return (
    <AppShell>
      <Outlet />
    </AppShell>
  );
}

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<ProtectedShell />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/workflows" element={<WorkflowDefinitionsPage />} />
          <Route path="/workflows/:workflowId" element={<WorkflowDetailPage />} />
          <Route path="/workflows/:workflowId/builder" element={<WorkflowBuilderPage />} />
          <Route path="/builder" element={<Navigate to="/workflows" replace />} />
          <Route path="/docs" element={<DocsPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
