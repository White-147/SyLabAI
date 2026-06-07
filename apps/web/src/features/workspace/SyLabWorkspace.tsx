import {
  AlertTriangle,
  Beaker,
  BookOpen,
  CheckCircle2,
  ClipboardCheck,
  FileText,
  FlaskConical,
  Loader2,
  Plus,
  RefreshCw,
  Route,
  Search,
  Send,
  Server,
  Settings,
  ShieldCheck,
  Upload,
} from 'lucide-react';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type {
  DocumentSummaryDto,
  GroundedAnswerDto,
  HealthDto,
  LabTaskDto,
  PathSuggestionDraftDto,
  ProviderStatusDto,
  SearchHitDto,
  StructuredExperimentRecordDto,
} from '../../shared/types/sylabTypes';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const defaultDocument = {
  title: '合成导入样例：耐水性观察记录',
  documentType: 'synthetic-experiment-record',
  summary: '用于验证文档导入、切片和来源追溯的公开合成样例。',
  content:
    'material: sample C waterborne resin. temperature: 68 C. time: 75 min. observation: film surface stable, no visible sediment after 24 h. yield: 79%. 风险：需要复核温度漂移、批次差异和安全评审。该内容是公开 Demo 的合成样例。',
};

const defaultExperimentNote =
  'material: sample A + additive B\ntemperature: 65-70 C\ntime: 90 min\nobservation: 黏度稳定，未见明显沉降\nyield: 82%\n备注：放大前需要人工复核安全边界。';

const navItems = [
  { id: 'documents', label: '资料中心', icon: FileText },
  { id: 'knowledge', label: '来源检索', icon: Search },
  { id: 'experiments', label: '实验记录', icon: FlaskConical },
  { id: 'suggestions', label: '工艺路径', icon: Route },
  { id: 'tasks', label: '任务交付', icon: ClipboardCheck },
  { id: 'settings', label: '运行边界', icon: Settings },
];

const deliverySteps = [
  { title: '资料入库', detail: '实验文档、记录和公开样例先形成可追溯来源' },
  { title: '来源检索', detail: '用证据片段支撑回答，不把模型输出当最终结论' },
  { title: '记录结构化', detail: '抽取条件、观察、结果和需要复核的安全边界' },
  { title: '路径草案', detail: '围绕小试、中试和放大前检查生成建议步骤' },
  { title: '任务交付', detail: '沉淀任务卡、复核清单和后续反馈闭环' },
];

