# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-13 22:14 UTC_

### Summary

For this executive reporting dashboard, the recommended stack is **C# .NET 8 with Blazor Server** using a single-project solution structure. Blazor Server is ideal here because it renders HTML server-side with SignalR for interactivity, requiring no separate frontend build pipeline. Use the default Blazor Server template (`dotnet new blazorserver`) with **Bootstrap 5** for responsive layout and styling—no additional CSS frameworks needed. Read project data from a local `data.json` file using `System.Text.Json` deserialization into strongly-typed C# models, registered via `IOptions<T>` pattern or a simple singleton service. For the milestone timeline, use pure CSS/HTML (flexbox-based horizontal timeline) rather than a JavaScript charting library to keep things lightweight and screenshot-friendly. No database, authentication, or cloud dependencies are required—just `dotnet run` and open a browser. Target a single Razor page (`Index.razor`) with reusable Razor components for each dashboard section.

### Key Findings

- For this executive reporting dashboard, the recommended stack is **C# .NET 8 with Blazor Server** using a single-project solution structure. Blazor Server is ideal here because it renders HTML server-side with SignalR for interactivity, requiring no separate frontend build pipeline. Use the default Blazor Server template (`dotnet new blazorserver`) with **Bootstrap 5** for responsive layout and styling—no additional CSS frameworks needed. Read project data from a local `data.json` file using `System.Text.Json` deserialization into strongly-typed C# models, registered via `IOptions<T>` pattern or a simple singleton service. For the milestone timeline, use pure CSS/HTML (flexbox-based horizontal timeline) rather than a JavaScript charting library to keep things lightweight and screenshot-friendly. No database, authentication, or cloud dependencies are required—just `dotnet run` and open a browser. Target a single Razor page (`Index.razor`) with reusable Razor components for each dashboard section.

### Detailed Analysis

For this executive reporting dashboard, the recommended stack is **C# .NET 8 with Blazor Server** using a single-project solution structure. Blazor Server is ideal here because it renders HTML server-side with SignalR for interactivity, requiring no separate frontend build pipeline. Use the default Blazor Server template (`dotnet new blazorserver`) with **Bootstrap 5** for responsive layout and styling—no additional CSS frameworks needed. Read project data from a local `data.json` file using `System.Text.Json` deserialization into strongly-typed C# models, registered via `IOptions<T>` pattern or a simple singleton service. For the milestone timeline, use pure CSS/HTML (flexbox-based horizontal timeline) rather than a JavaScript charting library to keep things lightweight and screenshot-friendly. No database, authentication, or cloud dependencies are required—just `dotnet run` and open a browser. Target a single Razor page (`Index.razor`) with reusable Razor components for each dashboard section.
