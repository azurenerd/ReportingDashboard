# Squad Decisions

## Active Decisions

### 2026-04-22: Implementation complete — SinglePRMode delivery
**By:** Copilot (implementation agent)
**What:** Delivered the full Executive Reporting Dashboard as a single cohesive Blazor Server implementation targeting all 7 user stories (US-1 through US-8).
**Key decisions:**
- **Target framework:** Kept `net10.0` from existing scaffold (runtime available, APIs like `MapStaticAssets` and `@Assets` require .NET 9+)
- **Heatmap data model:** Kept `List<List<string>>` for Items (simpler than spec's `Dictionary<string, List<string>>`, consistent with JSON)
- **Timeline math:** Uses `day/30` fraction for pixel-perfect alignment with reference HTML (equal-width month buckets)
- **Concrete DI:** Used `DashboardDataService` directly without interface (simpler for single-service app)
- **FileSystemWatcher:** Added `Renamed` handler for editor compatibility; polling fallback at 5s intervals

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
