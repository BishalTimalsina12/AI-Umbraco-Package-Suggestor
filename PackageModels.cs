namespace UmbracoPackageSuggest.Services;

public class PackageRecommendation
{
    public string PackageId { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Source { get; set; } = string.Empty; // "NuGet" or "UmbracoMarketplace"
    public string? Version { get; set; }
    public double RelevanceScore { get; set; }
    public long Downloads { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Compatibility { get; set; } = new();
    public string? PackageUrl { get; set; }
    public string? Reason { get; set; }
    public string? LLMReasoning { get; set; }

    public double CommunityScore { get; set; } // 0-1 based on GitHub stars, issues, updates
    public bool IsHiddenGem { get; set; } // High relevance, low downloads
    public string? PackagePersonality { get; set; } // Fun human-like description
    public List<string> IntegrationPoints { get; set; } = new(); // Where in codebase to integrate
    public List<string> ImpactedComponents { get; set; } = new(); // What gets enhanced
    public List<string> ImplementationSteps { get; set; } = new(); // Step-by-step integration guide
    public PerformancePrediction? PerformanceImpact { get; set; }
    public List<string> UseCases { get; set; } = new();
    public int CodeReductionPercentage { get; set; } // Estimated % of custom code this replaces
    public string? BusinessValue { get; set; } // Business justification for the recommendation
    public List<string> Evidence { get; set; } = new(); // Concrete evidence from the codebase supporting this recommendation
}

public class PerformancePrediction
{
    public double SeoBoost { get; set; } // 0-1
    public double SpeedImprovement { get; set; } // percentage
    public double EditorUsability { get; set; } // 0-1
    public List<string> PredictedBenefits { get; set; } = new();
}