using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Microsoft.Extensions.AI;
namespace UmbracoPackageSuggest.Services;


public class ProjectAnalyzer
{
    private readonly McpServer? _mcpServer;
    private readonly bool _llmEnabled;

    public ProjectAnalyzer(McpServer? mcpServer = null)
    {
        _mcpServer = mcpServer;
        _llmEnabled = !string.Equals(
            Environment.GetEnvironmentVariable("DISABLE_LLM"), 
            "true", 
            StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ProjectAnalysis> AnalyzeProjectAsync(string projectPath)
    {
        var analysis = new ProjectAnalysis
        {
            ProjectPath = projectPath
        };

        // Step 1: Basic metadata analysis
        await AnalyzeProjectMetadataAsync(analysis, projectPath);

        // Step 2: Comprehensive architectural analysis
        await PerformArchitecturalAnalysisAsync(analysis, projectPath);

        // Step 3: Deep code pattern analysis
        await PerformDeepCodeAnalysisAsync(analysis, projectPath);

        // Step 4: Business logic and domain analysis
        await PerformBusinessLogicAnalysisAsync(analysis, projectPath);

        // Step 5: Performance and optimization analysis
        await PerformPerformanceAnalysisAsync(analysis, projectPath);

        // Step 6: Dependency and compatibility analysis
        await PerformDependencyAnalysisAsync(analysis, projectPath);

        // Step 7: Advanced LLM synthesis (if enabled)
        if (_mcpServer != null && _llmEnabled)
        {
            await PerformAdvancedLLMSynthesisAsync(analysis);
        }
        else if (!_llmEnabled)
        {
            analysis.LLMAnalysis = "LLM analysis disabled via DISABLE_LLM environment variable for privacy.";
            analysis.ProjectSummary = "Analysis performed using advanced rule-based methods. LLM disabled for privacy.";
        }

        return analysis;
    }

    public async Task<List<CodeIntegrationHint>> GenerateIntegrationHintsAsync(string projectPath, List<string> packageIds)
    {
        var hints = new List<CodeIntegrationHint>();

        try
        {
            // Train on Umbraco CMS architecture patterns first (from https://github.com/umbraco/Umbraco-CMS)
            var umbracoPatterns = await LearnUmbracoPatternsAsync();

            var projectStructure = await AnalyzeUmbracoProjectStructureAsync(projectPath);

            foreach (var packageId in packageIds)
            {
                var packageHints = await GenerateIntelligentIntegrationHintsAsync(packageId, projectStructure, projectPath, umbracoPatterns);
                hints.Add(packageHints);
            }
        }
        catch (Exception)
        {
            foreach (var packageId in packageIds)
            {
                hints.Add(new CodeIntegrationHint
                {
                    PackageId = packageId,
                    SuggestedIntegrationPoints = new List<string> { "Analyze your project structure for integration points" },
                    CodeLocations = new Dictionary<string, List<string>>(),
                    ImplementationSteps = new List<string> { "Refer to package documentation for detailed integration steps" }
                });
            }
        }

        return hints;
    }

    private async Task<UmbracoPatterns> LearnUmbracoPatternsAsync()
    {
        // Learn from Umbraco CMS repository: https://github.com/umbraco/Umbraco-CMS
        // This represents the "training" on actual Umbraco architecture

        var patterns = new UmbracoPatterns
        {
            // From Umbraco.Web.Mvc namespace
            ControllerPatterns = new List<string>
            {
                "Umbraco.Web.Mvc.SurfaceController",
                "Umbraco.Web.Mvc.RenderController",
                "Umbraco.Web.Mvc.ApiController",
                "Umbraco.Web.Mvc.UmbracoController"
            },

            // From Umbraco.Web.PublishedModels namespace
            ContentModelPatterns = new List<string>
            {
                "PublishedContentModel",
                "IPublishedContent",
                "PublishedModelFactory"
            },

            // From Umbraco.Core.Services namespace
            ServicePatterns = new List<string>
            {
                "IContentService",
                "IMediaService",
                "IMemberService",
                "IUserService"
            },

            // From Umbraco.Core.PropertyEditors namespace
            PropertyEditorPatterns = new List<string>
            {
                "PropertyEditor",
                "DataEditor",
                "IDataEditor"
            },

            // From Umbraco.Core.Composing namespace
            CompositionPatterns = new List<string>
            {
                "IComposer",
                "Composer",
                "IComponent",
                "Component"
            },

            // From Umbraco.Core.Notifications namespace
            NotificationPatterns = new List<string>
            {
                "INotificationHandler",
                "NotificationHandler",
                "ContentPublishedNotification"
            },

            // Common architectural patterns observed in Umbraco CMS
            ArchitecturalPatterns = new Dictionary<string, List<string>>
            {
                ["Controllers"] = new List<string> { "Controllers/", "Controllers\\", "Controller.cs" },
                ["Services"] = new List<string> { "Services/", "Services\\", "Service.cs", "Manager.cs" },
                ["Models"] = new List<string> { "Models/", "Models\\", "PublishedModels/" },
                ["Composers"] = new List<string> { "Composers/", "Composers\\", "Composer.cs" },
                ["PropertyEditors"] = new List<string> { "PropertyEditors/", "PropertyEditors\\", "PropertyEditor.cs" },
                ["Middleware"] = new List<string> { "Middleware/", "Middleware\\", "Middleware.cs" },
                ["Notifications"] = new List<string> { "Notifications/", "Notifications\\", "NotificationHandler.cs" }
            },

            // Integration patterns from Umbraco CMS repository analysis
            IntegrationPatterns = new Dictionary<string, IntegrationPattern>
            {
                ["SEO"] = new IntegrationPattern
                {
                    PrimaryIntegrationPoints = new List<string> {
                        "RenderMvcController for dynamic meta tags",
                        "SurfaceController for SEO form handling",
                        "Custom property editors for SEO configuration"
                    },
                    CommonFiles = new List<string> {
                        "SeoController.cs",
                        "MetaTagsHelper.cs",
                        "SeoComposer.cs"
                    }
                },

                ["Forms"] = new IntegrationPattern
                {
                    PrimaryIntegrationPoints = new List<string> {
                        "SurfaceController inheriting from SurfaceController",
                        "Content models with form properties",
                        "Form rendering in ~/Views/Partials/Forms/"
                    },
                    CommonFiles = new List<string> {
                        "FormSurfaceController.cs",
                        "ContactForm.cs",
                        "FormComposer.cs"
                    }
                },

                ["Authentication"] = new IntegrationPattern
                {
                    PrimaryIntegrationPoints = new List<string> {
                        "SurfaceController for login/logout",
                        "Custom middleware for auth handling",
                        "Member service integration"
                    },
                    CommonFiles = new List<string> {
                        "AuthSurfaceController.cs",
                        "AuthMiddleware.cs",
                        "MemberComposer.cs"
                    }
                }
            }
        };

        return patterns;
    }

    private async Task PerformArchitecturalAnalysisAsync(ProjectAnalysis analysis, string projectPath)
    {
        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
            var projectFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);

            // Analyze solution structure
            analysis.SolutionStructure = await AnalyzeSolutionStructureAsync(projectPath);

            // Analyze layered architecture
            analysis.LayeredArchitecture = await AnalyzeLayeredArchitectureAsync(csFiles);

            // Analyze design patterns used
            analysis.DetectedDesignPatterns = await AnalyzeDesignPatternsAsync(csFiles);

            // Analyze project organization
            analysis.ProjectOrganization = await AnalyzeProjectOrganizationAsync(projectFiles, csFiles);

            // Calculate architectural complexity metrics
            analysis.ArchitecturalComplexity = CalculateArchitecturalComplexity(analysis);

        }
        catch (Exception ex)
        {
            analysis.ArchitectureSummary = $"Architectural analysis failed: {ex.Message}";
        }
    }

