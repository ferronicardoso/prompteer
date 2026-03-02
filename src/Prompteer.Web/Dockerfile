# ─── Stage 1: Build CSS ───────────────────────────────────────────────────────
FROM node:22-alpine AS css-build

WORKDIR /app/web

COPY Prompteer.Web/package.json Prompteer.Web/package-lock.json* ./
RUN npm ci

COPY Prompteer.Web/tailwind.config.js ./
COPY Prompteer.Web/wwwroot/css/input.css ./wwwroot/css/input.css
COPY Prompteer.Web/Views/ ./Views/

RUN npm run build:css

# ─── Stage 2: Build .NET ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

WORKDIR /src

# Copy solution + csproj files first (layer cache for restore)
COPY Prompteer.slnx ./
COPY Prompteer.Domain/Prompteer.Domain.csproj             Prompteer.Domain/
COPY Prompteer.Application/Prompteer.Application.csproj   Prompteer.Application/
COPY Prompteer.Infrastructure/Prompteer.Infrastructure.csproj Prompteer.Infrastructure/
COPY Prompteer.Web/Prompteer.Web.csproj                   Prompteer.Web/
COPY Prompteer.Application.Tests/Prompteer.Application.Tests.csproj Prompteer.Application.Tests/
COPY Prompteer.Domain.Tests/Prompteer.Domain.Tests.csproj            Prompteer.Domain.Tests/

RUN dotnet restore

# Copy source
COPY . .

# Copy compiled CSS from stage 1
COPY --from=css-build /app/web/wwwroot/css/app.css Prompteer.Web/wwwroot/css/app.css

RUN dotnet publish Prompteer.Web/Prompteer.Web.csproj \
    -c Release \
    -o /app/publish

# ─── Stage 3: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

# Non-root user (Debian-based image usa groupadd/useradd)
RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup appuser

COPY --from=dotnet-build --chown=appuser:appgroup /app/publish ./

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "Prompteer.Web.dll"]
