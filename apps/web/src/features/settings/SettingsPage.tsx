import {
  AlertTriangle,
  CheckCircle2,
  KeyRound,
  Loader2,
  PlugZap,
  RefreshCw,
  Settings,
  ShieldCheck,
} from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { sylabApi } from '../../shared/api/sylabApi';
import type {
  HealthDto,
  ProviderConnectivityTestResultDto,
  ProviderModelListResultDto,
  ProviderModelOptionDto,
  ProviderStatusDto,
} from '../../shared/types/sylabTypes';
import { Checklist, Field, KeyValueGrid, PageHeader, Panel, StatusPill, formatDate } from '../../shared/ui/labUi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

const defaultForm = {
  baseUrl: 'https://api.deepseek.com',
  model: 'deepseek-v4-pro',
  apiKey: '',
  liveCallsEnabled: false,
};

export default function SettingsPage() {
  const [loadState, setLoadState] = useState<LoadState>('idle');
  const [saveState, setSaveState] = useState<LoadState>('idle');
  const [modelState, setModelState] = useState<LoadState>('idle');
  const [testState, setTestState] = useState<LoadState>('idle');
  const [error, setError] = useState<string | null>(null);
  const [health, setHealth] = useState<HealthDto | null>(null);
  const [provider, setProvider] = useState<ProviderStatusDto | null>(null);
  const [modelsResult, setModelsResult] = useState<ProviderModelListResultDto | null>(null);
  const [testResult, setTestResult] = useState<ProviderConnectivityTestResultDto | null>(null);
  const [form, setForm] = useState(defaultForm);

  const models = modelsResult?.models ?? [];
  const canFetchModels = form.baseUrl.trim().length > 0;
  const canSave = form.baseUrl.trim().length > 0 && saveState !== 'loading';

  const providerValues = useMemo(
    () => ({
      Provider: provider?.provider ?? 'DeepSeek',
      Model: provider?.model ?? form.model,
      'Base URL': provider?.baseUrl ?? form.baseUrl,
      'Key source': formatKeySource(provider?.apiKeySource ?? 'none'),
      'Live calls': provider?.liveCallsEnabled ? 'enabled' : 'disabled',
    }),
    [form.baseUrl, form.model, provider],
  );

  useEffect(() => {
    void refreshSettings();
  }, []);

  async function refreshSettings() {
    setLoadState('loading');
    setError(null);

    try {
      const [nextHealth, nextProvider] = await Promise.all([
        sylabApi.getHealth(),
        sylabApi.getProviderStatus(),
      ]);
      setHealth(nextHealth);
      applyProviderStatus(nextProvider);
      setLoadState('ready');
    } catch (caught) {
      setLoadState('error');
      setError(readCaughtMessage(caught, '运行边界加载失败'));
    }
  }

  async function saveProviderSettings() {
    setSaveState('loading');
    setError(null);

    try {
      const nextProvider = await sylabApi.updateProviderSettings({
        baseUrl: form.baseUrl.trim(),
        model: form.model.trim(),
        apiKey: form.apiKey.trim() || undefined,
        liveCallsEnabled: form.liveCallsEnabled,
      });
      applyProviderStatus(nextProvider);
      setSaveState('ready');
    } catch (caught) {
      setSaveState('error');
      setError(readCaughtMessage(caught, 'Provider 配置保存失败'));
    }
  }

  async function clearApiKey() {
    setSaveState('loading');
    setError(null);
    setModelsResult(null);
    setTestResult(null);

    try {
      const nextProvider = await sylabApi.clearProviderApiKey();
      applyProviderStatus(nextProvider);
      setSaveState('ready');
    } catch (caught) {
      setSaveState('error');
      setError(readCaughtMessage(caught, 'API Key 清除失败'));
    }
  }

  async function fetchModels() {
    setModelState('loading');
    setError(null);
    setModelsResult(null);

    try {
      const nextProvider = await sylabApi.updateProviderSettings({
        baseUrl: form.baseUrl.trim(),
        model: form.model.trim(),
        apiKey: form.apiKey.trim() || undefined,
        liveCallsEnabled: form.liveCallsEnabled,
      });
      applyProviderStatus(nextProvider);
      const nextModels = await sylabApi.listProviderModels();
      setModelsResult(nextModels);
      if (nextModels.models.length > 0) {
        setForm((current) => ({
          ...current,
          model: pickModel(nextModels.models, current.model),
        }));
      }
      setModelState(nextModels.status === 'connected' ? 'ready' : 'error');
    } catch (caught) {
      setModelState('error');
      setError(readCaughtMessage(caught, '模型列表获取失败'));
    }
  }

  async function testConnectivity() {
    setTestState('loading');
    setError(null);
    setTestResult(null);

    try {
      const nextResult = await sylabApi.testProviderConnectivity();
      setTestResult(nextResult);
      setTestState(nextResult.status === 'connected' ? 'ready' : 'error');
    } catch (caught) {
      setTestState('error');
      setError(readCaughtMessage(caught, '联通测试失败'));
    }
  }

  function applyProviderStatus(nextProvider: ProviderStatusDto) {
    setProvider(nextProvider);
    setForm((current) => ({
      ...current,
      baseUrl: nextProvider.baseUrl,
      model: nextProvider.model,
      apiKey: '',
      liveCallsEnabled: nextProvider.liveCallsEnabled,
    }));
  }

  return (
    <div className="page-stack">
      <PageHeader
        eyebrow="Control boundary"
        title="运行边界"
        description="配置 Provider 连接信息，密钥只进入后端受保护存储；模型列表由后端使用 Base URL 和 API Key 获取。"
        icon={Settings}
        action={
          <button className="icon-action" type="button" onClick={() => void refreshSettings()} title="刷新运行边界">
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
          eyebrow="Provider"
          title="DeepSeek API"
          icon={<KeyRound size={18} />}
          action={<StatusPill ready={provider?.configured ?? false} label={provider?.configured ? '已配置' : '未配置'} />}
        >
          <div className="form-stack provider-form">
            <Field label="Base URL">
              <input
                value={form.baseUrl}
                onChange={(event) => setForm({ ...form, baseUrl: event.target.value })}
                placeholder="https://api.deepseek.com"
              />
            </Field>
            <Field label="API Key">
              <input
                type="password"
                value={form.apiKey}
                onChange={(event) => setForm({ ...form, apiKey: event.target.value })}
                placeholder={provider?.configured ? '已配置，留空则不修改' : '输入 API Key'}
                autoComplete="off"
              />
            </Field>
            <Field label="模型">
              <select
                value={form.model}
                onChange={(event) => setForm({ ...form, model: event.target.value })}
                disabled={models.length === 0}
              >
                {models.length === 0 ? (
                  <option value={form.model}>保存 Key 后获取模型</option>
                ) : (
                  models.map((model) => (
                    <option key={model.id} value={model.id}>
                      {model.id}
                    </option>
                  ))
                )}
              </select>
            </Field>
            <label className="toggle-row">
              <input
                type="checkbox"
                checked={form.liveCallsEnabled}
                onChange={(event) => setForm({ ...form, liveCallsEnabled: event.target.checked })}
              />
              <span>允许真实生成调用</span>
            </label>
          </div>

          <div className="button-row">
            <button className="solid-button" type="button" onClick={() => void saveProviderSettings()} disabled={!canSave}>
              {saveState === 'loading' ? <Loader2 className="spin" size={17} /> : <CheckCircle2 size={17} />}
              <span>保存连接信息</span>
            </button>
            <button className="icon-action" type="button" onClick={() => void fetchModels()} disabled={!canFetchModels || modelState === 'loading'}>
              {modelState === 'loading' ? <Loader2 className="spin" size={17} /> : <RefreshCw size={17} />}
              <span>保存并获取模型</span>
            </button>
            <button className="ghost-button" type="button" onClick={() => void clearApiKey()} disabled={saveState === 'loading'}>
              清除 Key
            </button>
          </div>

          {modelsResult && (
            <StatusBlock
              result={modelsResult}
              connectedLabel={`已获取 ${modelsResult.models.length} 个模型`}
              fallbackLabel={formatProviderStatus(modelsResult.status)}
            />
          )}

          <KeyValueGrid title="当前状态" values={providerValues} />
        </Panel>

        <Panel eyebrow="Connectivity" title="联通测试" icon={<PlugZap size={18} />}>
          <div className="button-row compact-actions">
            <button className="solid-button" type="button" onClick={() => void testConnectivity()} disabled={testState === 'loading'}>
              {testState === 'loading' ? <Loader2 className="spin" size={17} /> : <PlugZap size={17} />}
              <span>测试联通</span>
            </button>
          </div>

          {testResult ? (
            <StatusBlock
              result={testResult}
              connectedLabel="Provider 联通成功"
              fallbackLabel={formatProviderStatus(testResult.status)}
            />
          ) : (
            <div className="test-result muted-result">
              <strong>等待测试</strong>
              <p>测试调用后端的 Provider 边界，不展示原始 Provider 响应，也不会在前端回显 API Key。</p>
            </div>
          )}

          <div className="settings-note">
            <ShieldCheck size={18} />
            <span>如果这里提示 Control API 未连接，问题在前后端连接；如果返回认证、余额、限流等状态，问题才在 Provider 侧。</span>
          </div>

          <Checklist title="项目内目录" items={health?.runtimeDirectories} />
        </Panel>
      </div>
    </div>
  );
}

function StatusBlock({
  result,
  connectedLabel,
  fallbackLabel,
}: {
  result: ProviderConnectivityTestResultDto | ProviderModelListResultDto;
  connectedLabel: string;
  fallbackLabel: string;
}) {
  const isConnected = result.status === 'connected';

  return (
    <div className={isConnected ? 'test-result ready' : 'test-result'}>
      <strong>{isConnected ? connectedLabel : fallbackLabel}</strong>
      <p>{formatProviderMessage(result.status, result.message)}</p>
      <span>
        {result.httpStatusCode ? `HTTP ${result.httpStatusCode} / ` : ''}
        {formatDate(result.checkedAt)}
      </span>
    </div>
  );
}

function pickModel(models: ProviderModelOptionDto[], currentModel: string) {
  return models.some((model) => model.id === currentModel) ? currentModel : models[0].id;
}

function readCaughtMessage(caught: unknown, fallback: string) {
  return caught instanceof Error ? caught.message : fallback;
}

function formatKeySource(source: string) {
  if (source === 'none') {
    return '未配置';
  }

  if (source === 'local-protected-file') {
    return '本地受保护配置';
  }

  if (source.startsWith('environment:')) {
    return source.replace('environment:', '环境变量 ');
  }

  if (source.startsWith('configuration:')) {
    return '后端配置';
  }

  return source;
}

function formatProviderStatus(status: string) {
  const labels: Record<string, string> = {
    connected: '联通成功',
    not_configured: '未配置 API Key',
    invalid_base_url: '地址无效',
    auth_failed: '认证失败',
    insufficient_balance: '余额或额度不足',
    rate_limited: '请求限流',
    provider_error: 'Provider 异常',
    empty_models: '未返回模型',
    timeout: '请求超时',
    unreachable: '无法连接 Provider',
  };

  return labels[status] ?? status;
}

function formatProviderMessage(status: string, message: string) {
  const labels: Record<string, string> = {
    connected: '请求已通过后端 Provider 边界完成。',
    not_configured: '请先保存 API Key，再获取模型或测试联通。',
    invalid_base_url: 'Base URL 必须是 HTTPS 绝对地址。',
    auth_failed: 'API Key 无效或没有权限。',
    insufficient_balance: 'Key 可达，但余额或额度不足。',
    rate_limited: 'Provider 返回限流，请稍后重试。',
    provider_error: 'Provider 返回非成功状态。',
    empty_models: 'Provider 可达，但响应中没有可用模型 ID。',
    timeout: 'Provider 请求超时。',
    unreachable: '后端无法连接到 Provider 地址。',
  };

  return labels[status] ?? message;
}
