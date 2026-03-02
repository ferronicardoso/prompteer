using AutoMapper;
using Prompteer.Application.DTOs;
using Prompteer.Domain.Entities;
using Prompteer.Domain.Enums;

namespace Prompteer.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // AgentProfile
        CreateMap<AgentProfile, AgentProfileDto>()
            .ForMember(d => d.ToneDisplay, o => o.MapFrom(s => GetToneDisplay(s.Tone)));
        CreateMap<AgentProfileFormDto, AgentProfile>();

        // Technology
        CreateMap<Technology, TechnologyDto>()
            .ForMember(d => d.CategoryDisplay,  o => o.MapFrom(s => GetCategoryDisplay(s.Category)))
            .ForMember(d => d.EcosystemDisplay, o => o.MapFrom(s => GetEcosystemDisplay(s.Ecosystem)));
        CreateMap<Technology, TechnologySelectDto>()
            .ForMember(d => d.CategoryDisplay, o => o.MapFrom(s => GetCategoryDisplay(s.Category)));
        CreateMap<TechnologyFormDto, Technology>();

        // ArchitecturalPattern
        CreateMap<ArchitecturalPattern, ArchitecturalPatternDto>()
            .ForMember(d => d.EcosystemDisplay, o => o.MapFrom(s => GetEcosystemDisplay(s.Ecosystem)));
        CreateMap<ArchitecturalPatternFormDto, ArchitecturalPattern>();

        // BacklogTool
        CreateMap<BacklogTool, BacklogToolDto>();
        CreateMap<BacklogToolFormDto, BacklogTool>();

        // PromptTemplate
        CreateMap<PromptTemplate, PromptTemplateDto>();
        CreateMap<PromptTemplate, PromptTemplateSummaryDto>()
            .ForMember(d => d.TechnologyNames, o => o.Ignore());
        CreateMap<PromptTemplateVersion, PromptTemplateVersionDto>();
    }

    private static string GetToneDisplay(ToneType tone) => tone switch
    {
        ToneType.Technical => "Técnico",
        ToneType.Didactic  => "Didático",
        ToneType.Direct    => "Direto",
        ToneType.Detailed  => "Detalhista",
        _                  => tone.ToString()
    };

    private static string GetCategoryDisplay(TechCategory category) => category switch
    {
        TechCategory.Framework     => "Framework",
        TechCategory.Database      => "Banco de Dados",
        TechCategory.ORM           => "ORM",
        TechCategory.Frontend      => "Frontend",
        TechCategory.Auth          => "Autenticação",
        TechCategory.Messaging     => "Mensageria",
        TechCategory.Cache         => "Cache",
        TechCategory.Observability => "Observabilidade",
        TechCategory.DevOps        => "DevOps",
        TechCategory.Testing       => "Testes",
        TechCategory.Other         => "Outro",
        _                          => category.ToString()
    };

    private static string GetEcosystemDisplay(TechEcosystem eco) => eco switch
    {
        TechEcosystem.DotNet   => ".NET",
        TechEcosystem.Node     => "Node.js",
        TechEcosystem.Python   => "Python",
        TechEcosystem.Java     => "Java",
        TechEcosystem.Agnostic => "Agnóstico",
        _                      => eco.ToString()
    };
}
