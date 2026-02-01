using System.Text.Json;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Microsoft.Extensions.AI;

namespace UmbracoPackageSuggest.Services.Analysis;


public class RecommendationEngine
{
    private readonly NuGetApiClient _nuGetClient;
    private readonly UmbracoMarketplaceClient _marketplaceClient;
    private readonly McpServer? _mcpServer;
    private readonly bool _llmEnabled;

    public RecommendationEngine(NuGetApiClient nuGetClient, UmbracoMarketplaceClient marketplaceClient, McpServer? mcpServer = null)
    {
        _nuGetClient = nuGetClient;
        _marketplaceClient = marketplaceClient;
        _mcpServer = mcpServer;
        _llmEnabled = !string.Equals(
            Environment.GetEnvironmentVariable("DISABLE_LLM"), 
            "true", 
            StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<PackageRecommendation>> GetRecommendationsAsync(ProjectAnalysis analysis)
    {
        var recommendations = new List<PackageRecommendation>();

        var nuGetRecommendations = await GetNuGetRecommendationsAsync(analysis);
        recommendations.AddRange(nuGetRecommendations);

        var marketplaceRecommendations = await GetMarketplaceRecommendationsAsync(analysis);
        recommendations.AddRange(marketplaceRecommendations);

        if (_mcpServer != null && _llmEnabled && recommendations.Any())
        {
            await ScoreWithLLMAsync(recommendations, analysis);
        }
        else
        {
            foreach (var rec in recommendations)
            {
                if (rec.Source == "NuGet")
                {
                    var package = nuGetRecommendations.FirstOrDefault(r => r.PackageId == rec.PackageId);
                    if (package != null) rec.RelevanceScore = package.RelevanceScore;
                }
                else
                {
                    var package = marketplaceRecommendations.FirstOrDefault(r => r.PackageId == rec.PackageId);
                    if (package != null) rec.RelevanceScore = package.RelevanceScore;
                }
            }
        }

        // Identify hidden gems (high relevance, low downloads)
        IdentifyHiddenGems(recommendations);

        // Calculate community scores
        await CalculateCommunityScoresAsync(recommendations);

        // Rank and sort by relevance score
        return recommendations
            .OrderByDescending(r => r.RelevanceScore)
            .ThenByDescending(r => r.CommunityScore)
            .ThenByDescending(r => r.Downloads)
            .ToList();
    }

    private async Task ScoreWithLLMAsync(List<PackageRecommendation> recommendations, ProjectAnalysis analysis)
    {
        if (_mcpServer == null || !recommendations.Any()) return;

        // Prepare context for LLM
        var projectContext = $@"
Project Framework: {analysis.Framework}
Umbraco Version: {analysis.UmbracoVersion}
Installed Packages: {string.Join(", ", analysis.InstalledPackages)}
Detected Features: {string.Join(", ", analysis.Features)}
Architecture Patterns: {string.Join(", ", analysis.ArchitecturePatterns)}
Business Domain: {string.Join(", ", analysis.BusinessDomain)}
Code Patterns: {string.Join(", ", analysis.CodePatterns)}
LLM Analysis: {analysis.LLMAnalysis}
";

        // Process recommendations in batches to avoid token limits
        var batchSize = 10;
        for (int i = 0; i < recommendations.Count; i += batchSize)
        {
            var batch = recommendations.Skip(i).Take(batchSize).ToList();
            
            var packagesInfo = string.Join("\n", batch.Select((r, idx) => 
                $"{idx + 1}. {r.PackageName} ({r.PackageId})\n   Description: {r.Description}\n   Tags: {string.Join(", ", r.Tags)}\n   Downloads: {r.Downloads}\n   Source: {r.Source}"));

            var prompt = $@"You are an expert Umbraco .NET developer with a creative personality. Analyze these packages for this Umbraco project and provide detailed, human-like insights.

Project Context:
{projectContext}

Packages to Evaluate:
{packagesInfo}

For each package, provide creative and practical analysis:
1. Relevance score (0.0-1.0) based on how well it matches the project's needs
2. Detailed reasoning explaining why this package fits or doesn't fit
3. Specific use cases where this package would help
4. Package personality: Describe this package in human terms with a creative analogy (e.g., ""This package is like a Swiss Army knife for forms"" or ""It's the friendly neighborhood helper that organizes your content chaos"")
5. Integration points: Suggest 2-3 specific places in typical Umbraco codebases where this would integrate (e.g., ""Controllers"", ""ViewComponents"", ""Custom property editors"")
6. Impacted components: List what would be enhanced (e.g., ""Document Types"", ""Content editing workflow"", ""API endpoints"")
7. Performance prediction: Estimate SEO boost (0-1), speed improvement (percentage), and editor usability (0-1) if this package were added

Return JSON array with format:
[
  {{
    ""packageId"": ""package-id"",
    ""relevanceScore"": 0.85,
    ""reasoning"": ""Detailed explanation..."",
    ""useCases"": [""use case 1"", ""use case 2""],
    ""packagePersonality"": ""Creative human-like description with analogy"",
    ""integrationPoints"": [""Controllers"", ""ViewComponents"", ""Property Editors""],
    ""impactedComponents"": [""Document Types"", ""Content Workflow"", ""API Endpoints""],
    ""performanceImpact"": {{
      ""seoBoost"": 0.3,
      ""speedImprovement"": 15.5,
      ""editorUsability"": 0.7,
      ""predictedBenefits"": [""Faster content loading"", ""Better SEO scores""]
    }}
  }}
]";

            try
            {
                var messages = new[]
                {
                    new ChatMessage(ChatRole.User, prompt)
                };

                var options = new ChatOptions
                {
                    MaxOutputTokens = 3000,
                    Temperature = 0.2f
                };

                var llmClient = _mcpServer.AsSamplingChatClient();
                var response = await llmClient.GetResponseAsync(messages, options, CancellationToken.None);

                // Extract text from ChatResponse
                var responseText = response.Text ?? response.ToString() ?? string.Empty;

                // Parse LLM response
                var jsonStart = responseText.IndexOf('[');
                var jsonEnd = responseText.LastIndexOf(']');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var scores = JsonSerializer.Deserialize<JsonElement>(json);

                    foreach (var scoreItem in scores.EnumerateArray())
                    {
                        if (scoreItem.TryGetProperty("packageId", out var pkgId))
                        {
                            var packageId = pkgId.GetString();
                            var recommendation = batch.FirstOrDefault(r => r.PackageId == packageId);
                            
                            if (recommendation != null)
                            {
                                if (scoreItem.TryGetProperty("relevanceScore", out var score))
                                {
                                    recommendation.RelevanceScore = score.GetDouble();
                                }

                                if (scoreItem.TryGetProperty("reasoning", out var reasoning))
                                {
                                    recommendation.LLMReasoning = reasoning.GetString();
                                    recommendation.Reason = reasoning.GetString();
                                }

                                if (scoreItem.TryGetProperty("useCases", out var useCases))
                                {
                                    recommendation.UseCases = useCases.EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();
                                }

                                if (scoreItem.TryGetProperty("packagePersonality", out var personality))
                                {
                                    recommendation.PackagePersonality = personality.GetString();
                                }

                                if (scoreItem.TryGetProperty("integrationPoints", out var integrationPoints))
                                {
                                    recommendation.IntegrationPoints = integrationPoints.EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();
                                }

                                if (scoreItem.TryGetProperty("impactedComponents", out var impactedComponents))
                                {
                                    recommendation.ImpactedComponents = impactedComponents.EnumerateArray()
                                        .Select(x => x.GetString() ?? "")
                                        .Where(x => !string.IsNullOrEmpty(x))
                                        .ToList();
                                }

                                if (scoreItem.TryGetProperty("performanceImpact", out var performanceImpact))
                                {
                                    var prediction = new PerformancePrediction();
                                    if (performanceImpact.TryGetProperty("seoBoost", out var seoBoost))
                                        prediction.SeoBoost = seoBoost.GetDouble();
                                    if (performanceImpact.TryGetProperty("speedImprovement", out var speedImprovement))
                                        prediction.SpeedImprovement = speedImprovement.GetDouble();
                                    if (performanceImpact.TryGetProperty("editorUsability", out var editorUsability))
                                        prediction.EditorUsability = editorUsability.GetDouble();
                                    if (performanceImpact.TryGetProperty("predictedBenefits", out var predictedBenefits))
                                    {
                                        prediction.PredictedBenefits = predictedBenefits.EnumerateArray()
                                            .Select(x => x.GetString() ?? "")
                                            .Where(x => !string.IsNullOrEmpty(x))
                                            .ToList();
                                    }
                                    recommendation.PerformanceImpact = prediction;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If LLM scoring fails, use rule-based scores
                // Scores are already set from rule-based methods
            }
        }
    }

    private async Task<List<PackageRecommendation>> GetNuGetRecommendationsAsync(ProjectAnalysis analysis)
    {
        var recommendations = new List<PackageRecommendation>();

        // Search for Umbraco-related packages
        var umbracoQuery = analysis.UmbracoVersion != null
            ? $"umbraco {analysis.UmbracoVersion}"
            : "umbraco";

        var umbracoPackages = await _nuGetClient.SearchPackagesAsync(umbracoQuery, take: 30);
        
        foreach (var package in umbracoPackages)
        {
            if (analysis.InstalledPackages.Contains(package.Id, StringComparer.OrdinalIgnoreCase))
                continue;

            var score = CalculateNuGetRelevanceScore(package, analysis);
            if (score > 0.1) // Only include packages with meaningful relevance
            {
                recommendations.Add(new PackageRecommendation
                {
                    PackageId = package.Id,
                    PackageName = package.Id,
                    Description = package.Description,
                    Source = "NuGet",
                    Version = package.Version,
                    RelevanceScore = score,
                    Downloads = package.TotalDownloads,
                    Tags = package.Tags,
                    Reason = GenerateReason(package, analysis),
                    ImplementationSteps = await GenerateImplementationStepsAsync(package.Id, analysis)
                });
            }
        }

        // Search based on detected features
        foreach (var feature in analysis.Features)
        {
            var featurePackages = await _nuGetClient.SearchPackagesAsync($"umbraco {feature}", take: 10);
            foreach (var package in featurePackages)
            {
                if (analysis.InstalledPackages.Contains(package.Id, StringComparer.OrdinalIgnoreCase))
                    continue;

                var existing = recommendations.FirstOrDefault(r => r.PackageId == package.Id);
                if (existing != null)
                {
                    existing.RelevanceScore += 0.2; // Boost for feature match
                }
                else
                {
                    var score = CalculateNuGetRelevanceScore(package, analysis) + 0.2;
                    recommendations.Add(new PackageRecommendation
                    {
                        PackageId = package.Id,
                        PackageName = package.Id,
                        Description = package.Description,
                        Source = "NuGet",
                        Version = package.Version,
                        RelevanceScore = score,
                        Downloads = package.TotalDownloads,
                        Tags = package.Tags,
                        Reason = $"Relevant for {feature} feature",
                        ImplementationSteps = await GenerateImplementationStepsAsync(package.Id, analysis)
                    });
                }
            }
        }

        return recommendations;
    }

    private async Task<List<PackageRecommendation>> GetMarketplaceRecommendationsAsync(ProjectAnalysis analysis)
    {
        var recommendations = new List<PackageRecommendation>();

        // Get popular packages
        var popularPackages = await _marketplaceClient.GetPackagesAsync(orderBy: "MostDownloads", pageSize: 50);
        
        foreach (var package in popularPackages)
        {
            if (analysis.InstalledPackages.Contains(package.Id, StringComparer.OrdinalIgnoreCase))
                continue;

            var score = CalculateMarketplaceRelevanceScore(package, analysis);
            if (score > 0.1)
            {
                recommendations.Add(new PackageRecommendation
                {
                    PackageId = package.Id,
                    PackageName = package.Name,
                    Description = package.Description,
                    Source = "UmbracoMarketplace",
                    Version = package.Version,
                    RelevanceScore = score,
                    Downloads = package.Downloads,
                    Tags = package.Tags,
                    Compatibility = package.Compatibility,
                    PackageUrl = package.PackageUrl,
                    Reason = GenerateReason(package, analysis),
                    ImplementationSteps = await GenerateImplementationStepsAsync(package.Id, analysis)
                });
            }
        }

        // Search based on features
        foreach (var feature in analysis.Features)
        {
            var featurePackages = await _marketplaceClient.SearchPackagesAsync(feature, pageSize: 20);
            foreach (var package in featurePackages)
            {
                if (analysis.InstalledPackages.Contains(package.Id, StringComparer.OrdinalIgnoreCase))
                    continue;

                var existing = recommendations.FirstOrDefault(r => r.PackageId == package.Id);
                if (existing != null)
                {
                    existing.RelevanceScore += 0.3;
                }
                else
                {
                    var score = CalculateMarketplaceRelevanceScore(package, analysis) + 0.3;
                    recommendations.Add(new PackageRecommendation
                    {
                        PackageId = package.Id,
                        PackageName = package.Name,
                        Description = package.Description,
                        Source = "UmbracoMarketplace",
                        Version = package.Version,
                        RelevanceScore = score,
                        Downloads = package.Downloads,
                        Tags = package.Tags,
                        Compatibility = package.Compatibility,
                        PackageUrl = package.PackageUrl,
                        Reason = $"Relevant for {feature} feature",
                        ImplementationSteps = await GenerateImplementationStepsAsync(package.Id, analysis)
                    });
                }
            }
        }

        return recommendations;
    }

    private double CalculateNuGetRelevanceScore(NuGetPackageInfo package, ProjectAnalysis analysis)
    {
        double score = 0.0;

        // Base score from downloads (normalized)
        score += Math.Min(package.TotalDownloads / 1000000.0, 1.0) * 0.3;

        // Tag matching
        var packageTags = string.Join(" ", package.Tags).ToLowerInvariant();
        var description = (package.Description ?? "").ToLowerInvariant();
        var combinedText = $"{packageTags} {description}";

        // Umbraco version compatibility
        if (analysis.UmbracoVersion != null && combinedText.Contains(analysis.UmbracoVersion.ToLowerInvariant()))
        {
            score += 0.4;
        }

        // Feature matching
        foreach (var feature in analysis.Features)
        {
            if (combinedText.Contains(feature.ToLowerInvariant()))
            {
                score += 0.1;
            }
        }

        // Popular Umbraco packages boost
        var popularUmbracoPackages = new[] { "umbraco", "our", "skybrud", "ucommerce", "articulate" };
        if (popularUmbracoPackages.Any(p => package.Id.ToLowerInvariant().Contains(p)))
        {
            score += 0.2;
        }

        return Math.Min(score, 1.0);
    }

    private double CalculateMarketplaceRelevanceScore(UmbracoPackage package, ProjectAnalysis analysis)
    {
        double score = 0.0;

        // Base score from downloads (normalized)
        score += Math.Min(package.Downloads / 10000.0, 1.0) * 0.3;

        // Compatibility check
        if (analysis.UmbracoVersion != null && package.Compatibility.Any())
        {
            var versionMatch = package.Compatibility.Any(c => 
                c.Contains(analysis.UmbracoVersion, StringComparison.OrdinalIgnoreCase));
            if (versionMatch)
            {
                score += 0.4;
            }
        }

        // Tag and description matching
        var packageText = $"{string.Join(" ", package.Tags)} {package.Description}".ToLowerInvariant();
        
        // Feature matching
        foreach (var feature in analysis.Features)
        {
            if (packageText.Contains(feature.ToLowerInvariant()))
            {
                score += 0.15;
            }
        }

        return Math.Min(score, 1.0);
    }

    private string GenerateReason(NuGetPackageInfo package, ProjectAnalysis analysis)
    {
        var reasons = new List<string>();
        
        if (package.TotalDownloads > 100000)
        {
            reasons.Add("highly popular");
        }

        if (analysis.UmbracoVersion != null)
        {
            var versionMatch = (package.Description ?? "").ToLowerInvariant().Contains(
                analysis.UmbracoVersion.ToLowerInvariant());
            if (versionMatch)
            {
                reasons.Add($"compatible with Umbraco {analysis.UmbracoVersion}");
            }
        }

        var matchingFeatures = analysis.Features.Where(f => 
            (package.Description ?? "").ToLowerInvariant().Contains(f.ToLowerInvariant())).ToList();
        if (matchingFeatures.Any())
        {
            reasons.Add($"relevant for: {string.Join(", ", matchingFeatures)}");
        }

        return reasons.Any() ? string.Join("; ", reasons) : "general Umbraco package";
    }

    private string GenerateReason(UmbracoPackage package, ProjectAnalysis analysis)
    {
        var reasons = new List<string>();
        
        if (package.Downloads > 1000)
        {
            reasons.Add("popular marketplace package");
        }

        if (analysis.UmbracoVersion != null && package.Compatibility.Any(c => 
            c.Contains(analysis.UmbracoVersion, StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add($"compatible with Umbraco {analysis.UmbracoVersion}");
        }

        var matchingFeatures = analysis.Features.Where(f => 
            (package.Description ?? "").ToLowerInvariant().Contains(f.ToLowerInvariant())).ToList();
        if (matchingFeatures.Any())
        {
            reasons.Add($"relevant for: {string.Join(", ", matchingFeatures)}");
        }

        return reasons.Any() ? string.Join("; ", reasons) : "Umbraco marketplace package";
    }

    private void IdentifyHiddenGems(List<PackageRecommendation> recommendations)
    {
        if (!recommendations.Any()) return;

        // Calculate average downloads and relevance
        var avgDownloads = recommendations.Average(r => r.Downloads);
        var avgRelevance = recommendations.Average(r => r.RelevanceScore);

        foreach (var rec in recommendations)
        {
            // Hidden gem criteria:
            // - Relevance score above average
            // - Downloads below average (not overly popular)
            // - But still has some community adoption (not zero downloads)
            rec.IsHiddenGem = rec.RelevanceScore > avgRelevance * 0.8 &&
                             rec.Downloads < avgDownloads * 0.7 &&
                             rec.Downloads > 100; // Some minimum adoption
        }
    }

    private async Task CalculateCommunityScoresAsync(List<PackageRecommendation> recommendations)
    {
        // For now, calculate a basic community score based on available data
        // In a real implementation, this would query GitHub APIs, etc.
        foreach (var rec in recommendations)
        {
            double communityScore = 0.0;

            // Base score from downloads (normalized)
            communityScore += Math.Min(rec.Downloads / 100000.0, 1.0) * 0.4;

            // Relevance bonus (packages that are actually useful get higher scores)
            communityScore += rec.RelevanceScore * 0.3;

            // Hidden gem bonus (discovering good but underrated packages)
            if (rec.IsHiddenGem)
            {
                communityScore += 0.3;
            }

            // Compatibility bonus
            if (rec.Compatibility.Any())
            {
                communityScore += 0.1;
            }

            rec.CommunityScore = Math.Min(communityScore, 1.0);
        }
    }

    private async Task<List<string>> GenerateImplementationStepsAsync(string packageId, ProjectAnalysis analysis)
    {
        var steps = new List<string>();

        // Always start with installation
        steps.Add("Install package via NuGet Package Manager or CLI");

        try
        {
            // Try to get actual documentation from package sources
            var documentationSteps = await GetPackageDocumentationStepsAsync(packageId, analysis);

            if (documentationSteps.Any())
            {
                steps.AddRange(documentationSteps);
            }
            else
            {
                // Fallback to minimal guidance if no documentation found
                steps.Add("Refer to package documentation/README for detailed setup instructions");
                steps.Add("Check package repository for examples and configuration guides");
            }
        }
        catch
        {
            // If documentation fetch fails, provide minimal guidance
            steps.Add("Refer to package documentation/README for detailed setup instructions");
        }

        // Add framework and version compatibility checks
        if (analysis.Framework?.Contains("net") == true)
        {
            steps.Add($"Ensure compatibility with your .NET {analysis.Framework} project");
        }

        if (!string.IsNullOrEmpty(analysis.UmbracoVersion))
        {
            steps.Add($"Verify compatibility with Umbraco {analysis.UmbracoVersion}");
        }

        return steps;
    }

    private async Task<List<string>> GetPackageDocumentationStepsAsync(string packageId, ProjectAnalysis analysis)
    {
        var steps = new List<string>();

        try
        {
            // For now, provide intelligent documentation links based on package analysis
            // In a production system, this would make API calls to fetch actual README content

            var packageName = packageId.ToLowerInvariant();

            // Check if this is a well-known package with documentation
            if (packageName.Contains("umbraco.forms"))
            {
                steps.Add("📚 Official Umbraco Forms Documentation: https://docs.umbraco.com/umbraco-forms/");
                steps.Add("🎯 Quick Setup: Install package → Configure form types → Add to content");
            }
            else if (packageName.Contains("seo") || packageName.Contains("seotoolkit"))
            {
                steps.Add("📖 SEO Toolkit GitHub: https://github.com/patrickdemooij9/SeoToolkit.Umbraco");
                steps.Add("⚙️ Setup: Install → Configure SEO settings → Add properties to doctypes");
            }
            else if (packageName.Contains("deploy") || packageName.Contains("umbraco.deploy"))
            {
                steps.Add("📚 Umbraco Deploy Docs: https://docs.umbraco.com/umbraco-deploy/");
                steps.Add("🚀 Setup: Configure connection strings → Setup environments → Deploy");
            }
            else if (packageName.Contains("contentment"))
            {
                steps.Add("📖 Contentment GitHub: https://github.com/leekelleher/umbraco-contentment");
                steps.Add("🛠️ Setup: Install → Add data sources → Configure property editors");
            }
            else if (packageName.Contains("heartcore"))
            {
                steps.Add("📚 Umbraco Heartcore: https://docs.umbraco.com/umbraco-heartcore/");
                steps.Add("🌐 Setup: Create Heartcore project → Configure API keys → Implement client");
            }
            else
            {
                // For unknown packages, provide generic guidance
                steps.Add("🔍 Search for package documentation on NuGet.org or GitHub");
                steps.Add("📖 Look for README.md or docs folder in package repository");
                steps.Add("❓ Check package issues/discussions for setup help");
            }

            // Add Umbraco-specific guidance
            if (packageName.Contains("umbraco") || packageName.Contains("cms"))
            {
                steps.Add("🏗️ Umbraco packages typically require composer registration");
                steps.Add("⚙️ Configure settings in Umbraco backoffice after installation");
            }

        }
        catch
        {
            // If documentation lookup fails, return empty list (will use fallback)
        }

        return steps;
    }

}
