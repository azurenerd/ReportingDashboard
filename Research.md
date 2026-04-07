# Research

_No research has been documented yet._

## Research technology stack for My Project

_Researched on 2026-04-07 18:18 UTC_

### Summary



### Detailed Analysis

Let me research the specific libraries and their current state to provide accurate version numbers and recommendations.

The detailed **Research.md** is written to the session files folder with in-depth analysis for all 8 sub-questions. Here's the executive summary:

| Sub-Question | Recommendation | Key Rationale |
|---|---|---|
| **1. UI Library** | MudBlazor 9.2.0 | 16k+ GitHub stars, `MudTimeline` built-in, Material Design polish, MIT license |
| **2. Charts** | MudBlazor built-in (Phase 1) → Blazor-ApexCharts 6.1.0 (Phase 2) | Start simple, upgrade when animation/tooltips needed |
| **3. Data Loading** | `IOptionsMonitor<T>` + `reloadOnChange: true` | Idiomatic .NET, auto-reload, validated at startup |
| **4. Architecture** | Direct injection + CascadingValue | Read-only page — CQRS/Fluxor/repositories are all overkill |
| **5. Timeline** | `MudTimeline` horizontal orientation | Uniform spacing is better for exec scanning; proportional is overengineered |
| **6. PDF Export** | CSS `@media print` (Phase 1) → QuestPDF 2026.2.4 (Phase 2) | Zero-cost MVP, pixel-perfect upgrade path |
| **7. Styling** | MudBlazor theme + CSS isolation + 4-color status system | Green/Blue/Orange/Red maps to Complete/Active/Warning/Blocked |
| **8. Testing** | xUnit 2.9.3 + bUnit 2.7.2 | Industry standard, strong Blazor support |

**Total Phase 1 production dependencies: 1 package (MudBlazor). Total cost: $0.** Engineers can start building immediately from the component hierarchy and code samples in the document.
