# SyLabAI Control API（Hugging Face Spaces Docker 部署用）
# 后端：.NET 10 + SQLite（默认存储，免装 SQL Server，库文件自动创建+种子数据）
# 注意：SyLabAI 仓库根目录已有此文件，HF Space 直接使用；前端（apps/web）另行部署 Static Space
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/dotnet/control-plane/SyLabAI.ControlPlane.sln ./
COPY backend/dotnet/control-plane/src ./src
RUN dotnet publish src/SyLabAI.ControlApi/SyLabAI.ControlApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 5200
ENV SYLABAI_STORAGE_PROVIDER=Sqlite
# HF Spaces 注入 $PORT，应用必须监听该端口；SQLite 文件库存于容器临时磁盘，重启自动重建+灌入种子数据
ENTRYPOINT ["sh", "-c", "exec dotnet SyLabAI.ControlApi.dll --urls http://0.0.0.0:${PORT:-5200}"]
