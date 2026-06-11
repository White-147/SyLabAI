import { ClipboardCheck, Route, Send } from 'lucide-react';
import { useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { PathSuggestionDraftDto } from '../../shared/types/sylabTypes';
import { Checklist, EvidenceList, Field, PageHeader, Panel } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

export default function PathSuggestionsPage() {
  const [actionState, setActionState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [objective, setObjective] = useState('生成一条小规模耐水性验证路径');
  const [constraints, setConstraints] = useState('仅使用公开合成样例；必须保留人工复核和安全确认。');
  const [suggestion, setSuggestion] = useState<PathSuggestionDraftDto | null>(null);

  async function createSuggestion() {
    setActionState('loading');
    setError(null);

    try {
      setSuggestion(await sylabApi.createSuggestion(objective, constraints));
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '生成失败');
    }
  }

  async function createTaskFromSuggestion() {
    if (!suggestion) {
      return;
    }

    setActionState('loading');
    setError(null);

    try {
      await sylabApi.createTask({
        title: `任务卡：${suggestion.objective}`,
        steps: suggestion.proposedSteps,
        reviewChecklist: ['来源片段已复核', '安全边界已确认', '结果反馈已安排'],
      });
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '任务卡创建失败');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Process research"
        title="工艺路径"
        description="路径建议和风险点独立成页，保持草案属性，后续可扩展假设确认、版本对比和审批状态。"
        icon={Route}
      />

      {error && (
        <div className="inline-alert" role="status">
          <span>{error}</span>
        </div>
      )}

      <div className="page-grid two">
        <Panel
          eyebrow="Draft"
          title="路径建议草案"
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
            <textarea rows={4} value={constraints} onChange={(event) => setConstraints(event.target.value)} />
          </Field>

          <button className="ghost-button" type="button" onClick={() => void createTaskFromSuggestion()} disabled={!suggestion || actionState === 'loading'}>
            <ClipboardCheck size={17} />
            <span>转成任务卡</span>
          </button>
        </Panel>

        <Panel eyebrow="Review package" title="草案复核包" icon={<ClipboardCheck size={18} />}>
          <Checklist title="建议步骤" items={suggestion?.proposedSteps} />
          <Checklist title="风险点" items={suggestion?.risks} tone="warn" />
          <Checklist title="假设" items={suggestion?.assumptions} />
        </Panel>

        <Panel eyebrow="Evidence" title="来源依据" icon={<Route size={18} />} wide>
          <EvidenceList hits={suggestion?.evidence ?? []} />
        </Panel>
      </div>
    </div>
  );
}
