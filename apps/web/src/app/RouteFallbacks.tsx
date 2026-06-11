import { AlertTriangle } from 'lucide-react';
import { Link, useRouteError } from 'react-router-dom';

export function RouteErrorPage() {
  const error = useRouteError();
  const message = error instanceof Error ? error.message : '当前页面不可用';

  return (
    <main className="route-fallback">
      <AlertTriangle size={22} />
      <h1>页面暂不可用</h1>
      <p>{message}</p>
      <Link className="ghost-button route-fallback-action" to="/dashboard">
        返回工作台
      </Link>
    </main>
  );
}
