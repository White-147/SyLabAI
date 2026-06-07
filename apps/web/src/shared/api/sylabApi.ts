import type {
  CreateDocumentIngestionDto,
  CreateLabTaskDto,
  DocumentSummaryDto,
  GroundedAnswerDto,
  HealthDto,
  LabTaskDto,
  PathSuggestionDraftDto,
  ProviderStatusDto,
  SearchHitDto,
  StructuredExperimentRecordDto,
} from '../types/sylabTypes';

const configuredBaseUrl = import.meta.env.VITE_SYLABAI_API_BASE_URL as string | undefined;
const API_BASE_URL = (configuredBaseUrl?.replace(/\/$/, '') || 'http://127.0.0.1:5200');

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  });

  if (!response.ok) {
    const details = await response.text();
    throw new Error(details || `Request failed with status ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export const sylabApi = {
  baseUrl: API_BASE_URL,

  getHealth: () => request<HealthDto>('/api/health/'),

  getProviderStatus: () => request<ProviderStatusDto>('/api/settings/provider'),

  listDocuments: () => request<DocumentSummaryDto[]>('/api/documents/'),

  ingestDocument: (payload: CreateDocumentIngestionDto) =>
    request<DocumentSummaryDto>('/api/documents/ingestions', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),

  searchKnowledge: (query: string, limit = 5) =>
    request<SearchHitDto[]>('/api/knowledge/search', {
      method: 'POST',
      body: JSON.stringify({ query, limit }),
    }),

  askKnowledge: (question: string, evidenceLimit = 4) =>
    request<GroundedAnswerDto>('/api/knowledge/answers', {
      method: 'POST',
      body: JSON.stringify({ question, evidenceLimit }),
    }),

  extractExperiment: (title: string, rawNote: string) =>
    request<StructuredExperimentRecordDto>('/api/experiments/extractions', {
      method: 'POST',
      body: JSON.stringify({ title, rawNote }),
    }),

  createSuggestion: (objective: string, constraints: string) =>
    request<PathSuggestionDraftDto>('/api/suggestions/', {
      method: 'POST',
      body: JSON.stringify({ objective, constraints }),
    }),

  listTasks: () => request<LabTaskDto[]>('/api/lab-tasks/'),

  createTask: (payload: CreateLabTaskDto) =>
    request<LabTaskDto>('/api/lab-tasks/', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
};

