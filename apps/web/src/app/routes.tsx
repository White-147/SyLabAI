import { lazy } from 'react';
// hash 模式：静态托管（HF Static Space）下子路由刷新不 404
import { createHashRouter, Navigate } from 'react-router-dom';
import { AppShell } from './layouts/AppShell';
import { RouteErrorPage } from './RouteFallbacks';

const DashboardPage = lazy(() => import('../features/dashboard/DashboardPage'));
const DocumentLibraryPage = lazy(() => import('../features/document-library/DocumentLibraryPage'));
const KnowledgeChatPage = lazy(() => import('../features/knowledge-chat/KnowledgeChatPage'));
const ExperimentRecordsPage = lazy(() => import('../features/experiment-records/ExperimentRecordsPage'));
const PathSuggestionsPage = lazy(() => import('../features/path-suggestions/PathSuggestionsPage'));
const LabTasksPage = lazy(() => import('../features/lab-tasks/LabTasksPage'));
const SettingsPage = lazy(() => import('../features/settings/SettingsPage'));

export const router = createHashRouter([
  {
    path: '/',
    element: <AppShell />,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'documents', element: <DocumentLibraryPage /> },
      { path: 'knowledge', element: <KnowledgeChatPage /> },
      { path: 'experiments', element: <ExperimentRecordsPage /> },
      { path: 'suggestions', element: <PathSuggestionsPage /> },
      { path: 'tasks', element: <LabTasksPage /> },
      { path: 'settings', element: <SettingsPage /> },
      { path: '*', element: <RouteErrorPage /> },
    ],
  },
]);
