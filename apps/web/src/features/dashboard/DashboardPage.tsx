import { BarChart3, ClipboardCheck, FileText, Loader2, RefreshCw, Route } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { DocumentSummaryDto, LabTaskDto, ProviderStatusDto } from '../../shared/types/sylabTypes';
import { DocumentList, PageHeader, Panel, TaskList } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const deliverySteps = [
  { title: '资料入库', detail: '实验文档、记录和公开样例先形成可追溯来源' },
  { title: '来源检索', detail: '用证据片段支撑回答，不把模型输出当最终结论' },
  { title: '记录结构化', detail: '抽取条件、观察、结果和需要复核的安全边界' },
  { title: '路径草案', detail: '围绕小试、中试和放大前检查生成建议步骤' },
  { title: '任务交付', detail: '沉淀任务卡、复核清单和后续反馈闭环' },
];

export default function DashboardPage() {
  const [loadState, setLoadState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [provider, setProvider] = useState<ProviderStatusDto | null>(null);
  const [documents, setDocuments] = useState<DocumentSummaryDto[]>([]);
  const [tasks, setTasks] = useState<LabTaskDto[]>([]);

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
      const [nextProvider, nextDocuments, nextTasks] = await Promise.all([
        sylabApi.getProviderStatus(),
        sylabApi.listDocuments(),
        sylabApi.listTasks(),
      ]);

      setProvider(nextProvider);
      setDocuments(nextDocuments);
      setTasks(nextTasks);
      setLoadState('ready');
    } catch (caught) {
      setLoadState('error');
      setError(caught instanceof Error ? caught.message : '无法连接 Control API');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Shaoyuan lab AI workspace"
        title="工作台总览"
        description="按韶远官网的产品、技术、生产交付叙事组织实验 AI 工作流，所有结论保留来源和人工复核状态。"
        icon={BarChart3}
        action={
          <button className="icon-action" type="button" onClick={() => void refreshOverview()} title="刷新 Control API 状态">
            {loadState === 'loading' ? <Loader2 className="spin" size={18} /> : <RefreshCw size={18} />}
            <span>刷新</span>
          </button>
        }
      />

      {error && <InlineAlert message={error} />}

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

      <div className="page-grid two">
        <Panel eyebrow="Product center" title="最新资料" icon={<FileText size={18} />}>
          <DocumentList documents={documents.slice(0, 3)} />
        </Panel>
        <Panel eyebrow="Production handoff" title="待处理任务" icon={<ClipboardCheck size={18} />}>
          <TaskList tasks={tasks.slice(0, 3)} />
        </Panel>
        <Panel eyebrow="Process research" title="交付原则" icon={<Route size={18} />} wide>
          <div className="principle-list">
            <span>只使用已导入资料和公开合成样例。</span>
            <span>路径建议保留依据、假设、风险点和人工复核状态。</span>
            <span>不把 AI 摘要呈现为最终实验结论。</span>
          </div>
        </Panel>
      </div>
    </div>
  );
}

function InlineAlert({ message }: { message: string }) {
  return (
    <div className="inline-alert" role="status">
      <span>{message}</span>
    </div>
  );
}
