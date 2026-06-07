import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { SyLabWorkspace } from './features/workspace/SyLabWorkspace';
import './styles.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <SyLabWorkspace />
  </StrictMode>,
);

