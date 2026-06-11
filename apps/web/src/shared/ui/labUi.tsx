import { AlertTriangle, CheckCircle2, Search, type LucideIcon } from 'lucide-react';
import type { ReactNode } from 'react';
import type {
  DocumentSummaryDto,
  LabTaskDto,
  SearchHitDto,
} from '../types/sylabTypes';

export function PageHeader({
  eyebrow,
  title,
  description,
  icon: Icon,
  action,
}: {
  eyebrow: string;
  title: string;
  description: string;
  icon?: LucideIcon;
  action?: ReactNode;
}) {
  return (
    <header className="topbar page-header">
      <div>
        <p className="eyebrow">{eyebrow}</p>
        <h1>
          {Icon ? <Icon size={28} /> : null}
          <span>{title}</span>
        </h1>
        <p className="topbar-copy">{description}</p>
      </div>
      {action}
    </header>
  );
}

export function Panel({
  id,
  eyebrow,
  title,
  icon,
  action,
  children,
  wide,
}: {
  id?: string;
  eyebrow: string;
  title: string;
  icon: ReactNode;
  action?: ReactNode;
  children: ReactNode;
  wide?: boolean;
}) {
  return (
    <section className={wide ? 'panel wide' : 'panel'} id={id}>
      <div className="panel-heading">
        <div>
          <p className="eyebrow">{eyebrow}</p>
          <h2>
            {icon}
            <span>{title}</span>
          </h2>
        </div>
        {action}
      </div>
      {children}
    </section>
  );
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
    </label>
  );
}

export function DocumentList({ documents }: { documents: DocumentSummaryDto[] }) {
  if (documents.length === 0) {
    return <EmptyState label="暂无资料" />;
  }

  return (
    <div className="list-block">
      {documents.map((document) => (
        <article className="document-row" key={document.id}>
          <div>
            <strong>{document.title}</strong>
            <p>{document.summary}</p>
          </div>
          <div className="row-meta">
            <StatusPill ready={document.status === 'parsed'} label={formatDocumentStatus(document.status)} />
            <span>{document.documentType}</span>
            <small>
              {document.chunkCount} 个片段 · {formatDate(document.createdAt)}
            </small>
          </div>
        </article>
      ))}
    </div>
  );
}

export function EvidenceList({ hits }: { hits: SearchHitDto[] }) {
  if (hits.length === 0) {
    return <EmptyState label="暂无证据片段" />;
  }

  return (
    <div className="evidence-list">
      {hits.map((hit) => (
        <article className="evidence-row" key={hit.citation.chunkId}>
          <div className="score">{hit.score.toFixed(1)}</div>
          <div>
            <strong>{hit.citation.documentTitle}</strong>
            <span>
              {hit.citation.section} · 片段 {hit.citation.chunkOrdinal}
            </span>
            <p>{hit.snippet}</p>
          </div>
        </article>
      ))}
    </div>
  );
}

export function KeyValueGrid({ title, values }: { title: string; values?: Record<string, string> }) {
  const entries = Object.entries(values ?? {});

  return (
    <div className="kv-block">
      <h3>{title}</h3>
      {entries.length === 0 ? (
        <p className="muted">等待抽取</p>
      ) : (
        <dl>
          {entries.map(([key, value]) => (
            <div key={key}>
              <dt>{key}</dt>
              <dd>{value}</dd>
            </div>
          ))}
        </dl>
      )}
    </div>
  );
}

export function Checklist({ title, items, tone }: { title: string; items?: string[]; tone?: 'warn' }) {
  return (
    <div className="checklist">
      <h3>{title}</h3>
      {(items?.length ? items : ['等待生成']).map((item) => (
        <div className={tone === 'warn' ? 'check-row warn' : 'check-row'} key={item}>
          {tone === 'warn' ? <AlertTriangle size={16} /> : <CheckCircle2 size={16} />}
          <span>{item}</span>
        </div>
      ))}
    </div>
  );
}

export function TaskList({ tasks }: { tasks: LabTaskDto[] }) {
  if (tasks.length === 0) {
    return <EmptyState label="暂无任务卡" />;
  }

  return (
    <div className="task-list">
      {tasks.map((task) => (
        <article className="task-row" key={task.id}>
          <div>
            <strong>{task.title}</strong>
            <span>{formatDate(task.createdAt)}</span>
          </div>
          <StatusPill ready={task.status === 'ready'} label={task.status} />
          <ol>
            {task.steps.slice(0, 4).map((step) => (
              <li key={step}>{step}</li>
            ))}
          </ol>
        </article>
      ))}
    </div>
  );
}

export function StatusPill({ ready, label }: { ready: boolean; label: string }) {
  return <span className={ready ? 'status-pill ready' : 'status-pill'}>{label}</span>;
}

function formatDocumentStatus(status: string) {
  if (status === 'parsed') {
    return '已解析';
  }

  return status;
}

export function EmptyState({ label }: { label: string }) {
  return (
    <div className="empty-state">
      <Search size={18} />
      <span>{label}</span>
    </div>
  );
}

export function formatDate(value: string) {
  return new Intl.DateTimeFormat('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value));
}
