import { ClipboardCheck, Loader2, RefreshCw } from 'lucide-react';
import { useEffect, useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type { LabTaskDto } from '../../shared/types/sylabTypes';
import { PageHeader, Panel, TaskList } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

export default function LabTasksPage() {
  const [loadState, setLoadState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [tasks, setTasks] = useState<LabTaskDto[]>([]);

  useEffect(() => {
    void refreshTasks();
  }, []);

  async function refreshTasks() {
    setLoadState('loading');
    setError(null);

    try {
      setTasks(await sylabApi.listTasks());
      setLoadState('ready');
    } catch (caught) {
      setLoadState('error');
      setError(caught instanceof Error ? caught.message : '任务列表加载失败');
    }
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Production handoff"
        title="任务交付"
        description="任务卡、复核清单和结果回传后续都归入这个路由，避免和路径草案页面互相堆叠。"
        icon={ClipboardCheck}
        action={
          <button className="icon-action" type="button" onClick={() => void refreshTasks()} title="刷新任务卡">
            {loadState === 'loading' ? <Loader2 className="spin" size={18} /> : <RefreshCw size={18} />}
            <span>刷新</span>
          </button>
        }
      />

      {error && (
        <div className="inline-alert" role="status">
          <span>{error}</span>
        </div>
      )}

      <Panel eyebrow="Lab tasks" title="任务卡列表" icon={<ClipboardCheck size={18} />}>
        <TaskList tasks={tasks} />
      </Panel>
    </div>
  );
}
