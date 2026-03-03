using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Prompteer.Application.DTOs;
using Prompteer.Application.Services;

namespace Prompteer.Web.Controllers;

[Route("api/ai")]
public class AIController(IAIService aiService, IStringLocalizer<SharedResource> l) : Controller
{
    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate([FromBody] AIGenerateRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FieldType))
            return BadRequest(new AIGenerateResponseDto { Success = false, Error = l["AI.FieldTypeRequired"].Value });

        try
        {
            var text = await aiService.GenerateFieldAsync(request.FieldType, request.Context);
            return Ok(new AIGenerateResponseDto { Success = true, Text = text });
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new AIGenerateResponseDto { Success = false, Error = ex.Message });
        }
        catch (Exception ex)
        {
            return Ok(new AIGenerateResponseDto { Success = false, Error = string.Format(l["AI.GenerateError"].Value, ex.Message) });
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var configured = await aiService.IsConfiguredAsync();
        return Ok(new { configured });
    }
}
