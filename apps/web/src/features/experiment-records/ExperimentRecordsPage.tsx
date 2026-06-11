import { ClipboardCheck, FlaskConical } from 'lucide-react';
import { useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { StructuredExperimentRecordDto } from '../../shared/types/sylabTypes';
import { Field, KeyValueGrid, PageHeader, Panel } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const defaultExperimentNote =
  'material: sample A + additive B\ntemperature: 65-70 C\ntime: 90 min\nobservation: 黏度稳定，未见明显沉降\nyield: 82%\n备注：放大前需要人工复核安全边界。';

export default function ExperimentRecordsPage() {
  const [actionState, setActionState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [experimentTitle, setExperimentTitle] = useState('样例实验记录抽取');
  const [experimentNote, setExperimentNote] = useState(defaultExperimentNote);
  const [extraction, setExtraction] = useState<StructuredExperimentRecordDto | null>(null);

  async function extractExperiment() {
    setActionState('loading');
    setError(null);

    try {
      setExtraction(await sylabApi.extractExperiment(experimentTitle, experimentNote));
      setActionState('ready');
    } catch (caught) {
      setActionState('error');
      setError(caught instanceof Error ? caught.message : '抽取失败');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Lab records"
        title="实验记录"
        description="把原始实验记录单独放在实验记录路由内，后续字段校验、失败原因和反馈记录都在这里扩展。"
        icon={FlaskConical}
      />

      {error && (
        <div className="inline-alert" role="status">
          <span>{error}</span>
        </div>
      )}

      <div className="page-grid two">
        <Panel
          eyebrow="Extraction"
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
            <textarea rows={9} value={experimentNote} onChange={(event) => setExperimentNote(event.target.value)} />
          </Field>
        </Panel>

        <Panel eyebrow="Structured fields" title="抽取结果" icon={<ClipboardCheck size={18} />}>
          <KeyValueGrid title="实验条件" values={extraction?.conditions} />
          <KeyValueGrid title="结果字段" values={extraction?.results} />
        </Panel>
      </div>
    </div>
  );
}
