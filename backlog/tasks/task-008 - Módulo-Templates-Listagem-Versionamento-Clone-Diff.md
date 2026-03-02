---
id: task-008
title: 'Módulo: Templates (Listagem, Versionamento, Clone, Diff)'
status: Done
assignee: []
created_date: '2026-03-02 10:48'
updated_date: '2026-03-02 12:01'
labels:
  - templates
  - ui
dependencies:
  - task-007
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Implementar TemplatesController: listagem com paginação, busca e filtro por data. Abrir template no wizard (carrega WizardSessionData do JSON da versão). Clonar template (nova entrada com versão 1). Excluir (soft delete). Histórico de versões: listar v1..vN com data. Diff visual simples entre duas versões (highlight de linhas adicionadas/removidas no markdown).
<!-- SECTION:DESCRIPTION:END -->

## Known Bugs

### [BUG] Import does not restore wizard fields

**Reported:** 2026-03-02  
**Status:** Pending fix

When a template is exported and then re-imported, the generated prompt text is restored correctly but the wizard form fields (agent profile, technologies, architectural patterns, modules, rules, etc.) are **not populated** when the template is opened in the wizard for editing.

**Root cause (suspected):** The `WizardDataJson` stored in `PromptTemplateVersion` contains `AgentProfileId`, `TechnologyIds`, and `ArchitecturalPatternIds` as GUIDs from the original instance. After import, these GUIDs may not match the IDs assigned to the resolved entities in the new instance, so the wizard cannot bind them to the select fields.

**Expected fix:** During import, after resolving/creating technologies and patterns, update the `WizardDataJson` with the new GUIDs before saving — so the wizard can correctly pre-populate all fields when re-opening the imported template.
