import {
  BarChart3,
  ClipboardCheck,
  FileText,
  FlaskConical,
  Route,
  Search,
  Settings,
  type LucideIcon,
} from 'lucide-react';

export type AppNavItem = {
  path: string;
  label: string;
  description: string;
  icon: LucideIcon;
};

export const appNavItems: AppNavItem[] = [
  { path: '/dashboard', label: '工作台', description: '总览与交付链路', icon: BarChart3 },
  { path: '/documents', label: '资料中心', description: '导入、切片和来源管理', icon: FileText },
  { path: '/knowledge', label: '来源检索', description: '证据问答与片段追溯', icon: Search },
  { path: '/experiments', label: '实验记录', description: '条件与结果字段抽取', icon: FlaskConical },
  { path: '/suggestions', label: '工艺路径', description: '路径草案和风险点', icon: Route },
  { path: '/tasks', label: '任务交付', description: '任务卡与复核清单', icon: ClipboardCheck },
  { path: '/settings', label: '运行边界', description: 'Provider 与本地目录', icon: Settings },
];