export function SyLabWorkspace() {
  const [loadState, setLoadState] = useState<LoadState>('idle');
  const [actionState, setActionState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [health, setHealth] = useState<HealthDto | null>(null);
  const [provider, setProvider] = useState<ProviderStatusDto | null>(null);
  const [documents, setDocuments] = useState<DocumentSummaryDto[]>([]);
  const [tasks, setTasks] = useState<LabTaskDto[]>([]);
  const [searchHits, setSearchHits] = useState<SearchHitDto[]>([]);
  const [answer, setAnswer] = useState<GroundedAnswerDto | null>(null);
  const [extraction, setExtraction] = useState<StructuredExperimentRecordDto | null>(null);
  const [suggestion, setSuggestion] = useState<PathSuggestionDraftDto | null>(null);

  const [documentForm, setDocumentForm] = useState(defaultDocument);
  const [question, setQuestion] = useState('温度窗口和人工复核要求是什么？');
  const [experimentTitle, setExperimentTitle] = useState('样例实验记录抽取');
  const [experimentNote, setExperimentNote] = useState(defaultExperimentNote);
  const [objective, setObjective] = useState('生成一条小规模耐水性验证路径');
  const [constraints, setConstraints] = useState('仅使用公开合成样例；必须保留人工复核和安全确认。');

  const overview = useMemo(
    () => [
      { label: '文档', value: documents.length.toString(), detail: '已索引资料' },
      {
        label: '切片',
        value: documents.reduce((total, document) => total + document.chunkCount, 0).toString(),
        detail: '带来源片段',
      },
      { label: '任务', value: tasks.length.toString(), detail: '草案和待复核' },
      { label: 'Provider', value: provider?.configured ? '已配置' : '演示', detail: provider?.model ?? 'DeepSeek' },
    ],
    [documents, provider, tasks],
  );

  useEffect(() => {
    void refreshOverview();
  }, []);

  async function refreshOverview() {
    setLoadState('loading');
    setError(null);

    try {
      const [nextHealth, nextProvider, nextDocuments, nextTasks] = await Promise.all([
        sylabApi.getHealth(),
        sylabApi.getProviderStatus(),
        sylabApi.listDocuments(),
        sylabApi.listTasks(),
      ]);

      setHealth(nextHealth);
      setProvider(nextProvider);
      setDocuments(nextDocuments);
      setTasks(nextTasks);
      setLoadState('ready');
    } catch (caught) {
      setLoadState('error');
      setError(caught instanceof Error ? caught.message : '无法连接 Control API');
    }
  }

  async function ingestDocument() {
    await runAction(async () => {
      const created = await sylabApi.ingestDocument(documentForm);
      setDocuments((current) => [created, ...current.filter((document) => document.id !== created.id)]);
    });
  }

  async function askQuestion() {
    await runAction(async () => {
      const [hits, nextAnswer] = await Promise.all([
        sylabApi.searchKnowledge(question, 5),
        sylabApi.askKnowledge(question, 4),
      ]);

      setSearchHits(hits);
      setAnswer(nextAnswer);
    });
  }

  async function extractExperiment() {
    await runAction(async () => {
      const record = await sylabApi.extractExperiment(experimentTitle, experimentNote);
      setExtraction(record);
    });
  }

  async function createSuggestion() {
    await runAction(async () => {
      const draft = await sylabApi.createSuggestion(objective, constraints);
      setSuggestion(draft);
    });
  }

  async function createTaskFromSuggestion() {
    if (!suggestion) {
      return;
    }

    await runAction(async () => {
      const task = await sylabApi.createTask({
        title: `任务卡：${suggestion.objective}`,
        steps: suggestion.proposedSteps,
        reviewChecklist: ['来源片段已复核', '安全边界已确认', '结果反馈已安排'],
      });

      setTasks((current) => [task, ...current]);
    });
  }

  async function runAction(work: () => Promise<void>) {
    setActionState('loading');
    setError(null);

    try {
      await work();
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '操作失败');
    }
  }

  function scrollToSection(sectionId: string) {
    document.getElementById(sectionId)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  return (
    <main className="app-shell">
      <aside className="side-rail">
        <div className="brand-lockup">
          <div className="brand-mark">
            <Beaker size={22} strokeWidth={2.2} />
          </div>
          <div>
            <strong>SyLabAI</strong>
            <span>韶远实验 AI 工作台</span>
          </div>
        </div>

        <nav className="rail-nav" aria-label="SyLabAI sections">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <button key={item.id} className="rail-button" type="button" onClick={() => scrollToSection(item.id)}>
                <Icon size={18} />
                <span>{item.label}</span>
              </button>
            );
          })}
        </nav>

        <div className="rail-status">
          <Server size={18} />
          <div>
            <span>Control API</span>
            <strong>{loadState === 'ready' ? '已连接' : loadState === 'loading' ? '连接中' : '待连接'}</strong>
          </div>
        </div>
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">Shaoyuan lab AI workspace</p>
            <h1>从实验资料到任务交付的来源可追溯工作台</h1>
            <p className="topbar-copy">
              面向小分子砌块、定制合成和工艺研究场景，辅助完成资料导入、证据检索、实验记录结构化、路径建议和人工复核任务卡。
            </p>
          </div>
          <button className="icon-action" type="button" onClick={() => void refreshOverview()} title="刷新 Control API 状态">
            {loadState === 'loading' ? <Loader2 className="spin" size={18} /> : <RefreshCw size={18} />}
            <span>刷新</span>
          </button>
        </header>

        {error && (
          <div className="inline-alert" role="status">
            <AlertTriangle size={18} />
            <span>{error}</span>
          </div>
        )}

        <section className="overview-grid" aria-label="Overview">
          {overview.map((metric) => (
            <div className="metric" key={metric.label}>
              <span>{metric.label}</span>
              <strong>{metric.value}</strong>
              <small>{metric.detail}</small>
            </div>
          ))}
        </section>

        <section className="delivery-strip" aria-label="Delivery workflow">
          <div className="delivery-strip-heading">
            <p className="eyebrow">从小试到交付</p>
            <h2>保留来源、复核边界、再进入下一步</h2>
          </div>
          <div className="delivery-steps">
            {deliverySteps.map((step, index) => (
              <article className="delivery-step" key={step.title}>
                <span>{String(index + 1).padStart(2, '0')}</span>
                <strong>{step.title}</strong>
                <p>{step.detail}</p>
              </article>
            ))}
          </div>
        </section>

        <div className="work-grid">
          <Panel
            id="documents"
            eyebrow="Product center"
            title="资料中心"
            icon={<Upload size={18} />}
            action={
              <button className="solid-button" type="button" onClick={() => void ingestDocument()} disabled={actionState === 'loading'}>
                <Plus size={17} />
                <span>导入样例</span>
              </button>
            }
          >
            <div className="form-stack">
              <Field label="标题">
                <input
                  value={documentForm.title}
                  onChange={(event) => setDocumentForm({ ...documentForm, title: event.target.value })}
                />
              </Field>
              <Field label="类型">
                <input
                  value={documentForm.documentType}
                  onChange={(event) => setDocumentForm({ ...documentForm, documentType: event.target.value })}
                />
              </Field>
              <Field label="内容">
                <textarea
                  rows={5}
                  value={documentForm.content}
                  onChange={(event) => setDocumentForm({ ...documentForm, content: event.target.value })}
                />
              </Field>
            </div>

            <DocumentList documents={documents} />
          </Panel>

          <Panel
            id="knowledge"
            eyebrow="Technology center"
            title="来源问答"
            icon={<BookOpen size={18} />}
            action={
              <button className="solid-button" type="button" onClick={() => void askQuestion()} disabled={actionState === 'loading'}>
                <Send size={17} />
                <span>检索</span>
              </button>
            }
          >
            <Field label="问题">
              <textarea rows={3} value={question} onChange={(event) => setQuestion(event.target.value)} />
            </Field>

            <div className="answer-block">
              <div className="answer-status">
                <ShieldCheck size={18} />
                <span>{answer ? (answer.requiresHumanReview ? '需要人工复核' : '可作为摘要参考') : '等待来源检索'}</span>
              </div>
              <p>{answer?.answer ?? '提交问题后，这里会显示基于来源片段的回答摘要。'}</p>
            </div>

            <EvidenceList hits={answer?.evidence.length ? answer.evidence : searchHits} />
          </Panel>

          <Panel
            id="experiments"
            eyebrow="Lab records"
            title="记录结构化"
            icon={<FlaskConical size={18} />}
            action={
              <button className="solid-button" type="button" onClick={() => void extractExperiment()} disabled={actionState === 'loading'}>
                <ClipboardCheck size={17} />
                <span>抽取</span>
              </button>
            }
          >
            <Field label="记录标题">
              <input value={experimentTitle} onChange={(event) => setExperimentTitle(event.target.value)} />
            </Field>
            <Field label="原始记录">
              <textarea rows={6} value={experimentNote} onChange={(event) => setExperimentNote(event.target.value)} />
            </Field>

            <KeyValueGrid title="实验条件" values={extraction?.conditions} />
            <KeyValueGrid title="结果字段" values={extraction?.results} />
          </Panel>

          <Panel
            id="suggestions"
            eyebrow="Process research"
            title="工艺路径草案"
            icon={<Route size={18} />}
            action={
              <button className="solid-button" type="button" onClick={() => void createSuggestion()} disabled={actionState === 'loading'}>
                <Send size={17} />
                <span>生成</span>
              </button>
            }
          >
            <Field label="目标">
              <input value={objective} onChange={(event) => setObjective(event.target.value)} />
            </Field>
            <Field label="约束">
              <textarea rows={3} value={constraints} onChange={(event) => setConstraints(event.target.value)} />
            </Field>

            <Checklist title="建议步骤" items={suggestion?.proposedSteps} />
            <Checklist title="风险点" items={suggestion?.risks} tone="warn" />

            <button className="ghost-button" type="button" onClick={() => void createTaskFromSuggestion()} disabled={!suggestion || actionState === 'loading'}>
              <ClipboardCheck size={17} />
              <span>转成任务卡</span>
            </button>
          </Panel>

          <Panel id="tasks" eyebrow="Production handoff" title="任务交付卡" icon={<ClipboardCheck size={18} />}>
            <TaskList tasks={tasks} />
          </Panel>

          <Panel id="settings" eyebrow="Control boundary" title="Provider 与运行边界" icon={<Settings size={18} />}>
            <div className="provider-line">
              <div>
                <span>{provider?.provider ?? 'DeepSeek'}</span>
                <strong>{provider?.model ?? 'deepseek-chat'}</strong>
              </div>
              <StatusPill ready={provider?.configured ?? false} label={provider?.mode ?? 'demo-no-live-calls'} />
            </div>

            <Checklist title="安全门" items={provider?.safetyGates} />
            <Checklist title="运行目录" items={health?.runtimeDirectories} />
          </Panel>
        </div>
      </section>
    </main>
  );
}

