import { useParams } from "react-router-dom";
import { WorkflowDefinitionsStudio } from "../features/workflows/WorkflowDefinitionsStudio";

export function WorkflowBuilderPage() {
  const { workflowId } = useParams<{ workflowId: string }>();

  if (!workflowId) {
    return null;
  }

  return <WorkflowDefinitionsStudio workflowId={workflowId} />;
}
