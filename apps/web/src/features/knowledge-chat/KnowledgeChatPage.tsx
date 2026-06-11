import { AlertTriangle, BookOpen, Loader2, Search, Send, ShieldCheck } from 'lucide-react';
import { useMemo, useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { GroundedAnswerDto, SearchHitDto } from '../../shared/types/sylabTypes';
import { EvidenceList, Field, PageHeader, Panel } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const sampleQuestions = ['温度窗口和人工复核要求是什么？', '哪些片段提到了来源边界？', '放大前需要复核哪些风险？'];

export default function KnowledgeChatPage() {
  const [actionState, setActionState] = useState<LoadState>('idle');
  const [mode, setMode] = useState<'search' | 'answer'>('answer');
  const [error, setError] = useState<string | null>(null);
  const [question, setQuestion] = useState(sampleQuestions[0]);
  const [lastQuery, setLastQuery] = useState<string | null>(null);
  const [searchHits, setSearchHits] = useState<SearchHitDto[]>([]);
  const [answer, setAnswer] = useState<GroundedAnswerDto | null>(null);

  const evidence = useMemo(
    () => (answer?.evidence.length ? answer.evidence : searchHits),
    [answer, searchHits],
  );
  const canSubmit = question.trim().length > 0 && actionState !== 'loading';

  async function searchSources() {
    setActionState('loading');
    setMode('search');
    setError(null);

    try {
      const nextHits = await sylabApi.searchKnowledge(question.trim(), 6);
      setSearchHits(nextHits);
      setAnswer(null);
      setLastQuery(question.trim());
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '检索失败');
    }
  }

  async function askQuestion() {
    setActionState('loading');
    setMode('answer');
    setError(null);

    try {
      const [hits, nextAnswer] = await Promise.all([
        sylabApi.searchKnowledge(question.trim(), 6),
        sylabApi.askKnowledge(question.trim(), 4),
      ]);

      setSearchHits(hits);
      setAnswer(nextAnswer);
      setLastQuery(question.trim());
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '问答失败');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Technology center"
        title="来源检索"
        description="围绕已索引片段进行检索和来源化回答，输出默认保留人工复核边界。"
        icon={BookOpen}
      />

      {error && (
        <div className="inline-alert" role="status">
          <AlertTriangle size={18} />
          <span>{error}</span>
        </div>
      )}

      <div className="page-grid two">
        <Panel eyebrow="Question" title="问题操作" icon={<BookOpen size={18} />}>
          <Field label="问题">
            <textarea rows={5} value={question} onChange={(event) => setQuestion(event.target.value)} />
          </Field>

          <div className="query-chip-row" aria-label="常用问题">
            {sampleQuestions.map((sample) => (
              <button className="query-chip" type="button" key={sample} onClick={() => setQuestion(sample)}>
                {sample}
              </button>
            ))}
          </div>

          <div className="button-row">
            <button className="ghost-button" type="button" onClick={() => void searchSources()} disabled={!canSubmit}>
              {actionState === 'loading' && mode === 'search' ? <Loader2 className="spin" size={17} /> : <Search size={17} />}
              <span>检索来源</span>
            </button>
            <button className="solid-button" type="button" onClick={() => void askQuestion()} disabled={!canSubmit}>
              {actionState === 'loading' && mode === 'answer' ? <Loader2 className="spin" size={17} /> : <Send size={17} />}
              <span>生成摘要</span>
            </button>
          </div>

          <div className="answer-block">
            <div className="answer-status">
              <ShieldCheck size={18} />
              <span>{answer ? (answer.requiresHumanReview ? '需要人工复核' : '可作为摘要参考') : '等待来源检索'}</span>
            </div>
            <p>{answer?.answer ?? '先检索来源或生成摘要，这里会显示基于片段的回答。'}</p>
          </div>

          {answer?.caveats.length ? (
            <div className="caveat-list">
              {answer.caveats.map((caveat) => (
                <span key={caveat}>{caveat}</span>
              ))}
            </div>
          ) : null}
        </Panel>

        <Panel
          eyebrow="Evidence"
          title="证据片段"
          icon={<ShieldCheck size={18} />}
          action={
            <span className="panel-count">
              {lastQuery ? `${evidence.length} 条` : '未检索'}
            </span>
          }
        >
          {lastQuery ? (
            <div className="search-summary">
              <Search size={16} />
              <span>{lastQuery}</span>
            </div>
          ) : null}
          <EvidenceList hits={evidence} />
        </Panel>
      </div>
    </div>
  );
}
