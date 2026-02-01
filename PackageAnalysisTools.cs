using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using UmbracoPackageSuggest.Services;
using UmbracoPackageSuggest.Services.Analysis;
namespace UmbracoPackageSuggest.Tools;

[McpServerToolType]
public static class PackageAnalysisTools
{
    [McpServerTool, Description("Analyzes a .NET/Umbraco project using LLM-based intelligence to understand the entire codebase, architecture, and business needs, then suggests relevant NuGet and Umbraco marketplace packages with detailed reasoning. Use 'depth' parameter to control response detail level.")]
    public static async Task<string> SuggestPackages(
        McpServer thisServer,
        NuGetApiClient nuGetClient,
        UmbracoMarketplaceClient marketplaceClient,
        [Description("The path to the .NET/Umbraco project directory to analyze")] string projectPath,
        [Description("Response depth: 'simple' for basic suggestions, 'detailed' for comprehensive analysis (default: detailed)")] string depth = "detailed")
    {
        try
        {
            var analyzerWithLLM = new ProjectAnalyzer((McpServer?)thisServer);
            var engineWithLLM = new RecommendationEngine(nuGetClient, marketplaceClient, (McpServer?)thisServer);

            var analysis = await analyzerWithLLM.AnalyzeProjectAsync(projectPath);

            var recommendations = await engineWithLLM.GetRecommendationsAsync(analysis);

            var response = depth.ToLowerInvariant() == "simple"
                ? CreateSimpleResponse(recommendations, analysis)
                : CreateDetailedResponse(recommendations, analysis);

            return System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to analyze project: {ex.Message}\"}}";
        }
    }

