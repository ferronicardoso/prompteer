---
id: task-001
title: Configuração da Solução e Estrutura do Projeto
status: Done
assignee: []
created_date: '2026-03-02 10:47'
updated_date: '2026-03-02 11:06'
labels:
  - setup
  - infra
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Criar solução .NET 10 com 4 projetos (Domain, Application, Infrastructure, Web). Configurar referências entre projetos, instalar pacotes NuGet (Npgsql EF Core, FluentValidation, AutoMapper). Configurar appsettings.json (connection string PostgreSQL). Configurar Program.cs. Configurar Tailwind CSS CLI (package.json + tailwind.config.js + input.css → wwwroot/css/app.css). Verificar build limpo.
<!-- SECTION:DESCRIPTION:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. dotnet new sln
2. Criar 4 projetos de classe/web
3. Adicionar referências entre camadas
4. Instalar NuGet packages
5. Configurar appsettings.json (connection string PostgreSQL)
6. Configurar Program.cs (DbContext, Session, AutoMapper, FluentValidation, Repos)
7. Verificar build limpo
<!-- SECTION:PLAN:END -->
