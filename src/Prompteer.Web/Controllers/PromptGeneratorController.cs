using Microsoft.AspNetCore.Mvc;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;
using Prompteer.Application.Wizard;
using Prompteer.Web.Models;
using System.Text.Json;

namespace Prompteer.Web.Controllers;

public class PromptGeneratorController : Controller
{
    private readonly IPromptDraftService _draftService;
    private readonly IPromptBuilderService _builderService;
    private readonly IAgentProfileService _agentProfileService;
    private readonly ITechnologyService _technologyService;
    private readonly IArchitecturalPatternService _architecturalPatternService;
    private readonly IBacklogToolService _backlogToolService;
    private readonly IPromptTemplateService _templateService;

    public PromptGeneratorController(
        IPromptDraftService draftService,
        IPromptBuilderService builderService,
        IAgentProfileService agentProfileService,
        ITechnologyService technologyService,
        IArchitecturalPatternService architecturalPatternService,
        IBacklogToolService backlogToolService,
        IPromptTemplateService templateService)
    {
        _draftService = draftService;
        _builderService = builderService;
        _agentProfileService = agentProfileService;
        _technologyService = technologyService;
        _architecturalPatternService = architecturalPatternService;
        _backlogToolService = backlogToolService;
        _templateService = templateService;
    }

    // GET /PromptGenerator
    public async Task<IActionResult> Index(Guid? draftId, Guid? templateId)
    {
        if (templateId.HasValue && !draftId.HasValue)
        {
            // Carrega os dados do wizard da versão mais recente do template
            var wizardData = await _templateService.GetLatestWizardDataAsync(templateId.Value);

            var (newId, _) = await _draftService.GetOrCreateAsync(null);

            if (wizardData != null)
            {
                wizardData.EditingTemplateId = templateId.Value;
                await _draftService.UpdateAsync(newId, wizardData, 1);
            }

            return RedirectToAction(nameof(Step), new { step = 1, draftId = newId });
        }
        var (id, _) = await _draftService.GetOrCreateAsync(draftId);
        return RedirectToAction(nameof(Step), new { step = 1, draftId = id });
    }

    // GET /PromptGenerator/Step?step=N&draftId=...
    [HttpGet]
    public async Task<IActionResult> Step(int step, Guid draftId)
    {
        if (step < 1 || step > 9)
            return RedirectToAction(nameof(Index));

        var data = await _draftService.GetDataAsync(draftId);
        var vm = new WizardViewModel
        {
            DraftId = draftId,
            CurrentStep = step,
            Data = data
        };

        switch (step)
        {
            case 1:
                vm.AgentProfiles = await _agentProfileService.GetAllAsync();
                break;
            case 2:
                vm.BacklogTools = await _backlogToolService.GetAllAsync();
                break;
            case 3:
                vm.Technologies = await _technologyService.GetAllForSelectAsync();
                break;
            case 4:
                vm.ArchitecturalPatterns = await _architecturalPatternService.GetAllAsync();
                break;
            case 9:
                vm.GeneratedPrompt = await _builderService.BuildAsync(data);
                break;
        }

        return View($"Step{step}", vm);
    }

    // POST /PromptGenerator/Step — save current step and advance
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Step(int step, Guid draftId, IFormCollection form)
    {
        var existing = await _draftService.GetDataAsync(draftId);
        ApplyStepData(step, form, existing);
        await _draftService.UpdateAsync(draftId, existing, step);

        if (step == 8)
            return RedirectToAction(nameof(Preview), new { draftId });

        return RedirectToAction(nameof(Step), new { step = step + 1, draftId });
    }

    // POST /PromptGenerator/Previous — save current step and go back
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Previous(int step, Guid draftId, IFormCollection form)
    {
        var existing = await _draftService.GetDataAsync(draftId);
        ApplyStepData(step, form, existing);
        await _draftService.UpdateAsync(draftId, existing, step);

        return RedirectToAction(nameof(Step), new { step = step - 1, draftId });
    }

    // GET /PromptGenerator/Preview — generate prompt and show step 9
    [HttpGet]
    public async Task<IActionResult> Preview(Guid draftId)
    {
        var data = await _draftService.GetDataAsync(draftId);
        var prompt = await _builderService.BuildAsync(data);
        await _draftService.UpdateAsync(draftId, data, 9);

        var vm = new WizardViewModel
        {
            DraftId = draftId,
            CurrentStep = 9,
            Data = data,
            GeneratedPrompt = prompt
        };

        return View("Step9", vm);
    }

