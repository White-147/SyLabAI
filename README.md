<h1 align="center">SyLabAI</h1>

<p align="center">韶远实验 AI 助手 Demo，面向实验资料检索、历史实验记录结构化、路径建议和实验室任务下发的 Windows 内网 AI 应用方案。</p>

<p align="center">
  <a href="./README.md">简体中文</a> | <a href="./README.en.md">English</a>
</p>

<p align="center">
  <img alt="Status" src="https://img.shields.io/badge/status-portfolio%20demo-7952B3?style=for-the-badge">
  <img alt="Stack" src="https://img.shields.io/badge/stack-React%20%2B%20.NET%20%2B%20SQLite-2E7D32?style=for-the-badge">
  <img alt="AI" src="https://img.shields.io/badge/AI-DeepSeek%20API-2563EB?style=for-the-badge">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%20Server-0078D4?style=for-the-badge">
  <a href="./LICENSE"><img alt="License" src="https://img.shields.io/badge/license-Apache--2.0-blue?style=for-the-badge"></a>
</p>

SyLabAI 是一个面向实验室知识工作的公开 Demo 项目，定位为“韶远实验 AI 助手”的技术敲门砖。项目目标不是复刻真实企业内部系统，而是用一套可公开展示的工程骨架，表达我对 `AI + 实验资料 + 文档检索 + 实验路径辅助 + 人工确认闭环` 的理解。

当前仓库处于初始工程化阶段，已经整理出目录结构、架构约束、产品计划、数据隐私边界和参考项目说明；后续会逐步补充 React 前端、ASP.NET Core Control API、DeepSeek API 适配、文档解析、SQLite FTS 检索和实验路径建议 Demo。

> 说明：本仓库不包含任何真实企业内部资料、实验数据、供应商资料、API Key、业务截图或内部流程文件。所有后续演示数据都应使用脱敏样例或虚拟数据。真实业务系统如果落地，应在单位内部另起私有项目并重新做权限、数据、安全和部署设计。

## 项目功能规划

- 实验文档导入：上传 SOP、实验报告、历史记录、产品资料、公开文献或专利资料。
- 文档解析与切片：通过文档解析边界转换为可检索文本，并保留来源、页码、章节等追溯信息。
- 知识库问答：基于来源片段回答问题，返回引用和证据，避免只给无来源的 AI 结论。
- 实验记录结构化：抽取实验条件、参数、结果指标、失败原因和备注。
- 路径建议辅助：根据资料和历史记录生成候选实验路径、依据、假设、风险点和待确认事项。
- 实验室任务下发：生成任务卡或 SOP 草案，供实验人员人工确认和执行。
- 结果反馈闭环：记录人工执行后的结果，为下一轮检索和路径建议提供依据。

## 技术栈规划

| 模块 | 技术方向 |
| --- | --- |
| 前端 | React、TypeScript、Vite |
| 后端控制面 | ASP.NET Core Web API |
| 应用层 | C# Application Services、DTO、Use Case 边界 |
| 数据存储 | SQLite MVP；后续可评估 SQL Server Express |
| 检索 | SQLite FTS / 关键词检索优先，向量检索保留为可选扩展 |
| AI Provider | DeepSeek API，通过 OpenAI-compatible adapter 接入 |
| 文档解析 | MarkItDown 作为候选解析器，放在独立文档转换边界 |
| 部署目标 | Windows Server / 内网部署，IIS 或 Kestrel |
| 约束 | API-only、无 Docker 依赖、无 Linux 前提、无本地模型强依赖 |

## 系统架构

```mermaid
flowchart LR
    User["内部用户 / 实验相关人员"] --> Web["apps/web\nReact + TypeScript"]
    Web --> Api["Control API\nASP.NET Core"]
    Api --> App["Application\n用例编排"]
    App --> Domain["Domain\n实验文档 / 记录 / 任务模型"]
    App --> AI["Infrastructure.AI\nDeepSeek API Adapter"]
    App --> Docs["Infrastructure.Documents\n文档解析与切片边界"]
    App --> Db["Infrastructure.Sqlite\nSQLite + FTS"]
    Docs --> Converter["document-converter\nMarkItDown boundary"]
    App --> Worker["Worker\n导入 / 解析 / 抽取任务"]
    Worker --> AI
    Worker --> Docs
    Worker --> Db
```

SyLabAI 的第一版不会把 Docker、Linux、本地大模型或云端 SaaS 作为生产前提。前端只通过 Control API 访问后端能力；AI Provider、文档解析、文件系统、数据库和检索能力都放在明确边界内，避免 Demo 一开始就堆成难维护的脚本集合。

## 目录结构