function Panel({
  id,
  eyebrow,
  title,
  icon,
  action,
  children,
}: {
  id: string;
  eyebrow: string;
  title: string;
  icon: ReactNode;
  action?: ReactNode;
  children: ReactNode;
}) {
  return (
    <section className="panel" id={id}>
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

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="field">
      <span>{label}</span>
      {children}
    </label>
  );
}

function DocumentList({ documents }: { documents: DocumentSummaryDto[] }) {
  return (
    <div className="list-block">
      {documents.map((document) => (
        <article className="document-row" key={document.id}>
          <div>
            <strong>{document.title}</strong>
            <p>{document.summary}</p>
          </div>
          <div className="row-meta">
            <span>{document.documentType}</span>
            <small>{document.chunkCount} chunks</small>
          </div>
        </article>
      ))}
    </div>
  );
}

function EvidenceList({ hits }: { hits: SearchHitDto[] }) {
  if (hits.length === 0) {
    return (
      <div className="empty-state">
        <Search size={18} />
        <span>暂无证据片段</span>
      </div>
    );
  }

  return (
    <div className="evidence-list">
      {hits.map((hit) => (
        <article className="evidence-row" key={hit.citation.chunkId}>
          <div className="score">{hit.score.toFixed(1)}</div>
          <div>
            <strong>{hit.citation.documentTitle}</strong>
            <span>
              {hit.citation.section} · chunk {hit.citation.chunkOrdinal}
            </span>
            <p>{hit.snippet}</p>
          </div>
        </article>
      ))}
    </div>
  );
}

function KeyValueGrid({ title, values }: { title: string; values?: Record<string, string> }) {
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

function Checklist({ title, items, tone }: { title: string; items?: string[]; tone?: 'warn' }) {
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

function TaskList({ tasks }: { tasks: LabTaskDto[] }) {
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

function StatusPill({ ready, label }: { ready: boolean; label: string }) {
  return <span className={ready ? 'status-pill ready' : 'status-pill'}>{label}</span>;
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  }).format(new Date(value));
}
