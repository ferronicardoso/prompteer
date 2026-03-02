# ─── Stage 1: Build CSS ───────────────────────────────────────────────────────
FROM node:22-alpine AS css-build

WORKDIR /app/web

COPY src/Prompteer.Web/package.json src/Prompteer.Web/package-lock.json* ./
RUN npm ci

COPY src/Prompteer.Web/tailwind.config.js ./
COPY src/Prompteer.Web/wwwroot/css/input.css ./wwwroot/css/input.css
COPY src/Prompteer.Web/Views/ ./Views/

RUN npm run build:css

# ─── Stage 2: Build .NET ──────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dotnet-build

WORKDIR /src

# Copy solution + csproj files first (layer cache for restore)
COPY Prompteer.slnx ./
COPY src/Prompteer.Domain/Prompteer.Domain.csproj             src/Prompteer.Domain/
COPY src/Prompteer.Application/Prompteer.Application.csproj   src/Prompteer.Application/
COPY src/Prompteer.Infrastructure/Prompteer.Infrastructure.csproj src/Prompteer.Infrastructure/
COPY src/Prompteer.Web/Prompteer.Web.csproj                   src/Prompteer.Web/
COPY tests/Prompteer.Application.Tests/Prompteer.Application.Tests.csproj tests/Prompteer.Application.Tests/
COPY tests/Prompteer.Domain.Tests/Prompteer.Domain.Tests.csproj tests/Prompteer.Domain.Tests/

RUN dotnet restore

# Copy source
COPY src/ src/

# Copy compiled CSS from stage 1
COPY --from=css-build /app/web/wwwroot/css/app.css src/Prompteer.Web/wwwroot/css/app.css

RUN dotnet publish src/Prompteer.Web/Prompteer.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

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
