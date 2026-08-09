# SyLabAI（Hugging Face Spaces / Zeabur / Koyeb 等容器平台通用）
# 单容器部署：ASP.NET Core 后端 + 前端 dist（wwwroot 同域托管）+ SQLite（自动建库+种子数据）
# 前端构建
FROM node:20-alpine AS web-build
WORKDIR /web
COPY apps/web/package.json apps/web/package-lock.json ./
RUN npm ci
COPY apps/web/ ./
RUN npm run build

# 后端构建（.NET 10 SDK）
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/dotnet/control-plane/SyLabAI.ControlPlane.sln ./
COPY backend/dotnet/control-plane/src ./src
RUN dotnet publish src/SyLabAI.ControlApi/SyLabAI.ControlApi.csproj -c Release -o /app/publish

# 运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=web-build /web/dist ./wwwroot
EXPOSE 5200
ENV SYLABAI_STORAGE_PROVIDER=Sqlite
# 平台注入 $PORT（HF/Koyeb/Zeabur 约定）；SQLite 文件库存于容器临时磁盘，重启自动重建+种子
ENTRYPOINT ["sh", "-c", "exec dotnet SyLabAI.ControlApi.dll --urls http://0.0.0.0:${PORT:-5200}"]
