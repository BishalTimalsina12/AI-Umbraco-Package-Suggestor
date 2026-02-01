# UmbracoPackageSuggest MCP Server

An intelligent MCP (Model Context Protocol) server that provides AI-powered package suggestions and analysis for Umbraco CMS projects. This tool analyzes your .NET/Umbraco codebase and recommends relevant NuGet packages and Umbraco marketplace extensions based on your project's architecture, business domain, and code patterns.

## Overview

UmbracoPackageSuggest uses advanced AI analysis to understand your Umbraco project's structure, business logic, and architectural patterns. It then provides contextual package recommendations that can enhance your site's functionality, performance, and developer experience.

## Key Features

- **Intelligent Project Analysis**: Deep-dive analysis of your Umbraco codebase including architecture patterns, code complexity, and business logic
- **AI-Powered Recommendations**: LLM-driven package suggestions based on your project's specific needs
- **Marketplace Intelligence**: Comprehensive search across NuGet and Umbraco Marketplace
- **Integration Guidance**: Context-aware integration hints and implementation steps
- **Performance Simulation**: "What-if" scenarios showing potential impact of adding packages
- **Enterprise Insights**: Advanced architectural assessment and strategic recommendations


### Package Analysis Tools

#### `SuggestPackages`
Analyzes a .NET/Umbraco project and suggests relevant packages with detailed reasoning.
```json
{
  "projectPath": "/path/to/your/project",
  "depth": "detailed"
}
```
- `depth`: "simple" for basic suggestions, "detailed" for comprehensive analysis

#### `GenerateMarketplaceMap`
Creates an AI-driven marketplace map visualization, ranking packages by relevance, popularity, compatibility, and community score.
```json
{
  "projectPath": "/path/to/your/project"
}
```

#### `PerformDeepProjectAnalysis`
Performs sophisticated deep-dive analysis of your Umbraco project, providing enterprise-level insights on architecture, code quality, performance, and strategic recommendations.
```json
{
  "projectPath": "/path/to/your/project"
}
```

### Package Integration Tools

#### `SimulatePackageImpact`
Simulates "what-if" scenarios showing how your site would perform if certain packages are added.
```json
{
  "projectPath": "/path/to/your/project",
  "packageIds": ["Umbraco.Forms", "Umbraco.Commerce"]
}
```

#### `GetCodeIntegrationHints`
Provides intelligent, context-aware code integration hints based on your actual Umbraco project structure.
```json
{
  "projectPath": "/path/to/your/project",
  "packageIds": ["Umbraco.Forms"]
}
```

## Installation

1. **Prerequisites**
   - .NET 10.0 or later
   - Access to NuGet and Umbraco Marketplace APIs

2. **Build the project**
   ```bash
   dotnet build
   ```

3. **Run the MCP server**
   ```bash
   dotnet run
   ```

## Usage with MCP Clients

This server implements the Model Context Protocol and can be used with any MCP-compatible client. The server communicates via stdio and automatically discovers available tools.


```

## Response Formats

### Package Suggestions Response
```json
{
  "DeepProjectAnalysis": {
    "ProjectOverview": {
      "Framework": ".NET 8.0",
      "UmbracoVersion": "13.0.0",
      "TotalCodeFiles": 45,
      "TotalLinesOfCode": 12500,
      "DetectedFeatures": ["Content Management", "E-commerce"],
      "BusinessDomain": ["Retail", "Content Publishing"],
      "ArchitecturePatterns": ["Layered Architecture", "Repository Pattern"]
    },
    "ArchitecturalAssessment": {
      "ArchitectureStyle": "Layered",
      "LayerCompleteness": {
        "HasPresentationLayer": true,
        "HasBusinessLayer": true,
        "HasDataLayer": true,
        "HasServiceLayer": false
      }
    }
  },
  "PackageRecommendations": [
    {
      "Package": {
        "PackageId": "Umbraco.Forms",
        "PackageName": "Umbraco Forms",
        "Description": "Create forms in Umbraco",
        "RelevanceScore": 0.92,
        "CommunityScore": 0.85
      },
      "WhyRecommended": {
        "Reason": "Based on your contact forms and data collection needs",
        "BusinessValue": "Streamlines lead generation and user engagement"
      }
    }
  ]
}
```

## Architecture

The server consists of several key components:

- **McpServerSetup.cs**: Configures the MCP server with dependency injection
- **PackageSearchTools.cs**: Handles NuGet and marketplace searches
- **PackageAnalysisTools.cs**: Provides AI-powered analysis and recommendations
- **PackageIntegrationTools.cs**: Offers integration guidance and impact simulation
- **ProjectAnalyzer.cs**: Analyzes .NET/Umbraco project structure
- **RecommendationEngine.cs**: Generates intelligent package recommendations
- **NuGetApiClient.cs**: Interfaces with NuGet API
- **UmbracoMarketplaceClient.cs**: Interfaces with Umbraco Marketplace API

## Dependencies

- ModelContextProtocol 0.6.0-preview.1
- Microsoft.Extensions.Hosting 8.0.0
- Microsoft.Extensions.Http 8.0.0
- System.Text.Json 10.0.1
- Microsoft.Extensions.AI (for LLM integration)

## Target Framework

- .NET 10.0

## Development

To extend the server with new tools:

1. Create a new static class with `[McpServerToolType]` attribute
2. Add static methods with `[McpServerTool]` attribute and `[Description]` for parameters
3. Register any required services in `McpServerSetup.ConfigureServices()`


