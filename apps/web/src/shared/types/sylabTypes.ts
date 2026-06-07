export interface HealthDto {
  status: string;
  service: string;
  version: string;
  runtimeDirectories: string[];
  checkedAt: string;
}

export interface DocumentSummaryDto {
  id: string;
  title: string;
  documentType: string;
  status: string;
  summary: string;
  createdAt: string;
  chunkCount: number;
}

export interface CitationDto {
  documentId: string;
  chunkId: string;
  documentTitle: string;
  section: string;
  chunkOrdinal: number;
}

export interface SearchHitDto {
  citation: CitationDto;
  snippet: string;
  score: number;
}

export interface GroundedAnswerDto {
  answer: string;
  evidence: SearchHitDto[];
  caveats: string[];
  requiresHumanReview: boolean;
}

export interface StructuredExperimentRecordDto {
  id: string;
  title: string;
  conditions: Record<string, string>;
  results: Record<string, string>;
  observations: string[];
  evidence: SearchHitDto[];
  requiresHumanReview: boolean;
}

export interface PathSuggestionDraftDto {
  id: string;
  objective: string;
  proposedSteps: string[];
  assumptions: string[];
  risks: string[];
  evidence: SearchHitDto[];
  requiresHumanReview: boolean;
}

export interface LabTaskDto {
  id: string;
  title: string;
  status: string;
  steps: string[];
  reviewChecklist: string[];
  createdAt: string;
}

export interface ProviderStatusDto {
  provider: string;
  model: string;
  configured: boolean;
  mode: string;
  safetyGates: string[];
}

export interface CreateDocumentIngestionDto {
  title: string;
  documentType: string;
  content: string;
  summary?: string;
}

export interface CreateLabTaskDto {
  title: string;
  steps: string[];
  reviewChecklist: string[];
}

