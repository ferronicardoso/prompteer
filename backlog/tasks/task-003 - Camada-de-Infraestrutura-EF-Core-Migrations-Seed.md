---
id: task-003
title: 'Camada de Infraestrutura (EF Core, Migrations, Seed)'
status: Done
assignee: []
created_date: '2026-03-02 10:47'
updated_date: '2026-03-02 11:14'
labels:
  - infra
  - database
dependencies:
  - task-002
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implementar AppDbContext com configurações EF Core (Fluent API, query filters para soft delete, override SaveChangesAsync para auditoria). Criar IEntityTypeConfiguration para cada entidade. Implementar GenericRepository<T> e UnitOfWork. Criar DatabaseSeeder com 10 perfis de agente, ~25 tecnologias, ~10 padrões arquiteturais e 6 ferramentas de backlog. Rodar migration inicial.
<!-- SECTION:DESCRIPTION:END -->
