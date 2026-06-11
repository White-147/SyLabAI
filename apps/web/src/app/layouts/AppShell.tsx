import { Beaker, Loader2, Server } from 'lucide-react';
import { Suspense, useEffect, useState } from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { appNavItems } from '../navigation';
import { sylabApi } from '../../shared/api/sylabApi';

type LoadState = 'idle' | 'loading' | 'ready' | 'error';

export function AppShell() {
  const [loadState, setLoadState] = useState<LoadState>('idle');

  useEffect(() => {
    let cancelled = false;

    async function refreshStatus() {
      setLoadState('loading');

      try {
        await sylabApi.getHealth();
        if (!cancelled) {
          setLoadState('ready');
        }
      } catch {
        if (!cancelled) {
          setLoadState('error');
        }
      }
    }

    void refreshStatus();

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <main className="app-shell">
      <aside className="side-rail">
        <NavLink className="brand-lockup" to="/dashboard" aria-label="SyLabAI dashboard">
          <div className="brand-mark">
            <Beaker size={22} strokeWidth={2.2} />
          </div>
          <div>
            <strong>SyLabAI</strong>
            <span>韶远实验 AI 工作台</span>
          </div>
        </NavLink>

        <nav className="rail-nav" aria-label="SyLabAI menu">
          {appNavItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.path}
                className={({ isActive }) => (isActive ? 'rail-link active' : 'rail-link')}
                to={item.path}
              >
                <Icon size={18} />
                <span>{item.label}</span>
              </NavLink>
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

      <section className="app-content">
        <Suspense
          fallback={
            <div className="route-loading" role="status">
              <Loader2 className="spin" size={20} />
              <span>页面加载中</span>
            </div>
          }
        >
          <Outlet />
        </Suspense>
      </section>
    </main>
  );
}
