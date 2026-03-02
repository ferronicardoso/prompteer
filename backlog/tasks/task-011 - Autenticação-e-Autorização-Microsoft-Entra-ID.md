---
id: task-011
title: Autenticação e Autorização com Microsoft Entra ID
status: Pending
assignee: []
created_date: '2026-06-01'
updated_date: '2026-06-01'
labels:
  - auth
  - security
  - entra
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implementar autenticação via Microsoft Entra ID (Azure AD) e controle de acesso por papéis (RBAC) em toda a aplicação Prompteer. Inclui cadastro de usuários sincronizado com o Entra, gestão de permissões e isolamento de dados por usuário.
<!-- SECTION:DESCRIPTION:END -->

## Subtasks

### auth-001 — Configuração Microsoft Entra ID
- [ ] Registrar a aplicação no Azure Portal (Microsoft Entra ID → App Registrations)
- [ ] Configurar **Redirect URIs** (ex: `https://localhost:7000/signin-oidc`)
- [ ] Criar **Client Secret** e salvar em configuração segura
- [ ] Definir escopos: `openid`, `profile`, `email`, `User.Read`
- [ ] Instalar pacotes NuGet:
  - `Microsoft.Identity.Web`
  - `Microsoft.Identity.Web.UI`
- [ ] Adicionar seção `AzureAd` no `appsettings.json`:
  ```json
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "<seu-dominio>.onmicrosoft.com",
    "TenantId": "<TENANT_ID>",
    "ClientId": "<CLIENT_ID>",
    "ClientSecret": "<CLIENT_SECRET>",
    "CallbackPath": "/signin-oidc"
  }
  ```
- [ ] Configurar middleware em `Program.cs`:
  ```csharp
  builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);
  app.UseAuthentication();
  app.UseAuthorization();
  ```

---

### auth-002 — Entidade User e Domínio de Permissões
- [ ] Criar `ApplicationUser` em `Prompteer.Domain/Entities/`:
  ```csharp
  public class ApplicationUser : BaseEntity
  {
      public string EntraObjectId { get; set; }   // sub claim do token
      public string DisplayName   { get; set; }
      public string Email         { get; set; }
      public UserRole Role        { get; set; }
      public bool IsActive        { get; set; }
  }
  ```
- [ ] Criar enum `UserRole` em `Prompteer.Domain/Enums/`:
  - `Admin` — acesso total, gerencia usuários
  - `Editor` — cria e edita seus próprios templates; lê os públicos
  - `Viewer` — somente leitura
- [ ] Criar interface `ICurrentUserService` em `Prompteer.Domain/Interfaces/`
- [ ] Adicionar `DbSet<ApplicationUser>` ao `AppDbContext`
- [ ] Adicionar configuração EF: tabela `application_users`, índice único em `EntraObjectId`

---

### auth-003 — Migration e Seed
- [ ] Gerar migration: `dotnet ef migrations add AddApplicationUsers`
- [ ] Criar `SeedAdminUserAsync` no `DatabaseSeeder`:
  - Lê `ADMIN_ENTRA_OID` e `ADMIN_EMAIL` das variáveis de ambiente
  - Faz upsert do usuário admin inicial
- [ ] Adicionar `ADMIN_ENTRA_OID` e `ADMIN_EMAIL` ao `docker-compose.yml` como variáveis de ambiente opcionais

---

### auth-004 — Serviço de Usuário Atual
- [ ] Implementar `CurrentUserService : ICurrentUserService` em `Prompteer.Infrastructure/Services/`:
  - Injeta `IHttpContextAccessor`
  - Lê claim `oid` (Object ID do Entra) e `name`
  - Retorna `ApplicationUser` resolvido do banco
- [ ] Registrar no DI em `ServiceCollectionExtensions`
- [ ] Usar em `AppDbContext.SaveChangesAsync` para preencher `CreatedBy` / `UpdatedBy` nas entidades (adicionar essas props a `BaseEntity`)

---

