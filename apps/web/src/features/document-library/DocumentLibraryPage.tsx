import { AlertTriangle, CheckCircle2, Database, FileText, Loader2, Plus, RefreshCw, Upload } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { DocumentSummaryDto } from '../../shared/types/sylabTypes';
import { DocumentList, Field, PageHeader, Panel, StatusPill, formatDate } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const defaultDocument = {
  title: '合成导入样例：耐水性观察记录',
  documentType: 'experiment-record',
  summary: '用于验证文本导入、切片和来源追踪的公开合成样例。',
  content:
    'material: sample C waterborne resin. temperature: 68 C. time: 75 min. observation: film surface stable, no visible sediment after 24 h. yield: 79%. 风险：需要复核温度漂移、批次差异和安全评审。该内容是公开 Demo 的合成样例。',
};

export default function DocumentLibraryPage() {
  const [loadState, setLoadState] = useState<LoadState>('idle');
  const [actionState, setActionState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [documents, setDocuments] = useState<DocumentSummaryDto[]>([]);
  const [documentForm, setDocumentForm] = useState(defaultDocument);
  const [lastIngestion, setLastIngestion] = useState<DocumentSummaryDto | null>(null);

  const contentLength = documentForm.content.trim().length;
  const indexedChunkCount = useMemo(
    () => documents.reduce((total, document) => total + document.chunkCount, 0),
    [documents],
  );
  const parsedCount = useMemo(
    () => documents.filter((document) => document.status === 'parsed').length,
    [documents],
  );
  const isSubmitDisabled =
    actionState === 'loading' ||
    documentForm.title.trim().length === 0 ||
    documentForm.documentType.trim().length === 0 ||
    contentLength === 0;

  useEffect(() => {
    void refreshDocuments();
  }, []);

  async function refreshDocuments() {
    setLoadState('loading');
    setError(null);

    try {
      setDocuments(await sylabApi.listDocuments());
      setLoadState('ready');
    } catch (caught) {
      setLoadState('error');
      setError(caught instanceof Error ? caught.message : '无法加载资料列表');
    }
  }

  async function ingestDocument() {
    setActionState('loading');
    setError(null);

    try {
      const created = await sylabApi.ingestDocument({
        ...documentForm,
        title: documentForm.title.trim(),
        documentType: documentForm.documentType.trim(),
        content: documentForm.content.trim(),
        summary: documentForm.summary.trim() || undefined,
      });
      setDocuments((current) => [created, ...current.filter((document) => document.id !== created.id)]);
      setLastIngestion(created);
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '导入失败');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Product center"
        title="资料中心"
        description="受控导入实验记录、SOP 片段和公开合成样例，完成切片后进入来源检索与任务交付链路。"
        icon={FileText}
        action={
          <button className="icon-action" type="button" onClick={() => void refreshDocuments()} title="刷新资料列表">
            {loadState === 'loading' ? <Loader2 className="spin" size={18} /> : <RefreshCw size={18} />}
            <span>刷新</span>
          </button>
        }
      />

      {error && (
        <div className="inline-alert" role="status">
          <AlertTriangle size={18} />
          <span>{error}</span>
        </div>
      )}

      <div className="page-grid two">
        <Panel
          eyebrow="Controlled ingestion"
          title="文本资料导入"
          icon={<Upload size={18} />}
          action={
            <button className="solid-button" type="button" onClick={() => void ingestDocument()} disabled={isSubmitDisabled}>
              {actionState === 'loading' ? <Loader2 className="spin" size={17} /> : <Plus size={17} />}
              <span>{actionState === 'loading' ? '导入中' : '导入'}</span>
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
            <Field label="摘要">
              <input
                value={documentForm.summary}
                onChange={(event) => setDocumentForm({ ...documentForm, summary: event.target.value })}
              />
            </Field>
            <Field label="内容">
              <textarea
                rows={9}
                value={documentForm.content}
                onChange={(event) => setDocumentForm({ ...documentForm, content: event.target.value })}
              />
            </Field>
          </div>
          <div className="content-meter">
            <span>{contentLength.toLocaleString('zh-CN')} / 40,000 字符</span>
            <span>{contentLength === 0 ? '等待内容' : '可提交'}</span>
          </div>
        </Panel>

        <Panel eyebrow="Ingestion state" title="索引状态" icon={<Database size={18} />}>
          <div className="compact-metrics">
            <div>
              <span>资料</span>
              <strong>{documents.length}</strong>
            </div>
            <div>
              <span>已解析</span>
              <strong>{parsedCount}</strong>
            </div>
            <div>
              <span>片段</span>
              <strong>{indexedChunkCount}</strong>
            </div>
          </div>

          {lastIngestion ? (
            <div className="ingestion-result">
              <CheckCircle2 size={18} />
              <div>
                <strong>{lastIngestion.title}</strong>
                <span>
                  {lastIngestion.chunkCount} 个片段，{formatDate(lastIngestion.createdAt)}
                </span>
              </div>
              <StatusPill ready={lastIngestion.status === 'parsed'} label={formatDocumentStatus(lastIngestion.status)} />
            </div>
          ) : (
            <div className="ingestion-result muted-result">
              <FileText size={18} />
              <div>
                <strong>等待本轮导入</strong>
                <span>列表数据来自 Control API 和 PostgreSQL 存储。</span>
              </div>
            </div>
          )}
        </Panel>

        <Panel wide eyebrow="Indexed sources" title="已索引来源" icon={<FileText size={18} />}>
          <DocumentList documents={documents} />
        </Panel>
      </div>
    </div>
  );
}

function formatDocumentStatus(status: string) {
  if (status === 'parsed') {
    return '已解析';
  }

  return status;
}