    [McpServerTool, Description("Creates an AI-driven marketplace map visualization, ranking packages by relevance to your project, popularity, compatibility, and community score. Perfect for discovering both popular packages and hidden gems.")]
    public static async Task<string> GenerateMarketplaceMap(
        McpServer thisServer,
        NuGetApiClient nuGetClient,
        UmbracoMarketplaceClient marketplaceClient,
        ProjectAnalyzer projectAnalyzer,
        RecommendationEngine recommendationEngine,
        [Description("The path to the .NET/Umbraco project directory to analyze")] string projectPath)
    {
        try
        {
            var analysis = await projectAnalyzer.AnalyzeProjectAsync(projectPath);

            var recommendations = await recommendationEngine.GetRecommendationsAsync(analysis);

            var marketplaceMap = new
            {
                ProjectOverview = new
                {
                    analysis.Framework,
                    analysis.UmbracoVersion,
                    InstalledPackageCount = analysis.InstalledPackages.Count,
                    KeyFeatures = analysis.Features.Take(5),
                    ArchitectureStyle = analysis.ArchitecturePatterns.FirstOrDefault() ?? "Standard"
                },
                PackageClusters = new
                {
                    MainstreamWinners = recommendations
                        .Where(r => r.RelevanceScore > 0.7 && r.Downloads > 10000)
                        .OrderByDescending(r => r.RelevanceScore)
                        .Take(8)
                        .Select(r => new
                        {
                            r.PackageId,
                            r.PackageName,
                            RelevanceScore = Math.Round(r.RelevanceScore, 2),
                            PopularityScore = Math.Min(r.Downloads / 100000.0, 1.0),
                            r.IsHiddenGem,
                            r.PackagePersonality,
                            Category = "Mainstream Winner"
                        }),

                    HiddenGems = recommendations
                        .Where(r => r.IsHiddenGem && r.RelevanceScore > 0.6)
                        .OrderByDescending(r => r.RelevanceScore)
                        .Take(6)
                        .Select(r => new
                        {
                            r.PackageId,
                            r.PackageName,
                            RelevanceScore = Math.Round(r.RelevanceScore, 2),
                            PopularityScore = Math.Min(r.Downloads / 10000.0, 1.0),
                            r.IsHiddenGem,
                            r.PackagePersonality,
                            Category = "Hidden Gem"
                        }),

                    SpecializedTools = recommendations
                        .Where(r => r.Compatibility.Any() && r.RelevanceScore > 0.5)
                        .OrderByDescending(r => r.Compatibility.Count)
                        .Take(5)
                        .Select(r => new
                        {
                            r.PackageId,
                            r.PackageName,
                            RelevanceScore = Math.Round(r.RelevanceScore, 2),
                            CompatibilityVersions = r.Compatibility,
                            r.PackagePersonality,
                            Category = "Specialized Tool"
                        }),

                    CommunityFavorites = recommendations
                        .Where(r => r.CommunityScore > 0.7)
                        .OrderByDescending(r => r.CommunityScore)
                        .Take(5)
                        .Select(r => new
                        {
                            r.PackageId,
                            r.PackageName,
                            CommunityScore = Math.Round(r.CommunityScore, 2),
                            RelevanceScore = Math.Round(r.RelevanceScore, 2),
                            r.PackagePersonality,
                            Category = "Community Favorite"
                        })
                },
                DiscoveryInsights = new
                {
                    TotalPackagesAnalyzed = recommendations.Count,
                    HiddenGemsFound = recommendations.Count(r => r.IsHiddenGem),
                    HighCompatibilityPackages = recommendations.Count(r => r.Compatibility.Any()),
                    AverageRelevanceScore = Math.Round(recommendations.Average(r => r.RelevanceScore), 2),
                    TopCategories = recommendations
                        .SelectMany(r => r.Tags)
                        .GroupBy(t => t)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new { Category = g.Key, Count = g.Count() })
                },
                VisualizationHints = new
                {
                    Dimensions = new[] { "relevance", "popularity", "compatibility", "community" },
                    ColorScheme = new
                    {
                        MainstreamWinners = "#3B82F6", // Blue
                        HiddenGems = "#10B981",      // Green
                        SpecializedTools = "#F59E0B", // Yellow
                        CommunityFavorites = "#EF4444" // Red
                    },
                    SizeScaling = "downloads",
                    PositionAlgorithm = "force-directed with clustering"
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(marketplaceMap, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to generate marketplace map: {ex.Message}\"}}";
        }
    }

    [McpServerTool, Description("Performs sophisticated deep-dive analysis of your Umbraco project, providing enterprise-level insights on architecture, code quality, performance, and strategic recommendations. This is advanced project intelligence beyond basic suggestions.")]
    public static async Task<string> PerformDeepProjectAnalysis(
        ProjectAnalyzer projectAnalyzer,
        [Description("The path to the .NET/Umbraco project directory")] string projectPath)
    {
        try
        {
            var analysis = await projectAnalyzer.AnalyzeProjectAsync(projectPath);

            var deepInsights = new
            {
                AnalysisTimestamp = DateTime.UtcNow.ToString("O"),
                ProjectOverview = new
                {
                    Framework = analysis.Framework,
                    UmbracoVersion = analysis.UmbracoVersion,
                    InstalledPackagesCount = analysis.InstalledPackages.Count,
                    TotalLinesOfCode = analysis.CodeComplexityMetrics?.TotalLinesOfCode ?? 0
                },
                ArchitecturalAssessment = new
                {
                    ArchitectureStyle = analysis.LayeredArchitecture?.ArchitectureStyle ?? "Unknown",
                    LayerCompleteness = new
                    {
                        PresentationLayer = analysis.LayeredArchitecture?.HasPresentationLayer ?? false,
                        BusinessLayer = analysis.LayeredArchitecture?.HasBusinessLayer ?? false,
                        DataLayer = analysis.LayeredArchitecture?.HasDataLayer ?? false,
                        ServiceLayer = analysis.LayeredArchitecture?.HasServiceLayer ?? false
                    },
                    ComplexityMetrics = new
                    {
                        CouplingScore = analysis.ArchitecturalComplexity?.CouplingScore ?? 0,
                        CohesionScore = analysis.ArchitecturalComplexity?.CohesionScore ?? 0,
                        ComplexityHotspots = analysis.ArchitecturalComplexity?.ComplexityHotspots ?? new List<string>()
                    },
                    DetectedPatterns = analysis.DetectedDesignPatterns
                },
                CodeQualityAnalysis = new
                {
                    ComplexityMetrics = new
                    {
                        AverageMethodLength = Math.Round(analysis.CodeComplexityMetrics?.AverageMethodLength ?? 0, 1),
                        AverageClassLength = Math.Round(analysis.CodeComplexityMetrics?.AverageClassLength ?? 0, 1),
                        MostComplexFiles = analysis.CodeComplexityMetrics?.ComplexityByFile
                            .OrderByDescending(kvp => kvp.Value)
                            .Take(3)
                            .Select(kvp => new { File = kvp.Key, Lines = kvp.Value })
                            .ToList()
                    },
                    QualityAssessment = new
                    {
                        CodeSmells = analysis.CodingPatterns?.CodeSmells ?? new List<string>(),
                        BestPractices = analysis.CodingPatterns?.BestPractices ?? new List<string>(),
                        NamingConventions = analysis.CodingPatterns?.NamingConventions ?? new List<string>()
                    }
                },
                PerformanceProfile = new
                {
                    Bottlenecks = analysis.PerformanceBottlenecks,
                    CachingStrategy = new
                    {
                        Types = analysis.CachingPatterns?.CacheTypes ?? new List<string>(),
                        DistributedCache = analysis.CachingPatterns?.UsesDistributedCache ?? false,
                        OutputCache = analysis.CachingPatterns?.UsesOutputCache ?? false
                    },
                    AsyncUsage = new
                    {
                        MethodsCount = analysis.AsyncPatterns?.AsyncMethodsCount ?? 0,
                        Patterns = analysis.AsyncPatterns?.AsyncPatternsUsed ?? new List<string>(),
                        UsesConfigureAwait = analysis.AsyncPatterns?.UsesConfigureAwait ?? false
                    },
                    OptimizationOpportunities = analysis.PerformanceOptimizationOpportunities
                },
                BusinessLogicEvaluation = new
                {
                    DomainConcepts = analysis.BusinessDomainConcepts,
                    Complexity = new
                    {
                        BusinessRules = analysis.BusinessLogicComplexity?.BusinessRulesCount ?? 0,
                        ValidationRules = analysis.BusinessLogicComplexity?.ValidationRulesCount ?? 0,
                        ComplexMethods = analysis.BusinessLogicComplexity?.ComplexBusinessMethods ?? new List<string>()
                    }
                },
                StrategicInsights = new
                {
                    ArchitecturalSynthesis = analysis.ArchitecturalSynthesis ?? "Advanced architectural analysis available with LLM",
                    CodeQualityAssessment = analysis.CodeQualityAssessment ?? "Code quality assessment available with LLM",
                    BusinessLogicUnderstanding = analysis.BusinessLogicUnderstanding ?? "Business logic analysis available with LLM",
                    PackageRecommendations = analysis.PackageRecommendationSynthesis ?? "Strategic package recommendations available with LLM",
                    IntegrationComplexity = analysis.IntegrationComplexityAnalysis ?? "Integration complexity analysis available with LLM"
                },
                IntelligenceIndicators = new
                {
                    AnalysisDepth = "Enterprise",
                    InsightsGenerated = new[]
                    {
                        analysis.ArchitecturalSynthesis != null ? "Architectural Assessment" : null,
                        analysis.CodeQualityAssessment != null ? "Code Quality Analysis" : null,
                        analysis.BusinessLogicUnderstanding != null ? "Business Logic Understanding" : null,
                        analysis.PackageRecommendationSynthesis != null ? "Package Strategy" : null,
                        analysis.IntegrationComplexityAnalysis != null ? "Integration Planning" : null
                    }.Where(x => x != null).ToArray(),
                    DataPointsAnalyzed = new
                    {
                        FilesProcessed = analysis.CodeComplexityMetrics?.ComplexityByFile.Count ?? 0,
                        PatternsDetected = analysis.DetectedDesignPatterns.Count,
                        DependenciesMapped = analysis.InternalDependencies?.ClassDependencies.Count ?? 0,
                        BusinessRulesIdentified = analysis.BusinessLogicComplexity?.BusinessRulesCount ?? 0
                    }
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(deepInsights, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to perform deep project analysis: {ex.Message}\"}}";
        }
    }

    private static object CreateSimpleResponse(List<PackageRecommendation> recommendations, ProjectAnalysis analysis)
    {
        return new
        {
            ProjectInfo = new
            {
                Framework = analysis.Framework,
                UmbracoVersion = analysis.UmbracoVersion,
                ExistingPackages = analysis.InstalledPackages.Count
            },
            TopRecommendations = recommendations.Take(5).Select(r => new
            {
                Package = $"{r.PackageId} ({r.PackageName})",
                Description = r.Description,
                RelevanceScore = Math.Round(r.RelevanceScore, 2),
                WhyRecommended = r.Reason
            }),
            QuickActions = new[]
            {
                "Run with depth='detailed' for comprehensive analysis",
                "Use GetCodeIntegrationHints for specific package setup",
                "Try SimulatePackageImpact to see performance effects"
            }
        };
    }

    private static object CreateDetailedResponse(List<PackageRecommendation> recommendations, ProjectAnalysis analysis)
    {
        return new
        {
            DeepProjectAnalysis = new
            {
                ProjectOverview = new
                {
                    analysis.Framework,
                    analysis.UmbracoVersion,
                    TotalCodeFiles = analysis.CodeComplexityMetrics?.ComplexityByFile.Count ?? 0,
                    TotalLinesOfCode = analysis.CodeComplexityMetrics?.TotalLinesOfCode ?? 0,
                    DetectedFeatures = analysis.Features,
                    BusinessDomain = analysis.BusinessDomain,
                    ArchitecturePatterns = analysis.ArchitecturePatterns
                },

                ArchitecturalAssessment = new
                {
                    ArchitectureStyle = analysis.LayeredArchitecture?.ArchitectureStyle ?? "Unknown",
                    LayerCompleteness = new
                    {
                        HasPresentationLayer = analysis.LayeredArchitecture?.HasPresentationLayer ?? false,
                        HasBusinessLayer = analysis.LayeredArchitecture?.HasBusinessLayer ?? false,
                        HasDataLayer = analysis.LayeredArchitecture?.HasDataLayer ?? false,
                        HasServiceLayer = analysis.LayeredArchitecture?.HasServiceLayer ?? false
                    },
                    DesignPatterns = analysis.DetectedDesignPatterns,
                    ComplexityMetrics = new
                    {
                        CouplingScore = analysis.ArchitecturalComplexity?.CouplingScore ?? 0,
                        CohesionScore = analysis.ArchitecturalComplexity?.CohesionScore ?? 0,
                        ComplexityHotspots = analysis.ArchitecturalComplexity?.ComplexityHotspots ?? new List<string>()
                    }
                },

                CodeQualityAnalysis = new
                {
                    ComplexityMetrics = new
                    {
                        AverageMethodLength = Math.Round(analysis.CodeComplexityMetrics?.AverageMethodLength ?? 0, 1),
                        AverageClassLength = Math.Round(analysis.CodeComplexityMetrics?.AverageClassLength ?? 0, 1),
                        MostComplexFiles = analysis.CodeComplexityMetrics?.ComplexityByFile
                            .OrderByDescending(kvp => kvp.Value)
                            .Take(5)
                            .Select(kvp => new { File = kvp.Key, Lines = kvp.Value })
                            .ToList()
                    },
                    QualityAssessment = new
                    {
                        CodeSmells = analysis.CodingPatterns?.CodeSmells ?? new List<string>(),
                        BestPractices = analysis.CodingPatterns?.BestPractices ?? new List<string>(),
                        NamingConventions = analysis.CodingPatterns?.NamingConventions ?? new List<string>()
                    }
                },

                StrategicInsights = new
                {
                    ArchitecturalSynthesis = analysis.ArchitecturalSynthesis ?? "Advanced architectural analysis available with LLM",
                    CodeQualityAssessment = analysis.CodeQualityAssessment ?? "Code quality assessment available with LLM",
                    BusinessLogicUnderstanding = analysis.BusinessLogicUnderstanding ?? "Business logic analysis available with LLM",
                    PackageRecommendationSynthesis = analysis.PackageRecommendationSynthesis ?? "Strategic package recommendations available with LLM",
                    IntegrationComplexityAnalysis = analysis.IntegrationComplexityAnalysis ?? "Integration complexity analysis available with LLM"
                }
            },

            PackageRecommendations = recommendations.Take(10).Select(r => new
            {
                Package = new
                {
                    r.PackageId,
                    r.PackageName,
                    r.Description,
                    r.Source,
                    r.Version,
                    RelevanceScore = Math.Round(r.RelevanceScore, 3),
                    CommunityScore = Math.Round(r.CommunityScore, 3),
                    r.Downloads,
                    r.IsHiddenGem,
                    r.PackagePersonality
                },

                WhyRecommended = new
                {
                    r.Reason,
                    r.LLMReasoning,
                    r.BusinessValue,
                    CodeReductionPotential = r.CodeReductionPercentage > 0 ? $"{r.CodeReductionPercentage}% of custom code could be replaced" : null,
                    BusinessFit = $"Based on your {analysis.BusinessDomain.FirstOrDefault() ?? "business domain"} and {analysis.DetectedDesignPatterns.Count} design patterns detected"
                },

                IntegrationDetails = new
                {
                    r.IntegrationPoints,
                    r.ImplementationSteps,
                    IntegrationHints = new
                    {
                        QuickStart = $"💡 Add to {r.IntegrationPoints.FirstOrDefault() ?? "project"}",
                        Effort = r.ImpactedComponents.Count <= 2 ? "Quick" : r.ImpactedComponents.Count <= 4 ? "Medium" : "Advanced"
                    }
                },

                PerformanceImpact = r.PerformanceImpact != null ? new
                {
                    SeoBoost = Math.Round(r.PerformanceImpact.SeoBoost, 2),
                    SpeedImprovementPercent = Math.Round(r.PerformanceImpact.SpeedImprovement, 1),
                    EditorUsability = Math.Round(r.PerformanceImpact.EditorUsability, 2),
                    r.PerformanceImpact.PredictedBenefits
                } : null
            }),

            AnalysisSummary = new
            {
                AnalysisDepth = "Enterprise",
                ProjectComplexity = analysis.CodeComplexityMetrics?.TotalLinesOfCode > 10000 ? "Large Enterprise" :
                                   analysis.CodeComplexityMetrics?.TotalLinesOfCode > 5000 ? "Medium Business" : "Small Project",
                KeyFindings = new
                {
                    ArchitectureStyle = analysis.LayeredArchitecture?.ArchitectureStyle ?? "Unknown",
                    DesignPatternsCount = analysis.DetectedDesignPatterns.Count,
                    CodeQualityScore = analysis.CodingPatterns?.CodeSmells?.Count == 0 ? "Excellent" :
                                      (analysis.CodingPatterns?.CodeSmells?.Count ?? 0) < 3 ? "Good" : "Needs Improvement"
                },
                RecommendationsGenerated = recommendations.Count,
                AnalysisTimestamp = DateTime.UtcNow.ToString("O")
            }
        };
    }
}