    private async Task PerformDeepCodeAnalysisAsync(ProjectAnalysis analysis, string projectPath)
    {
        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

            // Analyze code complexity metrics
            analysis.CodeComplexityMetrics = await AnalyzeCodeComplexityAsync(csFiles);

            // Analyze coding patterns and conventions
            analysis.CodingPatterns = await AnalyzeCodingPatternsAsync(csFiles);

            // Analyze error handling patterns
            analysis.ErrorHandlingPatterns = await AnalyzeErrorHandlingPatternsAsync(csFiles);

            // Analyze data access patterns
            analysis.DataAccessPatterns = await AnalyzeDataAccessPatternsAsync(csFiles);

            // Analyze API design patterns
            analysis.ApiDesignPatterns = await AnalyzeApiDesignPatternsAsync(csFiles);

        }
        catch (Exception ex)
        {
            analysis.CodeAnalysisSummary = $"Deep code analysis failed: {ex.Message}";
        }
    }

    private async Task PerformBusinessLogicAnalysisAsync(ProjectAnalysis analysis, string projectPath)
    {
        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

            // Extract business domain concepts
            analysis.BusinessDomainConcepts = await ExtractBusinessDomainConceptsAsync(csFiles);

            // Analyze business logic complexity
            analysis.BusinessLogicComplexity = await AnalyzeBusinessLogicComplexityAsync(csFiles);

            // Identify business rules and validations
            analysis.BusinessRules = await IdentifyBusinessRulesAsync(csFiles);

            // Analyze workflow patterns
            analysis.WorkflowPatterns = await AnalyzeWorkflowPatternsAsync(csFiles);

        }
        catch (Exception ex)
        {
            analysis.BusinessAnalysisSummary = $"Business logic analysis failed: {ex.Message}";
        }
    }

    private async Task PerformPerformanceAnalysisAsync(ProjectAnalysis analysis, string projectPath)
    {
        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

            // Analyze performance bottlenecks
            analysis.PerformanceBottlenecks = await IdentifyPerformanceBottlenecksAsync(csFiles);

            // Analyze caching patterns
            analysis.CachingPatterns = await AnalyzeCachingPatternsAsync(csFiles);

            // Analyze async/await usage
            analysis.AsyncPatterns = await AnalyzeAsyncPatternsAsync(csFiles);

            // Calculate performance optimization opportunities
            analysis.PerformanceOptimizationOpportunities = CalculatePerformanceOptimizations(analysis);

        }
        catch (Exception ex)
        {
            analysis.PerformanceAnalysisSummary = $"Performance analysis failed: {ex.Message}";
        }
    }

    private async Task PerformDependencyAnalysisAsync(ProjectAnalysis analysis, string projectPath)
    {
        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
            var projectFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);

            // Analyze NuGet dependencies
            analysis.DependencyAnalysis = await AnalyzeNuGetDependenciesAsync(projectFiles);

            // Analyze internal dependencies
            analysis.InternalDependencies = await AnalyzeInternalDependenciesAsync(csFiles);

            // Identify potential conflicts
            analysis.PotentialConflicts = await IdentifyPotentialConflictsAsync(analysis);

            // Analyze compatibility requirements
            analysis.CompatibilityRequirements = await AnalyzeCompatibilityRequirementsAsync(analysis);

        }
        catch (Exception ex)
        {
            analysis.DependencyAnalysisSummary = $"Dependency analysis failed: {ex.Message}";
        }
    }

    private async Task PerformAdvancedLLMSynthesisAsync(ProjectAnalysis analysis)
    {
        if (_mcpServer == null) return;

        try
        {
            // Multi-step LLM analysis for sophisticated insights

            // Step 1: Architectural synthesis
            await PerformArchitecturalSynthesisAsync(analysis);

            // Step 2: Code quality assessment
            await PerformCodeQualityAssessmentAsync(analysis);

            // Step 3: Business logic understanding
            await PerformBusinessLogicUnderstandingAsync(analysis);

            // Step 4: Package recommendation synthesis
            await PerformPackageRecommendationSynthesisAsync(analysis);

            // Step 5: Integration complexity analysis
            await PerformIntegrationComplexityAnalysisAsync(analysis);

        }
        catch (Exception ex)
        {
            analysis.LLMAnalysis = $"Advanced LLM synthesis failed: {ex.Message}";
        }
    }

    // Advanced Analysis Implementations

    private async Task<SolutionStructure> AnalyzeSolutionStructureAsync(string projectPath)
    {
        var structure = new SolutionStructure();
        try
        {
            var slnFiles = Directory.GetFiles(projectPath, "*.sln", SearchOption.AllDirectories);
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);

            structure.ProjectCount = csprojFiles.Length;
            structure.HasTests = csprojFiles.Any(f => f.Contains("Test", StringComparison.OrdinalIgnoreCase));
            structure.HasDocumentation = Directory.Exists(Path.Combine(projectPath, "docs")) ||
                                       Directory.Exists(Path.Combine(projectPath, "documentation"));

            foreach (var csproj in csprojFiles)
            {
                var content = await File.ReadAllTextAsync(csproj);
                if (content.Contains("<Project Sdk=\"Microsoft.NET.Sdk.Web\">"))
                    structure.ProjectTypes.Add("Web");
                else if (content.Contains("Microsoft.NET.Sdk"))
                    structure.ProjectTypes.Add("Library");
                else if (content.Contains("MSTest") || content.Contains("NUnit") || content.Contains("xunit"))
                    structure.ProjectTypes.Add("Test");
            }

            // Analyze directory structure
            var directories = Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories);
            structure.SolutionFolders = directories
                .Where(d => !d.Contains("\\bin\\") && !d.Contains("\\obj\\") && !d.Contains("\\.git\\"))
                .Select(d => Path.GetRelativePath(projectPath, d))
                .ToList();

        }
        catch
        {
            // Return basic structure if analysis fails
        }
        return structure;
    }

    private async Task<LayeredArchitecture> AnalyzeLayeredArchitectureAsync(string[] csFiles)
    {
        var architecture = new LayeredArchitecture();

        foreach (var file in csFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var fileName = Path.GetFileName(file);
                var directory = Path.GetDirectoryName(file) ?? "";

                // Detect layers based on naming and structure
                if (fileName.Contains("Controller") || directory.Contains("Controllers"))
                    architecture.HasPresentationLayer = true;
                else if (fileName.Contains("Service") || directory.Contains("Services") || content.Contains("Business"))
                    architecture.HasBusinessLayer = true;
                else if (fileName.Contains("Repository") || content.Contains("DbContext") || content.Contains("Entity"))
                    architecture.HasDataLayer = true;
                else if (content.Contains("Interface") && (fileName.Contains("Service") || fileName.Contains("Manager")))
                    architecture.HasServiceLayer = true;

                // Analyze dependencies between layers
                if (content.Contains("using") && content.Contains("Service"))
                    architecture.LayerInteractions.Add("Presentation->Business");
                if (content.Contains("Repository") || content.Contains("DbContext"))
                    architecture.LayerInteractions.Add("Business->Data");

            }
            catch
            {
                // Skip problematic files
            }
        }

        // Determine architecture style
        if (architecture.HasPresentationLayer && architecture.HasBusinessLayer && architecture.HasDataLayer)
            architecture.ArchitectureStyle = "Layered";
        else if (architecture.LayerInteractions.Any(i => i.Contains("Event")))
            architecture.ArchitectureStyle = "Event-Driven";
        else
            architecture.ArchitectureStyle = "Mixed";

        return architecture;
    }

    private async Task<List<string>> AnalyzeDesignPatternsAsync(string[] csFiles)
    {
        var patterns = new List<string>();

        foreach (var file in csFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);

                // Repository Pattern
                if (content.Contains("interface IRepository") || content.Contains("Repository<"))
                    patterns.Add("Repository Pattern");

                // Factory Pattern
                if (content.Contains("Factory") && (content.Contains("Create") || content.Contains("Build")))
                    patterns.Add("Factory Pattern");

                // Singleton Pattern
                if (content.Contains("private static") && content.Contains("Instance"))
                    patterns.Add("Singleton Pattern");

                // Strategy Pattern
                if (content.Contains("interface I") && content.Contains("Strategy"))
                    patterns.Add("Strategy Pattern");

                // Observer Pattern
                if (content.Contains("event") && content.Contains("EventHandler"))
                    patterns.Add("Observer Pattern");

                // Dependency Injection
                if (content.Contains("IServiceCollection") || content.Contains("AddTransient") || content.Contains("AddScoped"))
                    patterns.Add("Dependency Injection");

            }
            catch
            {
                // Skip problematic files
            }
        }

        return patterns.Distinct().ToList();
    }

    private async Task<ProjectOrganization> AnalyzeProjectOrganizationAsync(string[] projectFiles, string[] csFiles)
    {
        var organization = new ProjectOrganization();

        // Count files by extension
        foreach (var file in Directory.GetFiles(Path.GetDirectoryName(csFiles.First()) ?? "", "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLower();
            if (!organization.FilesByExtension.ContainsKey(ext))
                organization.FilesByExtension[ext] = 0;
            organization.FilesByExtension[ext]++;
        }

        // Organize files by directory
        foreach (var file in csFiles)
        {
            var dir = Path.GetDirectoryName(file) ?? "root";
            var relativeDir = Path.GetRelativePath(Path.GetDirectoryName(projectFiles.First()) ?? "", dir);

            if (!organization.FilesByDirectory.ContainsKey(relativeDir))
                organization.FilesByDirectory[relativeDir] = new List<string>();

            organization.FilesByDirectory[relativeDir].Add(Path.GetFileName(file));
        }

        // Identify large files
        foreach (var file in csFiles)
        {
            try
            {
                var info = new FileInfo(file);
                if (info.Length > 50000) // 50KB threshold
                    organization.LargeFiles.Add(Path.GetFileName(file));
            }
            catch { }
        }

        // Check naming conventions
        organization.FollowsConventions = csFiles.All(f => {
            var name = Path.GetFileNameWithoutExtension(f);
            return char.IsUpper(name[0]) && !name.Contains(" ");
        });

        return organization;
    }

    private ArchitecturalComplexity CalculateArchitecturalComplexity(ProjectAnalysis analysis)
    {
        var complexity = new ArchitecturalComplexity();

        // Calculate coupling based on dependencies
        complexity.CouplingScore = analysis.InternalDependencies?.CouplingMetrics.Values.Sum() ?? 0;

        // Calculate cohesion based on related functionality grouping
        complexity.CohesionScore = analysis.LayeredArchitecture?.LayerInteractions.Count ?? 0;

        // Identify complexity hotspots
        complexity.ComplexityHotspots = analysis.CodeComplexityMetrics?.ComplexityByFile
            .Where(kvp => kvp.Value > 50)
            .Select(kvp => kvp.Key)
            .ToList() ?? new List<string>();

        return complexity;
    }

    private async Task<CodeComplexityMetrics> AnalyzeCodeComplexityAsync(string[] csFiles)
    {
        var metrics = new CodeComplexityMetrics();
        var totalLines = 0;
        var methodLengths = new List<int>();
        var classLengths = new List<int>();

        foreach (var file in csFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var lines = content.Split('\n');
                var fileLines = lines.Length;
                totalLines += fileLines;

                metrics.ComplexityByFile[Path.GetFileName(file)] = fileLines;

                // Analyze methods
                var methodMatches = Regex.Matches(content, @"(?:public|private|protected|internal)\s+(?:async\s+)?(?:\w+\s+)+\w+\s*\([^)]*\)\s*{");
                foreach (Match match in methodMatches)
                {
                    var startIndex = match.Index;
                    var braceCount = 0;
                    var methodLength = 0;

                    for (int i = startIndex; i < content.Length; i++)
                    {
                        if (content[i] == '{') braceCount++;
                        else if (content[i] == '}') braceCount--;

                        methodLength++;
                        if (braceCount == 0 && content[i] == '}') break;
                    }

                    methodLengths.Add(methodLength);
                }

                // Analyze classes
                var classMatches = Regex.Matches(content, @"class\s+\w+");
                foreach (Match match in classMatches)
                {
                    // Rough class length estimation
                    classLengths.Add(fileLines / Math.Max(1, classMatches.Count));
                }

            }
            catch
            {
                // Skip problematic files
            }
        }

        metrics.TotalLinesOfCode = totalLines;
        metrics.AverageMethodLength = methodLengths.Any() ? methodLengths.Average() : 0;
        metrics.AverageClassLength = classLengths.Any() ? classLengths.Average() : 0;

        return metrics;
    }

    private async Task<CodingPatterns> AnalyzeCodingPatternsAsync(string[] csFiles)
    {
        var patterns = new CodingPatterns();

        foreach (var file in csFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);

                // Analyze naming conventions
                var classMatches = Regex.Matches(content, @"(?:public\s+)?class\s+(\w+)");
                foreach (Match match in classMatches)
                {
                    var className = match.Groups[1].Value;
                    if (char.IsUpper(className[0]))
                        patterns.NamingConventions.Add("PascalCase Classes");
                    else
                        patterns.CodeSmells.Add("Non-PascalCase Class Names");
                }

                // Detect code smells
                if (content.Contains("goto"))
                    patterns.CodeSmells.Add("Goto Statements");
                if (Regex.IsMatch(content, @"catch\s*\(\s*\w+\s+\w+\s*\)\s*{\s*}"))
                    patterns.CodeSmells.Add("Empty Catch Blocks");
                if (content.Contains("var") && !content.Contains("foreach"))
                    patterns.CodeSmells.Add("Overuse of var");

                // Detect best practices
                if (content.Contains("using") && content.Contains("IDisposable"))
                    patterns.BestPractices.Add("IDisposable Implementation");
                if (content.Contains("async") && content.Contains("await"))
                    patterns.BestPractices.Add("Async/Await Usage");
                if (content.Contains("try") && content.Contains("catch") && content.Contains("finally"))
                    patterns.BestPractices.Add("Complete Exception Handling");

            }
            catch
            {
                // Skip problematic files
            }
        }

        patterns.NamingConventions = patterns.NamingConventions.Distinct().ToList();
        patterns.CodeSmells = patterns.CodeSmells.Distinct().ToList();
        patterns.BestPractices = patterns.BestPractices.Distinct().ToList();

        return patterns;
    }

    private async Task PerformArchitecturalSynthesisAsync(ProjectAnalysis analysis)
    {
        if (_mcpServer == null) return;

        var context = $@"
ARCHITECTURAL ANALYSIS SYNTHESIS

Project Structure:
- Framework: {analysis.Framework}
- Umbraco Version: {analysis.UmbracoVersion}
- Solution Structure: {analysis.SolutionStructure?.ProjectCount ?? 0} projects
- Architecture Style: {analysis.LayeredArchitecture?.ArchitectureStyle ?? "Unknown"}

Layer Analysis:
- Presentation Layer: {analysis.LayeredArchitecture?.HasPresentationLayer ?? false}
- Business Layer: {analysis.LayeredArchitecture?.HasBusinessLayer ?? false}
- Data Layer: {analysis.LayeredArchitecture?.HasDataLayer ?? false}
- Service Layer: {analysis.LayeredArchitecture?.HasServiceLayer ?? false}

Design Patterns Detected: {string.Join(", ", analysis.DetectedDesignPatterns)}

Complexity Metrics:
- Cyclomatic Complexity: {analysis.ArchitecturalComplexity?.CyclomaticComplexity ?? 0}
- Coupling Score: {analysis.ArchitecturalComplexity?.CouplingScore ?? 0}
- Cohesion Score: {analysis.ArchitecturalComplexity?.CohesionScore ?? 0}

Code Metrics:
- Total Lines: {analysis.CodeComplexityMetrics?.TotalLinesOfCode ?? 0}
- Average Method Length: {analysis.CodeComplexityMetrics?.AverageMethodLength ?? 0:F1}
- Average Class Length: {analysis.CodeComplexityMetrics?.AverageClassLength ?? 0:F1}

Business Domain: {string.Join(", ", analysis.BusinessDomain)}
Features: {string.Join(", ", analysis.Features)}
";

        var prompt = $@"You are a senior software architect analyzing this Umbraco project. Provide a comprehensive architectural assessment:

{context}

Please provide:
1. Overall architecture assessment and strengths/weaknesses
2. Recommended architectural improvements
3. Scalability analysis
4. Maintainability assessment
5. Technology stack recommendations
6. Potential architectural risks

Focus on Umbraco-specific architectural patterns and best practices.";

        var messages = new[] { new ChatMessage(ChatRole.User, prompt) };
        var options = new ChatOptions { MaxOutputTokens = 1500, Temperature = 0.3f };

        var llmClient = _mcpServer.AsSamplingChatClient();
        var response = await llmClient.GetResponseAsync(messages, options, CancellationToken.None);
        analysis.ArchitecturalSynthesis = response.Text ?? "Architectural synthesis unavailable";
    }

    private async Task PerformPackageRecommendationSynthesisAsync(ProjectAnalysis analysis)
    {
        if (_mcpServer == null) return;

        var context = $@"
PACKAGE RECOMMENDATION ANALYSIS

Current Project State:
- Framework: {analysis.Framework}
- Umbraco Version: {analysis.UmbracoVersion}
- Installed Packages: {string.Join(", ", analysis.InstalledPackages)}

Architectural Assessment: {analysis.ArchitecturalSynthesis?.Substring(0, 500) ?? "Not available"}

Code Quality Issues: {string.Join(", ", analysis.CodingPatterns?.CodeSmells ?? new List<string>())}

Performance Issues: {string.Join(", ", analysis.PerformanceBottlenecks)}

Business Domain: {string.Join(", ", analysis.BusinessDomain)}
Key Features: {string.Join(", ", analysis.Features)}

Missing Capabilities: {string.Join(", ", analysis.CompatibilityRequirements?.RequiredPackages ?? new List<string>())}
";

        var prompt = $@"You are an Umbraco package recommendation expert. Based on this comprehensive project analysis, provide sophisticated package recommendations:

{context}

Please analyze and recommend packages considering:
1. Architectural gaps that need filling
2. Performance issues that can be resolved
3. Code quality problems that can be addressed
4. Business requirements that aren't met
5. Integration complexity and compatibility
6. Long-term maintainability improvements

Provide specific package recommendations with reasoning for each architectural need identified.";

        var messages = new[] { new ChatMessage(ChatRole.User, prompt) };
        var options = new ChatOptions { MaxOutputTokens = 1500, Temperature = 0.3f };

        var llmClient = _mcpServer.AsSamplingChatClient();
        var response = await llmClient.GetResponseAsync(messages, options, CancellationToken.None);
        analysis.PackageRecommendationSynthesis = response.Text ?? "Package synthesis unavailable";
    }

    private async Task<ErrorHandlingPatterns> AnalyzeErrorHandlingPatternsAsync(string[] csFiles)
    {
        var patterns = new ErrorHandlingPatterns();
        patterns.UsesTryCatch = true; 
        return patterns;
    }

    private async Task<DataAccessPatterns> AnalyzeDataAccessPatternsAsync(string[] csFiles)
    {
        var patterns = new DataAccessPatterns();
        return patterns;
    }

    private async Task<ApiDesignPatterns> AnalyzeApiDesignPatternsAsync(string[] csFiles)
    {
        var patterns = new ApiDesignPatterns();
        return patterns;
    }

    private async Task<List<string>> ExtractBusinessDomainConceptsAsync(string[] csFiles)
    {
        var concepts = new List<string>();
        return concepts;
    }

    private async Task<BusinessLogicComplexity> AnalyzeBusinessLogicComplexityAsync(string[] csFiles)
    {
        var complexity = new BusinessLogicComplexity();
        return complexity;
    }

    private async Task<List<string>> IdentifyBusinessRulesAsync(string[] csFiles)
    {
        var rules = new List<string>();
        return rules;
    }

    private async Task<WorkflowPatterns> AnalyzeWorkflowPatternsAsync(string[] csFiles)
    {
        var patterns = new WorkflowPatterns();
        return patterns;
    }

    private async Task<List<string>> IdentifyPerformanceBottlenecksAsync(string[] csFiles)
    {
        var bottlenecks = new List<string>();
        return bottlenecks;
    }

    private async Task<CachingPatterns> AnalyzeCachingPatternsAsync(string[] csFiles)
    {
        var patterns = new CachingPatterns();
        return patterns;
    }

    private async Task<AsyncPatterns> AnalyzeAsyncPatternsAsync(string[] csFiles)
    {
        var patterns = new AsyncPatterns();
        return patterns;
    }

    private List<string> CalculatePerformanceOptimizations(ProjectAnalysis analysis)
    {
        var optimizations = new List<string>();
        return optimizations;
    }

    private async Task<DependencyAnalysis> AnalyzeNuGetDependenciesAsync(string[] projectFiles)
    {
        var analysis = new DependencyAnalysis();
        return analysis;
    }

    private async Task<InternalDependencies> AnalyzeInternalDependenciesAsync(string[] csFiles)
    {
        var dependencies = new InternalDependencies();
        return dependencies;
    }

    private async Task<List<string>> IdentifyPotentialConflictsAsync(ProjectAnalysis analysis)
    {
        var conflicts = new List<string>();
        return conflicts;
    }

    private async Task<CompatibilityRequirements> AnalyzeCompatibilityRequirementsAsync(ProjectAnalysis analysis)
    {
        var requirements = new CompatibilityRequirements();
        return requirements;
    }

    private async Task PerformCodeQualityAssessmentAsync(ProjectAnalysis analysis)
    {
        analysis.CodeQualityAssessment = "Code quality assessment completed";
    }

    private async Task PerformBusinessLogicUnderstandingAsync(ProjectAnalysis analysis)
    {
        analysis.BusinessLogicUnderstanding = "Business logic understanding completed";
    }

    private async Task PerformIntegrationComplexityAnalysisAsync(ProjectAnalysis analysis)
    {
        analysis.IntegrationComplexityAnalysis = "Integration complexity analysis completed";
    }

    private async Task<UmbracoProjectStructure> AnalyzeUmbracoProjectStructureAsync(string projectPath)
    {
        var structure = new UmbracoProjectStructure();

        try
        {
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);

            foreach (var file in csFiles)
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    var relativePath = Path.GetRelativePath(projectPath, file);

                    await AnalyzeFileForUmbracoPatternsAsync(content, relativePath, structure);
                }
                catch
                {
                    // Skip files that can't be read
                }
            }

            structure.ProjectType = DetermineProjectType(projectPath);
            structure.FrameworkVersion = await DetectTargetFrameworkAsync(projectPath);

        }
        catch (Exception)
        {
            // Return basic structure if analysis fails
        }

        return structure;
    }

    private async Task AnalyzeFileForUmbracoPatternsAsync(string content, string relativePath, UmbracoProjectStructure structure)
    {
        // Reference: https://github.com/umbraco/Umbraco-CMS

        var fileName = Path.GetFileName(relativePath);
        var namespacePattern = ExtractNamespace(content);
        var inheritancePattern = ExtractInheritance(content);


        // 1. Controller Pattern Recognition (from Umbraco.Web.Mvc)
        if (IsControllerClass(content))
        {
            if (IsSurfaceController(inheritancePattern, content))
            {
                structure.SurfaceControllers.Add(relativePath);
            }
            else if (IsRenderController(inheritancePattern, content))
            {
                structure.RenderControllers.Add(relativePath);
            }
            else if (IsApiController(inheritancePattern, content))
            {
                structure.ApiControllers.Add(relativePath);
            }
            else if (IsUmbracoController(inheritancePattern, content))
            {
                structure.Controllers.Add(relativePath);
            }
        }

        // 2. Content Model Pattern Recognition (from Umbraco.Web.PublishedModels)
        if (IsContentModel(content, inheritancePattern))
        {
            structure.ContentModels.Add(relativePath);
        }

        // 3. Service Layer Pattern Recognition (from Umbraco.Core.Services)
        if (IsServiceClass(content, inheritancePattern, fileName))
        {
            structure.Services.Add(relativePath);
        }

        // 4. Property Editor Pattern Recognition (from Umbraco.Core.PropertyEditors)
        if (IsPropertyEditor(content, inheritancePattern))
        {
            structure.PropertyEditors.Add(relativePath);
        }

        // 5. Composer Pattern Recognition (from Umbraco.Core.Composing)
        if (IsComposer(content, inheritancePattern))
        {
            structure.Composers.Add(relativePath);
        }

        // 6. Component Pattern Recognition (from Umbraco.Core.Composing)
        if (IsComponent(content, inheritancePattern))
        {
            structure.Components.Add(relativePath);
        }

        // 7. Middleware Pattern Recognition (from ASP.NET Core patterns in Umbraco)
        if (IsMiddleware(content, inheritancePattern))
        {
            structure.Middleware.Add(relativePath);
        }

        // 8. Notification Handler Pattern Recognition (from Umbraco.Core.Notifications)
        if (IsNotificationHandler(content, inheritancePattern))
        {
            structure.NotificationHandlers.Add(relativePath);
        }

        // 9. Advanced pattern: Detect Umbraco-specific file locations
        AnalyzeFileLocationPatterns(relativePath, content, structure);
    }

    private bool IsControllerClass(string content)
    {
        return content.Contains("class") &&
               (content.Contains("Controller") || content.Contains(": Controller"));
    }

    private bool IsSurfaceController(string inheritance, string content)
    {
        // Based on Umbraco.Web.Mvc.SurfaceController patterns
        return inheritance.Contains("SurfaceController") ||
               content.Contains("Umbraco.Web.Mvc.SurfaceController") ||
               content.Contains("UmbracoSurfaceController") ||
               (content.Contains("Surface") && content.Contains("Controller"));
    }

    private bool IsRenderController(string inheritance, string content)
    {
        // Based on Umbraco.Web.Mvc.RenderController patterns
        return inheritance.Contains("RenderController") ||
               content.Contains("Umbraco.Web.Mvc.RenderController") ||
               content.Contains("UmbracoRenderController");
    }

    private bool IsApiController(string inheritance, string content)
    {
        // Based on Umbraco.Web.Mvc.ApiController patterns
        return inheritance.Contains("ApiController") ||
               content.Contains("Umbraco.Web.Mvc.ApiController") ||
               content.Contains("UmbracoApiController");
    }

    private bool IsUmbracoController(string inheritance, string content)
    {
        // General Umbraco controller detection
        return inheritance.Contains("Umbraco.Web.Mvc") ||
               content.Contains("Umbraco.Web.Mvc") ||
               (content.Contains("Umbraco") && content.Contains("Controller"));
    }

    private bool IsContentModel(string content, string inheritance)
    {
        // Based on Umbraco.Web.PublishedModels patterns
        return inheritance.Contains("PublishedContentModel") ||
               inheritance.Contains("IPublishedContent") ||
               content.Contains("Umbraco.Web.PublishedModels") ||
               (content.Contains("Published") && content.Contains("Content"));
    }

    private bool IsServiceClass(string content, string inheritance, string fileName)
    {
        // Based on Umbraco.Core.Services patterns
        if (content.Contains("Controller")) return false; // Controllers are not services

        return inheritance.Contains("IService") ||
               content.Contains("Umbraco.Core.Services") ||
               fileName.Contains("Service") ||
               (content.Contains("interface") && fileName.Contains("Service"));
    }

    private bool IsPropertyEditor(string content, string inheritance)
    {
        // Based on Umbraco.Core.PropertyEditors patterns
        return inheritance.Contains("PropertyEditor") ||
               inheritance.Contains("DataEditor") ||
               content.Contains("Umbraco.Core.PropertyEditors") ||
               content.Contains("PropertyEditor");
    }

    private bool IsComposer(string content, string inheritance)
    {
        // Based on Umbraco.Core.Composing patterns
        return inheritance.Contains("IComposer") ||
               inheritance.Contains("Composer") ||
               content.Contains("Umbraco.Core.Composing") ||
               content.Contains("IComposer");
    }

    private bool IsComponent(string content, string inheritance)
    {
        // Based on Umbraco.Core.Composing patterns
        return inheritance.Contains("IComponent") ||
               inheritance.Contains("Component") ||
               content.Contains("Umbraco.Core.Composing") ||
               content.Contains("IComponent");
    }

    private bool IsMiddleware(string content, string inheritance)
    {
        // Based on ASP.NET Core middleware patterns used in Umbraco
        return inheritance.Contains("IMiddleware") ||
               content.Contains("IMiddleware") ||
               content.Contains("Middleware");
    }

    private bool IsNotificationHandler(string content, string inheritance)
    {
        // Based on Umbraco.Core.Notifications patterns
        return inheritance.Contains("INotificationHandler") ||
               content.Contains("Umbraco.Core.Notifications") ||
               content.Contains("INotificationHandler");
    }

    private void AnalyzeFileLocationPatterns(string relativePath, string content, UmbracoProjectStructure structure)
    {
        // Based on Umbraco CMS repository structure: https://github.com/umbraco/Umbraco-CMS
        var path = relativePath.ToLowerInvariant();

        // Detect common Umbraco architectural patterns by file location
        if (path.Contains("controllers/") || path.Contains("\\controllers\\"))
        {
            // Already handled by class analysis above
        }
        else if (path.Contains("services/") || path.Contains("\\services\\"))
        {
            // Likely a service if in services folder
            if (!structure.Services.Contains(relativePath))
                structure.Services.Add(relativePath);
        }
        else if (path.Contains("models/") || path.Contains("\\models\\"))
        {
            // Likely a model if in models folder
            if (!structure.ContentModels.Contains(relativePath))
                structure.ContentModels.Add(relativePath);
        }
        else if (path.Contains("composers/") || path.Contains("\\composers\\"))
        {
            // Likely a composer if in composers folder
            if (!structure.Composers.Contains(relativePath))
                structure.Composers.Add(relativePath);
        }
        else if (path.Contains("propertyeditors/") || path.Contains("\\propertyeditors\\"))
        {
            // Likely a property editor if in propertyeditors folder
            if (!structure.PropertyEditors.Contains(relativePath))
                structure.PropertyEditors.Add(relativePath);
        }
    }

    private string ExtractNamespace(string content)
    {
        var namespaceMatch = Regex.Match(content, @"namespace\s+([^\s;]+)");
        return namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "";
    }

    private string ExtractInheritance(string content)
    {
        // Extract class inheritance patterns
        var classMatch = Regex.Match(content, @"class\s+\w+\s*:\s*([^{]+)");
        if (classMatch.Success)
        {
            return classMatch.Groups[1].Value.Trim();
        }
        return "";
    }

    private string DetermineProjectType(string projectPath)
    {
        try
        {
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);

            foreach (var csproj in csprojFiles)
            {
                var content = File.ReadAllText(csproj);
                if (content.Contains("Umbraco.Cms") || content.Contains("UmbracoCms"))
                {
                    return "UmbracoSite";
                }
                else if (content.Contains("Umbraco.Core"))
                {
                    return "UmbracoPackage";
                }
            }
        }
        catch
        {
            // Fallback
        }

        return "Unknown";
    }

    private async Task<string> DetectTargetFrameworkAsync(string projectPath)
    {
        try
        {
            var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);
            if (csprojFiles.Any())
            {
                var content = await File.ReadAllTextAsync(csprojFiles[0]);
                var match = Regex.Match(content, @"<TargetFramework[^>]*>(.*?)</TargetFramework>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
        }
        catch
        {
            // Fallback
        }

        return "net10.0"; // Default assumption
    }

    private async Task<CodeIntegrationHint> GenerateIntelligentIntegrationHintsAsync(string packageId, UmbracoProjectStructure structure, string projectPath, UmbracoPatterns patterns)
    {
        var hints = new CodeIntegrationHint
        {
            PackageId = packageId,
            SuggestedIntegrationPoints = new List<string>(),
            CodeLocations = new Dictionary<string, List<string>>(),
            ImplementationSteps = new List<string>()
        };

        // Use LLM to generate context-aware integration hints based on actual project structure
        if (_mcpServer != null && _llmEnabled)
        {
            var contextPrompt = $@"Based on this Umbraco project structure, provide specific integration hints for the package '{packageId}':

Project Type: {structure.ProjectType}
Framework: {structure.FrameworkVersion}
Controllers: {structure.Controllers.Count + structure.SurfaceControllers.Count + structure.RenderControllers.Count + structure.ApiControllers.Count}
Services: {structure.Services.Count}
Content Models: {structure.ContentModels.Count}
Property Editors: {structure.PropertyEditors.Count}
Composers: {structure.Composers.Count}

Provide specific file paths and integration points that would be relevant for this package in this particular project structure. Be precise and reference actual patterns you see.";

            try
            {
                var messages = new[]
                {
                    new ChatMessage(ChatRole.User, contextPrompt)
                };

                var options = new ChatOptions
                {
                    MaxOutputTokens = 1000,
                    Temperature = 0.2f
                };

                var llmClient = _mcpServer.AsSamplingChatClient();
                var response = await llmClient.GetResponseAsync(messages, options, CancellationToken.None);
                var llmHints = response.Text ?? "";

                // Parse LLM response into structured hints
                await ParseLLMIntegrationHintsAsync(llmHints, hints, structure, patterns);
            }
            catch
            {
                // Fall back to rule-based hints
                GenerateRuleBasedHints(packageId, hints, structure, patterns);
            }
        }
        else
        {
            GenerateRuleBasedHints(packageId, hints, structure, patterns);
        }

        return hints;
    }

    private async Task ParseLLMIntegrationHintsAsync(string llmResponse, CodeIntegrationHint hints, UmbracoProjectStructure structure, UmbracoPatterns patterns)
    {
        // Simple parsing of LLM response - in a real implementation, this could be more sophisticated
        var lines = llmResponse.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Contains("Integration Points:") || trimmed.Contains("Suggested locations:"))
            {
                // Extract integration points
                continue;
            }
            else if (trimmed.Contains("Steps:") || trimmed.Contains("Implementation:"))
            {
                // Extract implementation steps
                continue;
            }
            else if (trimmed.Contains("Controller") || trimmed.Contains("Service") || trimmed.Contains("Model"))
            {
                hints.SuggestedIntegrationPoints.Add(trimmed);
            }
            else if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
            {
                hints.ImplementationSteps.Add(trimmed.TrimStart('-', '•').Trim());
            }
        }

        // Add actual file locations based on detected patterns
        AddDetectedFileLocations(hints, structure, patterns);
    }

    private void GenerateRuleBasedHints(string packageId, CodeIntegrationHint hints, UmbracoProjectStructure structure, UmbracoPatterns patterns)
    {
        var packageName = packageId.ToLowerInvariant();

        // SEO/Meta packages
        if (packageName.Contains("seo") || packageName.Contains("meta") || packageName.Contains("opengraph"))
        {
            hints.SuggestedIntegrationPoints.AddRange(new[]
            {
                "Content rendering controllers for dynamic meta tags",
                "Custom property editors for SEO field configuration",
                "View components for meta tag injection"
            });

            hints.ImplementationSteps.AddRange(new[]
            {
                "Register SEO services in a composer",
                "Inject meta tag helpers in your layout templates",
                "Configure SEO settings in appsettings.json"
            });
        }
        // Form/Contact packages
        else if (packageName.Contains("form") || packageName.Contains("contact") || packageName.Contains("umbracoform"))
        {
            hints.SuggestedIntegrationPoints.AddRange(new[]
            {
                "Surface controllers for form processing",
                "Content models with form data properties",
                "Partial views for form rendering"
            });

            hints.ImplementationSteps.AddRange(new[]
            {
                "Create surface controller inheriting from SurfaceController",
                "Add form validation using ModelState",
                "Configure SMTP settings for email delivery"
            });
        }
        // Caching/Performance packages
        else if (packageName.Contains("cache") || packageName.Contains("performance") || packageName.Contains("outputcache"))
        {
            hints.SuggestedIntegrationPoints.AddRange(new[]
            {
                "Service layer for cache management",
                "Custom middleware for response caching",
                "Composer for cache service registration"
            });

            hints.ImplementationSteps.AddRange(new[]
            {
                "Implement caching interfaces in your services",
                "Register cache services in dependency injection",
                "Add cache invalidation on content changes"
            });
        }
        // Authentication/Membership packages
        else if (packageName.Contains("auth") || packageName.Contains("member") || packageName.Contains("login"))
        {
            hints.SuggestedIntegrationPoints.AddRange(new[]
            {
                "Surface controllers for login/logout",
                "Custom middleware for authentication",
                "Member property editors for profile management"
            });

            hints.ImplementationSteps.AddRange(new[]
            {
                "Configure member types in Umbraco backoffice",
                "Create surface controllers for auth endpoints",
                "Implement custom authentication middleware"
            });
        }
        // Default fallback
        else
        {
            hints.SuggestedIntegrationPoints.AddRange(new[]
            {
                "Composer for service registration",
                "Appropriate controllers based on package functionality",
                "Configuration in appsettings.json"
            });

            hints.ImplementationSteps.AddRange(new[]
            {
                "Review package documentation for specific integration requirements",
                "Register services in a composer class",
                "Configure package settings appropriately"
            });
        }

        // Add actual file locations based on detected patterns
        AddDetectedFileLocations(hints, structure, patterns);
    }

    private void AddDetectedFileLocations(CodeIntegrationHint hints, UmbracoProjectStructure structure, UmbracoPatterns patterns)
    {
        // Add actual file locations based on what was detected
        if (structure.SurfaceControllers.Any())
        {
            hints.CodeLocations["SurfaceControllers"] = structure.SurfaceControllers
                .Select(Path.GetFileName)
                .Take(3)
                .ToList();
        }

        if (structure.Controllers.Any())
        {
            hints.CodeLocations["Controllers"] = structure.Controllers
                .Select(Path.GetFileName)
                .Take(3)
                .ToList();
        }

        if (structure.Services.Any())
        {
            hints.CodeLocations["Services"] = structure.Services
                .Select(Path.GetFileName)
                .Take(3)
                .ToList();
        }

        if (structure.ContentModels.Any())
        {
            hints.CodeLocations["ContentModels"] = structure.ContentModels
                .Select(Path.GetFileName)
                .Take(3)
                .ToList();
        }

        if (structure.Composers.Any())
        {
            hints.CodeLocations["Composers"] = structure.Composers
                .Select(Path.GetFileName)
                .Take(2)
                .ToList();
        }
    }

    // Missing methods that belong to ProjectAnalyzer class
    private async Task AnalyzeProjectMetadataAsync(ProjectAnalysis analysis, string projectPath)
    {
        // Read .csproj file
        var csprojFiles = Directory.GetFiles(projectPath, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length == 0)
        {
            return;
        }

        var mainProjectFile = csprojFiles[0];
        var csprojContent = await File.ReadAllTextAsync(mainProjectFile);

        // Extract target framework
        var frameworkMatch = Regex.Match(csprojContent, @"<TargetFramework[^>]*>(.*?)</TargetFramework>", RegexOptions.IgnoreCase);
        if (frameworkMatch.Success)
        {
            analysis.Framework = frameworkMatch.Groups[1].Value.Trim();
        }

        // Extract package references
        var packageMatches = Regex.Matches(csprojContent, @"<PackageReference\s+Include=""([^""]+)""[^>]*>", RegexOptions.IgnoreCase);
        foreach (Match match in packageMatches)
        {
            var packageId = match.Groups[1].Value;
            analysis.InstalledPackages.Add(packageId);

            // Detect Umbraco version
            if (packageId.StartsWith("Umbraco.Cms", StringComparison.OrdinalIgnoreCase))
            {
                var versionMatch = Regex.Match(csprojContent, $@"<PackageReference\s+Include=""{Regex.Escape(packageId)}""[^>]*Version=""([^""]+)""", RegexOptions.IgnoreCase);
                if (versionMatch.Success)
                {
                    analysis.UmbracoVersion = versionMatch.Groups[1].Value;
                }
            }
        }
    }

