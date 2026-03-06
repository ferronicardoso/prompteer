# Prompteer

A structured prompt generator for AI agents (Claude Code, GitHub Copilot CLI, and similar tools).
Prompteer guides you through a 9-step wizard to collect context about your project and generates a ready-to-use Markdown prompt.

🔗 [Source code on GitHub](https://github.com/ferronicardoso/prompteer)

---

## Quick Start

### Pull the image

```bash
docker pull ferronicardoso/prompteer:latest
```

### Run with Docker Compose

Create a `docker-compose.yml` file:

```yaml
services:
  db:
    image: postgres:17-alpine
    restart: unless-stopped
    environment:
      POSTGRES_DB: prompteer
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d prompteer"]
      interval: 10s
      timeout: 5s
      retries: 5

  web:
    image: ferronicardoso/prompteer:latest
    restart: unless-stopped
    depends_on:
      db:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      POSTGRES_HOST: db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: prompteer
    ports:
      - "8080:8080"

volumes:
  postgres_data:
```

Then start it:

```bash
export POSTGRES_PASSWORD=your_strong_password_here
docker compose up -d
```

The application will be available at **http://localhost:8080**.

---

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `POSTGRES_HOST` | PostgreSQL host | ✅ Yes |
| `POSTGRES_USER` | PostgreSQL username | No — defaults to `postgres` |
| `POSTGRES_PASSWORD` | PostgreSQL password | ✅ Yes |
| `POSTGRES_DB` | PostgreSQL database name | No — defaults to `prompteer` |
| `ASPNETCORE_ENVIRONMENT` | Application environment (`Production`, `Development`) | No — defaults to `Production` |

### Connection string format

```
Host=<host>;Port=5432;Database=prompteer;Username=postgres;Password=<password>
```

---

## Microsoft Entra ID (Azure AD) — Optional

Corporate SSO can be configured from within the app after the first login (**Settings → Microsoft Entra ID**). No environment variables are required upfront.

---

## Reverse Proxy (Nginx)

When running behind Nginx, you must increase the proxy buffer sizes. The Microsoft Entra authentication cookie contains an encrypted JWT with all identity claims and can exceed Nginx's default buffer size (~4–8 KB), causing a **502 Bad Gateway** on the `/signin-oidc` callback.

Add the following to your `location /` block:

```nginx
server {
    listen 443 ssl;
    server_name your-domain.com;
    # ssl_certificate / ssl_certificate_key ...

    location / {
        proxy_pass http://localhost:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Required: Entra auth cookie exceeds Nginx default buffer size
        proxy_buffer_size       128k;
        proxy_buffers           4 256k;
        proxy_busy_buffers_size 256k;
    }
}
```

Without these settings, the login flow with Microsoft Entra will silently fail with 502 even though the application logs show successful token acquisition.

---

## First Run

On the very first startup, Prompteer automatically redirects to `/Setup` where you create the local admin account (display name, email, and password). No pre-seeded credentials — you define them on first use.

---

## Ports

| Port | Protocol | Description |
|------|----------|-------------|
| `8080` | HTTP | Web application |

---

## Available Tags

| Tag | Description |
|-----|-------------|
| `latest` | Most recent stable release |
| `1.x.x` | Specific version (semver) |

---

## Data Persistence

All application data is stored in PostgreSQL. Mount a named volume (as shown in the example above) to persist data across container restarts.

---

## License

MIT © [Raphael Augusto Ferroni Cardoso](https://github.com/ferronicardoso)
