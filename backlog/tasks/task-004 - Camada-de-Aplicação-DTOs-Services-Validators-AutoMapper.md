---
id: task-004
title: 'Camada de Aplicação (DTOs, Services, Validators, AutoMapper)'
status: Done
assignee: []
created_date: '2026-03-02 10:47'
updated_date: '2026-03-02 11:20'
labels:
  - application
dependencies:
  - task-003
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Criar DTOs de entrada e saída para todas as entidades. Implementar FluentValidation validators. Criar AutoMapper profiles. Implementar interfaces e classes de serviço: IAgentProfileService, ITechnologyService, IArchitecturalPatternService, IBacklogToolService, IPromptTemplateService, IPromptBuilderService, IPromptDraftService. Criar WizardSessionData POCO (carregado do PromptDraft.WizardDataJson). Gerenciamento de estado do wizard via banco (PromptDraft) em vez de ISession.
<!-- SECTION:DESCRIPTION:END -->