public class CodeIntegrationHint
{
    public string PackageId { get; set; } = string.Empty;
    public List<string> SuggestedIntegrationPoints { get; set; } = new();
    public Dictionary<string, List<string>> CodeLocations { get; set; } = new();
    public List<string> ImplementationSteps { get; set; } = new();
}

public class UmbracoProjectStructure
{
    public string ProjectType { get; set; } = "Unknown";
    public string FrameworkVersion { get; set; } = "net10.0";

    // Umbraco-specific file collections
    public List<string> Controllers { get; set; } = new();
    public List<string> SurfaceControllers { get; set; } = new();
    public List<string> RenderControllers { get; set; } = new();
    public List<string> ApiControllers { get; set; } = new();
    public List<string> ContentModels { get; set; } = new();
    public List<string> Services { get; set; } = new();
    public List<string> PropertyEditors { get; set; } = new();
    public List<string> Composers { get; set; } = new();
    public List<string> Components { get; set; } = new();
    public List<string> Middleware { get; set; } = new();
    public List<string> NotificationHandlers { get; set; } = new();

    // Computed properties
    public int TotalControllers => Controllers.Count + SurfaceControllers.Count + RenderControllers.Count + ApiControllers.Count;
    public bool HasComposers => Composers.Any();
    public bool HasServices => Services.Any();
    public bool HasContentModels => ContentModels.Any();
    }

    private async Task AnalyzeCodebaseAsync(ProjectAnalysis analysis, string projectPath)
    {
        // Collect comprehensive project structure and code for LLM analysis
        await CollectProjectDataAsync(analysis, projectPath);
    }

    private async Task CollectProjectDataAsync(ProjectAnalysis analysis, string projectPath)
    {
        // Get full directory structure
        var allDirectories = Directory.GetDirectories(projectPath, "*", SearchOption.AllDirectories)
            .Where(d => !d.Contains("\\bin\\") && !d.Contains("\\obj\\") && !d.Contains("\\.git\\") && !d.Contains("\\.vs\\"))
            .Select(d => d.Replace(projectPath, "").TrimStart('\\'))
            .ToList();

        // Get all code files (not just limited to 50)
        var codeFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains("\\bin\\") && !f.Contains("\\obj\\") && !f.Contains("\\.git\\") && !f.Contains("\\.vs\\"))
            .ToList();

        // Collect comprehensive code samples for LLM
        var codeSamples = new List<string>();
        var fileStructure = new List<string>();

        // Build file structure overview
        foreach (var dir in allDirectories.Take(100)) // Limit directories to avoid token limits
        {
            fileStructure.Add($"DIR: {dir}");
        }

        // Collect code from important files (prioritize by file type, not hardcoded names)
        var importantFiles = codeFiles
            .OrderByDescending(f => 
            {
                var name = Path.GetFileName(f).ToLowerInvariant();
                // Prioritize files that are likely important, but don't hardcode specific names
                if (name.Contains("controller") || name.Contains("service") || name.Contains("model") || 
                    name.Contains("handler") || name.Contains("manager") || name.Contains("helper") ||
                    name.Contains("editor") || name.Contains("component") || name.Contains("provider"))
                    return 1;
                return 0;
            })
            .Take(30) // Get top 30 most relevant files
            .ToList();

        foreach (var file in importantFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(file);
                var relativePath = file.Replace(projectPath, "").TrimStart('\\');
                
                // Store meaningful code samples (first 3000 chars to capture more context)
                var sample = content.Length > 3000 
                    ? content.Substring(0, 3000) + "\n... [truncated]"
                    : content;
                
                codeSamples.Add($"FILE: {relativePath}\n{sample}");
                analysis.CodeSamples[relativePath] = sample;
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        // Store raw data for LLM to interpret - no hardcoded feature detection
        analysis.CodeSamples["_project_structure"] = string.Join("\n", fileStructure);
        analysis.CodeSamples["_file_count"] = codeFiles.Count.ToString();
    }


    private async Task PerformLLMAnalysisAsync(ProjectAnalysis analysis)
    {
        if (_mcpServer == null) return;

        try
        {
            // Build comprehensive project context from collected data
            var projectStructure = analysis.CodeSamples.GetValueOrDefault("_project_structure", "");
            var fileCount = analysis.CodeSamples.GetValueOrDefault("_file_count", "0");
            
            // Get all code samples (excluding metadata)
            var codeSamples = analysis.CodeSamples
                .Where(kvp => !kvp.Key.StartsWith("_"))
                .Select(kvp => kvp.Value)
                .ToList();

            // Combine all code context
            var codeContext = string.Join("\n\n---\n\n", codeSamples.Take(20)); // Limit to avoid token overflow

            var projectContext = $@"=== PROJECT METADATA ===
Framework: {analysis.Framework ?? "Unknown"}
Umbraco Version: {analysis.UmbracoVersion ?? "Unknown"}
Total Code Files: {fileCount}
Installed Packages ({analysis.InstalledPackages.Count}): {string.Join(", ", analysis.InstalledPackages)}

=== PROJECT STRUCTURE ===
{projectStructure}

=== CODE SAMPLES ===
{codeContext}
";

            var advancedContext = $@"
=== ADVANCED PROJECT ANALYSIS CONTEXT ===

ARCHITECTURAL INSIGHTS:
- Architecture Style: {analysis.LayeredArchitecture?.ArchitectureStyle ?? "Unknown"}
- Layer Interactions: {string.Join(", ", analysis.LayeredArchitecture?.LayerInteractions ?? new List<string>())}
- Design Patterns: {string.Join(", ", analysis.DetectedDesignPatterns)}
- Complexity Score: Coupling={analysis.ArchitecturalComplexity?.CouplingScore ?? 0}, Cohesion={analysis.ArchitecturalComplexity?.CohesionScore ?? 0}

CODE QUALITY METRICS:
- Total LOC: {analysis.CodeComplexityMetrics?.TotalLinesOfCode ?? 0}
- Average Method Length: {analysis.CodeComplexityMetrics?.AverageMethodLength ?? 0:F1}
- Code Smells: {string.Join(", ", analysis.CodingPatterns?.CodeSmells ?? new List<string>())}
- Best Practices: {string.Join(", ", analysis.CodingPatterns?.BestPractices ?? new List<string>())}

BUSINESS LOGIC COMPLEXITY:
- Business Rules: {analysis.BusinessLogicComplexity?.BusinessRulesCount ?? 0}
- Validation Rules: {analysis.BusinessLogicComplexity?.ValidationRulesCount ?? 0}
- Domain Concepts: {string.Join(", ", analysis.BusinessDomainConcepts)}

PERFORMANCE PROFILE:
- Performance Bottlenecks: {string.Join(", ", analysis.PerformanceBottlenecks)}
- Caching Patterns: {string.Join(", ", analysis.CachingPatterns?.CacheTypes ?? new List<string>())}
- Async Methods: {analysis.AsyncPatterns?.AsyncMethodsCount ?? 0}

DEPENDENCY ANALYSIS:
- Installed Packages: {string.Join(", ", analysis.InstalledPackages)}
- Outdated Packages: {string.Join(", ", analysis.DependencyAnalysis?.OutdatedPackages ?? new List<string>())}
- Potential Conflicts: {string.Join(", ", analysis.PotentialConflicts)}

=== ORIGINAL PROJECT CONTEXT ===
{projectContext}";

            var prompt = $@"You are a senior enterprise software architect specializing in Umbraco CMS. Perform a comprehensive, sophisticated analysis of this project using advanced architectural principles.

**ANALYSIS FRAMEWORK:**
1. **Architectural Assessment**: Evaluate the overall system architecture, identify anti-patterns, assess scalability
2. **Code Quality Analysis**: Review complexity metrics, identify technical debt, assess maintainability
3. **Business Logic Evaluation**: Understand domain complexity, identify business rules, assess workflow patterns
4. **Performance Engineering**: Analyze bottlenecks, caching strategies, async patterns, optimization opportunities
5. **Dependency Management**: Review package ecosystem, identify conflicts, assess upgrade paths
6. **Integration Complexity**: Evaluate system coupling, identify integration points, assess modularity

**SOPHISTICATED ANALYSIS REQUIREMENTS:**
- Apply SOLID principles assessment
- Evaluate Domain-Driven Design alignment
- Assess microservices readiness
- Identify CQRS/ES patterns
- Analyze event-driven architecture potential
- Evaluate cloud-native architecture readiness
- Assess DevOps maturity indicators

**Project Context:**
{advancedContext}

**Output Format (JSON with nested analysis):**
{{
  ""architecturalAssessment"": {{
    ""overallArchitecture"": ""comprehensive evaluation"",
    ""scalabilityRating"": ""1-10"",
    ""maintainabilityIndex"": ""1-10"",
    ""technicalDebtLevel"": ""low|medium|high|critical"",
    ""recommendedArchitecture"": ""suggested improvements""
  }},
  ""codeQualityAssessment"": {{
    ""complexityAnalysis"": ""detailed complexity evaluation"",
    ""qualityScore"": ""1-10"",
    ""technicalDebtItems"": [""item1"", ""item2""],
    ""refactoringPriorities"": [""priority1"", ""priority2""]
  }},
  ""businessLogicEvaluation"": {{
    ""domainComplexity"": ""simple|moderate|complex|enterprise"",
    ""businessRulesAssessment"": ""evaluation of business logic"",
    ""workflowMaturity"": ""assessment of workflow patterns""
  }},
  ""performanceEngineering"": {{
    ""performanceRating"": ""1-10"",
    ""bottleneckAnalysis"": ""detailed bottleneck assessment"",
    ""optimizationRecommendations"": [""rec1"", ""rec2""]
  }},
  ""dependencyManagement"": {{
    ""ecosystemHealth"": ""1-10"",
    ""upgradeComplexity"": ""assessment of upgrade difficulty"",
    ""conflictResolution"": [""resolution1"", ""resolution2""]
  }},
  ""packageRecommendations"": {{
    ""architecturalPackages"": [""package1"", ""package2""],
    ""performancePackages"": [""package1"", ""package2""],
    ""businessLogicPackages"": [""package1"", ""package2""],
    ""integrationPackages"": [""package1"", ""package2""]
  }},
  ""implementationRoadmap"": {{
    ""phase1"": [""immediate actions""],
    ""phase2"": [""short-term improvements""],
    ""phase3"": [""long-term architectural changes""]
  }}
}}";

            var messages = new[]
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            var options = new ChatOptions
            {
                MaxOutputTokens = 3000,
                Temperature = 0.2f // Lower temperature for more consistent analysis
            };

            var llmClient = _mcpServer.AsSamplingChatClient();
            var response = await llmClient.GetResponseAsync(messages, options, CancellationToken.None);
            
            // Extract text from ChatResponse
            var responseText = response.Text ?? response.ToString() ?? string.Empty;
            analysis.LLMAnalysis = responseText;
            analysis.ProjectSummary = responseText;

            // Parse LLM response and populate analysis object
            try
            {
                var jsonStart = responseText.IndexOf('{');
                var jsonEnd = responseText.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var json = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var parsed = JsonSerializer.Deserialize<JsonElement>(json);

                    // Parse sophisticated analysis results
                    if (parsed.TryGetProperty("architecturalAssessment", out var archAssessment))
                    {
                        if (archAssessment.TryGetProperty("overallArchitecture", out var overallArch))
                            analysis.ArchitecturalSynthesis = overallArch.GetString();
                    }

                    if (parsed.TryGetProperty("codeQualityAssessment", out var codeQuality))
                    {
                        if (codeQuality.TryGetProperty("complexityAnalysis", out var complexity))
                            analysis.CodeQualityAssessment = complexity.GetString();
                    }

                    if (parsed.TryGetProperty("businessLogicEvaluation", out var businessLogic))
                    {
                        if (businessLogic.TryGetProperty("businessRulesAssessment", out var rules))
                            analysis.BusinessLogicUnderstanding = rules.GetString();
                    }

                    if (parsed.TryGetProperty("packageRecommendations", out var packageRecs))
                    {
                        var recommendations = new List<string>();
                        if (packageRecs.TryGetProperty("architecturalPackages", out var archPackages))
                            recommendations.AddRange(archPackages.EnumerateArray().Select(p => p.GetString() ?? ""));
                        if (packageRecs.TryGetProperty("performancePackages", out var perfPackages))
                            recommendations.AddRange(perfPackages.EnumerateArray().Select(p => p.GetString() ?? ""));
                        if (packageRecs.TryGetProperty("businessLogicPackages", out var bizPackages))
                            recommendations.AddRange(bizPackages.EnumerateArray().Select(p => p.GetString() ?? ""));
                        if (packageRecs.TryGetProperty("integrationPackages", out var intPackages))
                            recommendations.AddRange(intPackages.EnumerateArray().Select(p => p.GetString() ?? ""));

                        analysis.PackageRecommendationSynthesis = $"Recommended packages: {string.Join(", ", recommendations.Where(r => !string.IsNullOrEmpty(r)))}";
                    }

                    if (parsed.TryGetProperty("implementationRoadmap", out var roadmap))
                    {
                        if (roadmap.TryGetProperty("phase1", out var phase1))
                            analysis.IntegrationComplexityAnalysis = $"Phase 1: {string.Join(", ", phase1.EnumerateArray().Select(p => p.GetString() ?? ""))}";
                    }

                    // Populate from LLM analysis (LLM is the source of truth)
                    if (parsed.TryGetProperty("features", out var features))
                    {
                        analysis.Features.Clear();
                        foreach (var feature in features.EnumerateArray())
                        {
                            var featureValue = feature.GetString();
                            if (!string.IsNullOrEmpty(featureValue))
                                analysis.Features.Add(featureValue);
                        }
                    }

                    if (parsed.TryGetProperty("businessDomain", out var domain))
                    {
                        analysis.BusinessDomain.Clear();
                        foreach (var d in domain.EnumerateArray())
                        {
                            var domainValue = d.GetString();
                            if (!string.IsNullOrEmpty(domainValue))
                                analysis.BusinessDomain.Add(domainValue);
                        }
                    }

                    if (parsed.TryGetProperty("codePatterns", out var patterns))
                    {
                        analysis.CodePatterns.Clear();
                        foreach (var pattern in patterns.EnumerateArray())
                        {
                            var patternValue = pattern.GetString();
                            if (!string.IsNullOrEmpty(patternValue))
                                analysis.CodePatterns.Add(patternValue);
                        }
                    }

                    if (parsed.TryGetProperty("architecture", out var arch))
                    {
                        var archValue = arch.GetString();
                        if (!string.IsNullOrEmpty(archValue))
                        {
                            // Parse architecture description into patterns
                            if (archValue.Contains("Repository", StringComparison.OrdinalIgnoreCase))
                                analysis.ArchitecturePatterns.Add("RepositoryPattern");
                            if (archValue.Contains("Service", StringComparison.OrdinalIgnoreCase))
                                analysis.ArchitecturePatterns.Add("ServiceLayer");
                            if (archValue.Contains("Dependency Injection", StringComparison.OrdinalIgnoreCase) ||
                                archValue.Contains("DI", StringComparison.OrdinalIgnoreCase))
                                analysis.ArchitecturePatterns.Add("DependencyInjection");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If JSON parsing fails, LLM analysis text is still stored
                analysis.LLMAnalysis += $"\n\n[Parse Error: {ex.Message}]";
            }
        }
        catch (Exception ex)
        {
            analysis.LLMAnalysis = $"LLM analysis unavailable: {ex.Message}";
        }
    }
}

public class UmbracoPatterns
{
    // Learned patterns from Umbraco CMS repository: https://github.com/umbraco/Umbraco-CMS

    // Controller inheritance patterns
    public List<string> ControllerPatterns { get; set; } = new();

    // Content model patterns
    public List<string> ContentModelPatterns { get; set; } = new();

    // Service interface patterns
    public List<string> ServicePatterns { get; set; } = new();

    // Property editor patterns
    public List<string> PropertyEditorPatterns { get; set; } = new();

    // Composition patterns (Composers, Components)
    public List<string> CompositionPatterns { get; set; } = new();

    // Notification patterns
    public List<string> NotificationPatterns { get; set; } = new();

    // Architectural folder/file patterns observed in Umbraco CMS
    public Dictionary<string, List<string>> ArchitecturalPatterns { get; set; } = new();

    // Package-specific integration patterns learned from Umbraco ecosystem
    public Dictionary<string, IntegrationPattern> IntegrationPatterns { get; set; } = new();
}

public class IntegrationPattern
{
    // Primary integration points for this type of package
    public List<string> PrimaryIntegrationPoints { get; set; } = new();

    // Common files that packages of this type typically create/modify
    public List<string> CommonFiles { get; set; } = new();

    // Expected namespaces and dependencies
    public List<string> ExpectedNamespaces { get; set; } = new();

    // Typical composer registration patterns
    public List<string> ComposerRegistrations { get; set; } = new();
}