```text
SyLabAI/
├── apps/
│   └── web/                              # React + TypeScript 前端
├── backend/
│   ├── dotnet/control-plane/             # ASP.NET Core Control API 与后端分层
│   │   ├── src/SyLabAI.ControlApi
│   │   ├── src/SyLabAI.Application
│   │   ├── src/SyLabAI.Domain
│   │   ├── src/SyLabAI.Infrastructure.AI
│   │   ├── src/SyLabAI.Infrastructure.Documents
│   │   ├── src/SyLabAI.Infrastructure.Sqlite
│   │   ├── src/SyLabAI.Worker
│   │   └── tests/SyLabAI.ControlApi.Tests
│   └── services/document-converter/      # 文档解析 sidecar / adapter 边界
├── docs/                                 # 架构、开发、约束、运维和参考项目说明
├── scripts/windows/                      # Windows 本地开发、验证和部署脚本
├── data/                                 # 本地 SQLite / runtime 数据，占位不提交真实数据
├── uploads/                              # 上传文件目录，占位不提交真实文件
├── outputs/                              # 生成报告 / 任务卡，占位不提交真实输出
├── .tools/ .cache/ .config/ .tmp/         # 项目本地工具、缓存、配置和临时目录
├── AGENTS.md
├── README.md
└── LICENSE
```

## 核心业务链路

```mermaid
sequenceDiagram
    participant U as 用户
    participant W as Web 前端
    participant A as Control API
    participant D as 文档解析边界
    participant DB as SQLite / FTS
    participant M as DeepSeek API

    U->>W: 上传实验资料 / 历史记录
    W->>A: 创建文档导入任务
    A->>D: 转换文档并生成文本片段
    D-->>A: 返回 chunk + metadata + provenance
    A->>DB: 保存文档、切片和检索索引
    U->>W: 提问或请求实验路径建议
    W->>A: 查询知识库 / 创建建议任务
    A->>DB: 检索相关片段和实验记录
    A->>M: 发送脱敏上下文和结构化提示
    M-->>A: 返回回答 / 路径建议草案
    A-->>W: 展示引用、依据、风险和人工确认状态
    U->>W: 确认 / 修改 / 下发任务卡
    W->>A: 保存人工确认和反馈
```

## 数据与隐私说明

本项目是公开作品集 Demo，因此默认不公开任何真实业务材料：

- 不提交真实实验记录、内部 SOP、供应商资料、客户资料或企业内部截图。
- 不提交 DeepSeek API Key、Provider Secret、数据库文件、日志、上传文件或生成报告。
- 不在 public DTO、日志、截图或文档中暴露本地绝对路径、原始 Provider Payload 或未脱敏 Prompt。
- 后续如需演示，会使用虚拟实验数据、公开资料或人工构造的合成样例。

更多边界见 [运维与证据边界](./docs/operations-and-evidence.md) 和 [工程约束](./docs/engineering-constraints.md)。

## 本地开发规划

当前仓库还没有 scaffold 具体代码。计划中的开发入口如下：

### 1. 后端

```powershell
cd <repo-root>
dotnet restore .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet build .\backend\dotnet\control-plane\SyLabAI.ControlPlane.sln
dotnet run --project .\backend\dotnet\control-plane\src\SyLabAI.ControlApi\SyLabAI.ControlApi.csproj
```

### 2. 前端

```powershell
cd <repo-root>\apps\web
npm install
npm run dev
```

### 3. 文档解析边界

```powershell
cd <repo-root>\backend\services\document-converter
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
```

实际命令会在对应模块 scaffold 后同步更新。

## 项目亮点

- 选题贴近真实产业场景：不是普通聊天机器人，而是围绕实验资料、历史结果、路径建议和人工确认闭环设计。
- Windows Server / 内网优先：适合企业内部工具形态，不强行依赖 Docker 或 Linux 运维。
- API-only：模型能力通过远程 API 接入，避免本地部署大模型带来的硬件和运维负担。
- 来源可追溯：知识问答和路径建议都强调 citations / provenance，而不是只输出无依据结论。
- 工程边界先行：在写业务代码前先定义 Control API、Application、Domain、Infrastructure 和 Document Converter 边界。
- 公开 Demo 与真实业务隔离：仓库只展示工程思路，不放真实企业数据。

## 文档导航

- [工程约束](./docs/engineering-constraints.md)
- [总体架构](./docs/architecture.md)
- [开发说明](./docs/development.md)
- [运维与证据边界](./docs/operations-and-evidence.md)
- [参考项目说明](./docs/reference-projects.md)
- [产品计划](./docs/product-plan.md)
- [Codex/开发入口约束](./AGENTS.md)

## 后续可改进方向

- scaffold ASP.NET Core Control API 与分层项目。
- scaffold React + TypeScript 前端工作台。
- 接入 DeepSeek API 的 OpenAI-compatible Provider Adapter。
- 构造公开、脱敏、可演示的实验资料样例。
- 实现 SQLite FTS 检索、文档引用和问答 Demo。
- 实现实验记录结构化抽取与路径建议草案。
- 补充 GitHub Actions 的基础构建和 Markdown 检查。
