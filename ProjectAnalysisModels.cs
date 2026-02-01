namespace UmbracoPackageSuggest.Services;

public class ProjectAnalysis
{
    public string? Framework { get; set; }
    public string? UmbracoVersion { get; set; }
    public List<string> InstalledPackages { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public string? ProjectPath { get; set; }
    public string? ProjectSummary { get; set; }
    public List<string> CodePatterns { get; set; } = new();
    public List<string> ArchitecturePatterns { get; set; } = new();
    public List<string> BusinessDomain { get; set; } = new();
    public Dictionary<string, string> CodeSamples { get; set; } = new();
    public string? LLMAnalysis { get; set; }

    public SolutionStructure? SolutionStructure { get; set; }
    public LayeredArchitecture? LayeredArchitecture { get; set; }
    public List<string> DetectedDesignPatterns { get; set; } = new();
    public ProjectOrganization? ProjectOrganization { get; set; }
    public ArchitecturalComplexity? ArchitecturalComplexity { get; set; }
    public string? ArchitectureSummary { get; set; }

    public CodeComplexityMetrics? CodeComplexityMetrics { get; set; }
    public CodingPatterns? CodingPatterns { get; set; }
    public ErrorHandlingPatterns? ErrorHandlingPatterns { get; set; }
    public DataAccessPatterns? DataAccessPatterns { get; set; }
    public ApiDesignPatterns? ApiDesignPatterns { get; set; }
    public string? CodeAnalysisSummary { get; set; }

    public List<string> BusinessDomainConcepts { get; set; } = new();
    public BusinessLogicComplexity? BusinessLogicComplexity { get; set; }
    public List<string> BusinessRules { get; set; } = new();
    public WorkflowPatterns? WorkflowPatterns { get; set; }
    public string? BusinessAnalysisSummary { get; set; }

    public List<string> PerformanceBottlenecks { get; set; } = new();
    public CachingPatterns? CachingPatterns { get; set; }
    public AsyncPatterns? AsyncPatterns { get; set; }
    public List<string> PerformanceOptimizationOpportunities { get; set; } = new();
    public string? PerformanceAnalysisSummary { get; set; }

    public DependencyAnalysis? DependencyAnalysis { get; set; }
    public InternalDependencies? InternalDependencies { get; set; }
    public List<string> PotentialConflicts { get; set; } = new();
    public CompatibilityRequirements? CompatibilityRequirements { get; set; }
    public string? DependencyAnalysisSummary { get; set; }

    public string? ArchitecturalSynthesis { get; set; }
    public string? CodeQualityAssessment { get; set; }
    public string? BusinessLogicUnderstanding { get; set; }
    public string? PackageRecommendationSynthesis { get; set; }
    public string? IntegrationComplexityAnalysis { get; set; }
}

public class SolutionStructure
{
    public int ProjectCount { get; set; }
    public List<string> ProjectTypes { get; set; } = new();
    public bool HasTests { get; set; }
    public bool HasDocumentation { get; set; }
    public List<string> SolutionFolders { get; set; } = new();
}

public class LayeredArchitecture
{
    public bool HasPresentationLayer { get; set; }
    public bool HasBusinessLayer { get; set; }
    public bool HasDataLayer { get; set; }
    public bool HasServiceLayer { get; set; }
    public List<string> LayerInteractions { get; set; } = new();
    public string ArchitectureStyle { get; set; } = "Unknown";
}

public class ProjectOrganization
{
    public Dictionary<string, int> FilesByExtension { get; set; } = new();
    public Dictionary<string, List<string>> FilesByDirectory { get; set; } = new();
    public List<string> LargeFiles { get; set; } = new();
    public bool FollowsConventions { get; set; }
}

public class ArchitecturalComplexity
{
    public int CyclomaticComplexity { get; set; }
    public int CouplingScore { get; set; }
    public int CohesionScore { get; set; }
    public List<string> ComplexityHotspots { get; set; } = new();
}

public class CodeComplexityMetrics
{
    public double AverageMethodLength { get; set; }
    public double AverageClassLength { get; set; }
    public int TotalLinesOfCode { get; set; }
    public Dictionary<string, int> ComplexityByFile { get; set; } = new();
}

public class CodingPatterns
{
    public List<string> NamingConventions { get; set; } = new();
    public List<string> CodeSmells { get; set; } = new();
    public List<string> BestPractices { get; set; } = new();
    public Dictionary<string, int> PatternFrequency { get; set; } = new();
}

public class ErrorHandlingPatterns
{
    public List<string> ExceptionTypes { get; set; } = new();
    public bool UsesTryCatch { get; set; }
    public bool UsesCustomExceptions { get; set; }
    public List<string> ErrorHandlingStrategies { get; set; } = new();
}

public class DataAccessPatterns
{
    public List<string> OrmPatterns { get; set; } = new();
    public List<string> QueryPatterns { get; set; } = new();
    public bool UsesRepositoryPattern { get; set; }
    public bool UsesUnitOfWork { get; set; }
}

public class ApiDesignPatterns
{
    public List<string> HttpMethods { get; set; } = new();
    public List<string> ResponsePatterns { get; set; } = new();
    public bool UsesRestPrinciples { get; set; }
    public bool UsesVersioning { get; set; }
}

public class BusinessLogicComplexity
{
    public int BusinessRulesCount { get; set; }
    public int ValidationRulesCount { get; set; }
    public List<string> ComplexBusinessMethods { get; set; } = new();
}

public class WorkflowPatterns
{
    public List<string> StateMachines { get; set; } = new();
    public List<string> SagaPatterns { get; set; } = new();
    public List<string> EventDrivenPatterns { get; set; } = new();
}

public class CachingPatterns
{
    public List<string> CacheTypes { get; set; } = new();
    public List<string> CacheStrategies { get; set; } = new();
    public bool UsesDistributedCache { get; set; }
    public bool UsesOutputCache { get; set; }
}

public class AsyncPatterns
{
    public int AsyncMethodsCount { get; set; }
    public List<string> AsyncPatternsUsed { get; set; } = new();
    public bool UsesTaskWhenAll { get; set; }
    public bool UsesConfigureAwait { get; set; }
}

public class DependencyAnalysis
{
    public Dictionary<string, string> PackageVersions { get; set; } = new();
    public List<string> OutdatedPackages { get; set; } = new();
    public List<string> VulnerablePackages { get; set; } = new();
    public Dictionary<string, List<string>> PackageDependencies { get; set; } = new();
}

public class InternalDependencies
{
    public Dictionary<string, List<string>> ClassDependencies { get; set; } = new();
    public List<string> CircularDependencies { get; set; } = new();
    public Dictionary<string, int> CouplingMetrics { get; set; } = new();
}

public class CompatibilityRequirements
{
    public string MinimumFrameworkVersion { get; set; } = "";
    public List<string> RequiredPackages { get; set; } = new();
    public List<string> ConflictingPackages { get; set; } = new();
    public Dictionary<string, string> VersionConstraints { get; set; } = new();
}