---
id: task-002
title: Camada de Domínio
status: Done
assignee: []
created_date: '2026-03-02 10:47'
updated_date: '2026-03-02 11:07'
labels:
  - domain
dependencies:
  - task-001
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implementar todas as entidades, enums e interfaces do domínio. Entidades: BaseEntity (abstrata), AgentProfile, Technology, ArchitecturalPattern, BacklogTool, PromptTemplate, PromptTemplateVersion, PromptVersionTechnology, PromptVersionPattern, PromptModule, PromptModuleItem, PromptDraft (para persistência de rascunhos do wizard: WizardDataJson, CurrentStep, Name?). Enums: ToneType, TechCategory, TechEcosystem. Interfaces: IRepository<T>, IUnitOfWork.
<!-- SECTION:DESCRIPTION:END -->