    // POST /PromptGenerator/SaveTemplate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTemplate(Guid draftId, string templateName, string? templateDesc, string generatedPrompt)
    {
        var data = await _draftService.GetDataAsync(draftId);
        await _templateService.SaveFromWizardAsync(data.EditingTemplateId, templateName, templateDesc, data, generatedPrompt);
        TempData["Success"] = data.EditingTemplateId.HasValue
            ? "Template atualizado com sucesso!"
            : "Template salvo com sucesso!";
        return RedirectToAction("Index", "Templates");
    }

    // POST /PromptGenerator/CreateAgentProfile (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAgentProfile(AgentProfileFormDto dto)
    {
        var result = await _agentProfileService.CreateAsync(dto);
        return Json(new
        {
            id = result.Id,
            name = result.Name,
            role = result.Role,
            knowledgeDomain = result.KnowledgeDomain,
            tone = result.Tone,
            toneDisplay = result.ToneDisplay,
            defaultConstraints = result.DefaultConstraints
        });
    }

    // POST /PromptGenerator/CreateTechnology (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTechnology(TechnologyFormDto dto)
    {
        var result = await _technologyService.CreateAsync(dto);
        return Json(new
        {
            id = result.Id,
            name = result.Name,
            version = result.Version,
            categoryDisplay = result.CategoryDisplay,
            ecosystemDisplay = result.EcosystemDisplay,
            label = result.Version is null ? result.Name : $"{result.Name} {result.Version}"
        });
    }

    // POST /PromptGenerator/CreateArchitecturalPattern (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateArchitecturalPattern(ArchitecturalPatternFormDto dto)
    {
        var result = await _architecturalPatternService.CreateAsync(dto);
        return Json(new
        {
            id = result.Id,
            name = result.Name,
            description = result.Description,
            ecosystemDisplay = result.EcosystemDisplay
        });
    }

    // POST /PromptGenerator/CreateBacklogTool (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBacklogTool(BacklogToolFormDto dto)
    {
        var result = await _backlogToolService.CreateAsync(dto);
        return Json(new
        {
            id = result.Id,
            name = result.Name,
            defaultInstructions = result.DefaultInstructions
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ApplyStepData(int step, IFormCollection form, WizardSessionData data)
    {
        switch (step)
        {
            case 1:
                data.AgentProfileId = Guid.TryParse(form["AgentProfileId"], out var agId) ? agId : null;
                break;

            case 2:
                data.BacklogToolId = Guid.TryParse(form["BacklogToolId"], out var btId) ? btId : null;
                data.BacklogInstructions = form["BacklogInstructions"].FirstOrDefault();
                break;

            case 3:
                data.ProjectName = form["ProjectName"].FirstOrDefault();
                data.ProjectDescription = form["ProjectDescription"].FirstOrDefault();
                data.TechnologyIds = form["TechnologyIds"]
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(x => Guid.Parse(x!))
                    .ToList();
                break;

            case 4:
                data.ArchitecturalPatternIds = form["ArchitecturalPatternIds"]
                    .Where(x => Guid.TryParse(x, out _))
                    .Select(x => Guid.Parse(x!))
                    .ToList();
                data.RequiredPackages = form["RequiredPackages"]
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()!;
                data.CodeConventions = form["CodeConventions"].FirstOrDefault();
                break;

            case 5:
                data.IncludeEnvironment = form["IncludeEnvironment"].Contains("true");
                data.DeploymentTargets = form["DeploymentTargets"]
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()!;
                data.GitStrategy = form["GitStrategy"].FirstOrDefault();
                data.IncludeCICD = form["IncludeCICD"].Contains("true");
                data.CICDTool = form["CICDTool"].FirstOrDefault();
                break;

            case 6:
                data.IncludeTests = form["IncludeTests"].Contains("true");
                data.TestTypes = form["TestTypes"]
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()!;
                data.TestFramework = form["TestFramework"].FirstOrDefault();
                data.MinCoverage = int.TryParse(form["MinCoverage"], out var cov) ? cov : null;
                data.TestObservations = form["TestObservations"].FirstOrDefault();
                break;

            case 7:
                var modulesJson = form["ModulesJson"].FirstOrDefault();
                if (!string.IsNullOrEmpty(modulesJson))
                {
                    data.Modules = JsonSerializer.Deserialize<List<WizardModule>>(modulesJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                }
                break;

            case 8:
                data.RuleFlags = form["RuleFlags"]
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList()!;
                data.AdditionalRules = form["AdditionalRules"].FirstOrDefault();
                break;
        }
    }
}