### auth-005 — Controle de Acesso por Role
- [ ] Aplicar `[Authorize]` em todos os controllers
- [ ] Criar policies em `Program.cs`:
  ```csharp
  options.AddPolicy("AdminOnly",  p => p.RequireRole("Admin"));
  options.AddPolicy("EditorOrAbove", p => p.RequireRole("Admin", "Editor"));
  ```
- [ ] Proteger com `[Authorize(Policy = "AdminOnly")]`:
  - Exclusão de registros de sistema (`IsSystemDefault = true`)
  - Gerenciamento de usuários
- [ ] Proteger com `[Authorize(Policy = "EditorOrAbove")]`:
  - Criação e edição de templates, perfis, tecnologias, etc.
- [ ] Viewers ficam no `[Authorize]` padrão (somente leitura)
- [ ] Ocultar botões de ação nas views conforme role do usuário logado (`@if (User.IsInRole("Admin"))`)

---

### auth-006 — UI de Autenticação
- [ ] Criar `AccountController` com `SignIn` / `SignOut` (delegando ao `MicrosoftIdentityWebApp`)
- [ ] Criar `Views/Account/AccessDenied.cshtml` (página 403 com estilo Tailwind)
- [ ] Atualizar `_Navbar.cshtml`:
  - Exibir avatar/iniciais + nome do usuário logado
  - Exibir role como badge colorido (Admin = vermelho, Editor = azul, Viewer = cinza)
  - Botão **Sair** com ícone
- [ ] Atualizar `_Layout.cshtml` com `@inject ICurrentUserService currentUser`
- [ ] Configurar `appsettings.json`: `"ErrorPath": "/Account/AccessDenied"`

---

### auth-007 — Gerenciamento de Usuários (Tela Admin)
- [ ] Criar `UsersController` com `[Authorize(Policy = "AdminOnly")]`
- [ ] Ações:
  - `Index` — lista paginada de usuários (nome, email, role, status, último acesso)
  - `Edit` — alterar role e status ativo/inativo
  - `Delete` (soft delete) — desativar acesso
- [ ] Criar views correspondentes com layout Tailwind consistente com o resto da app
- [ ] Adicionar link **Usuários** no sidebar (visível somente para Admin)
- [ ] Lógica de **sincronização no primeiro login** (upsert por `EntraObjectId`):
  - Implementar via `IClaimsTransformation` ou filtro global no controller base

---

### auth-008 — Isolamento de Dados por Usuário
- [ ] Adicionar `CreatedByUserId (Guid?)` e `IsPublic (bool)` a `PromptTemplate`
- [ ] Ajustar `PromptDraft` com `CreatedByUserId`
- [ ] Gerar migration: `AddUserIsolationToTemplates`
- [ ] Ajustar `IPromptTemplateService` e queries:
  - **Admin** vê todos
  - **Editor** vê os seus + os `IsPublic = true`
  - **Viewer** vê somente `IsPublic = true`
- [ ] Atualizar `TemplatesController` para aplicar filtro via `ICurrentUserService`
- [ ] Adicionar toggle **Público / Privado** na tela de save do template (Step 9)

---

## Acceptance Criteria

- [ ] Usuário não autenticado é redirecionado para login Microsoft ao acessar qualquer rota
- [ ] Login via conta Microsoft funciona e retorna à página original
- [ ] Role Admin consegue acessar `/Users` e alterar roles de outros usuários
- [ ] Role Editor consegue criar templates; não vê templates privados de outros
- [ ] Role Viewer não visualiza botões de criação/edição/exclusão
- [ ] Logout limpa a sessão e redireciona para home pública
- [ ] Build e docker compose sobem sem erros após implementação

---

## Notas Técnicas

- Utilizar **MSAL.NET** implicitamente via `Microsoft.Identity.Web` — não implementar MSAL manualmente
- `EntraObjectId` = claim `oid` (Object ID, estável e único por tenant)
- Preferir **Role-based** authorization para controle simples; adicionar **Policy-based** apenas onde necessário
- Evitar armazenar tokens no banco — usar apenas o `oid` para identificar o usuário local
- Secrets em produção devem usar **Azure Key Vault** ou variáveis de ambiente — nunca em código ou appsettings commitados
- Para desenvolvimento local: usar `dotnet user-secrets` para `AzureAd:ClientSecret`
