# PM Specification: My Project

## Executive Summary

My Project is a lightweight, single-page executive reporting dashboard built on Blazor Server (.NET 8) designed to provide visibility into project milestones, task progress, and shipping status. The application reads project data from a JSON configuration file and renders a clean, screenshot-optimized view suitable for executive presentations and PowerPoint decks, requiring zero end-user infrastructure (self-contained .exe, no .NET pre-install needed).

## Business Goals

1. Enable executives to view complete project progress at a glance without manual report creation
2. Provide pixel-perfect, screenshot-ready visuals that executives can capture and insert directly into PowerPoint decks
3. Reduce project reporting friction by eliminating manual slide creation and data entry
4. Create a simple, maintainable reporting mechanism using a JSON data file as the single source of truth
5. Deploy as a self-contained, locally-executed Windows application with zero infrastructure dependencies

## User Stories & Acceptance Criteria

**US-001: Executive Views Project Milestone Timeline**
- **As a** project executive, **I want** to see a chronological timeline of project milestones at the top of the dashboard, **so that** I can quickly understand the project schedule and major deliverables.
- **Acceptance Criteria:**
  - [ ] Timeline displays 5-10 milestones in chronological order with dates and milestone names
  - [ ] Design visually matches OriginalDesignConcept.html timeline section
  - [ ] Timeline is implemented as custom HTML/CSS (no external charting library required)
  - [ ] Milestones are read from data.json and rendered dynamically

**US-002: Executive Views Task Status Breakdown**
- **As a** project executive, **I want** to see tasks categorized as Shipped, In Progress, and Carried Over from previous months, **so that** I understand what has been delivered, what is actively being worked on, and what remains pending.
- **Acceptance Criteria:**
  - [ ] Dashboard displays three distinct sections (Shipped, In Progress, Carried Over)
  - [ ] Each section shows task count and a visual representation (cards or list)
  - [ ] Layout matches OriginalDesignConcept.html design template
  - [ ] Task data is loaded from data.json and reflects sample fictional project

**US-003: Executive Sees Project Progress Metrics**
- **As a** project executive, **I want** to see a visual bar chart showing progress across task categories and project metadata (start date, target completion, project name), **so that** I can assess project health at a glance.
- **Acceptance Criteria:**
  - [ ] Bar chart renders shipped/in-progress/carried-over task counts using ChartJs Blazor
  - [ ] Chart displays cleanly and is suitable for screenshot export
  - [ ] Metrics panel shows project name, dates, and key statistics
  - [ ] Chart and metrics are populated from data.json

**US-004: Executive Captures Dashboard for PowerPoint**
- **As a** a project executive, **I want** to take a screenshot or PDF export of the dashboard, **so that** I can include the visual in my PowerPoint presentation to executives.
- **Acceptance Criteria:**
  - [ ] Dashboard uses fixed 1200px layout width for consistent rendering across devices
  - [ ] Print preview (F12 > Print > Save as PDF) renders pixel-perfect output
  - [ ] Dashboard output matches OriginalDesignConcept.html visual design with 95%+ accuracy
  - [ ] No layout shifts, overlapping elements, or responsive breakpoints occur when exporting
  - [ ] CSS media queries ensure clean print/screenshot output

**US-005: Operator Updates Project Data**
- **As a** project operator, **I want** to update milestone dates, task counts, and project metadata by editing data.json and restarting the application, **so that** the dashboard reflects the latest project status.
- **Acceptance Criteria:**
  - [ ] Application loads data.json at startup without errors
  - [ ] JSON schema is documented with sample data provided
  - [ ] Dashboard reflects all changes in data.json after application restart
  - [ ] Malformed JSON displays a user-friendly error message (not a crash)
  - [ ] Sample data.json includes fictional project with realistic data

**US-006: Application Launches Without External Dependencies**
- **As a** project executive, **I want** to run the dashboard by double-clicking a single .exe file, **so that** I don't need to install .NET, configure IIS, or manage infrastructure.
- **Acceptance Criteria:**
  - [ ] Self-contained .exe is generated via `dotnet publish -r win-x64 --self-contained`
  - [ ] .exe size is between 110-140MB
  - [ ] Application runs on Windows 10/11 machines without .NET pre-installed
  - [ ] Browser opens automatically to http://localhost:5000 when .exe is launched
  - [ ] Application shuts down cleanly when browser tab is closed or Ctrl+C is pressed

## Scope

### In Scope

- Single-page Blazor Server application (.NET 8.0.x)
- Static JSON data loading from data.json file at application startup
- Custom HTML/CSS timeline visualization for milestones (no charting library)
- ChartJs Blazor 4.1.2 integration for bar charts (shipped/in-progress/carried-over task counts)
- Print/screenshot optimization via CSS media queries and fixed-width layout (1200px)
- Self-contained Windows .exe deployment (win-x64 architecture)
- Sample fictional project data in data.json
- Basic console-level application logging via Microsoft.Extensions.Logging
- Unit tests for JSON deserialization (ProjectData model validation)
- User documentation explaining how to run .exe and export to PowerPoint
- Kestrel web server (built-in, no IIS required)
- System.Text.Json for JSON deserialization

### Out of Scope

- Multi-project dashboard support (single project per .exe instance)
- Real-time data updates, data polling, or FileSystemWatcher auto-reload
- SignalR push notifications or live dashboard updates
- User authentication, authorization, or role-based access control
- Multi-user collaboration features or concurrent editing
- Relational database (SQL Server, SQLite, PostgreSQL, etc.) or Entity Framework Core
- REST API endpoints or web service integrations
- Cloud deployment (Azure App Service, AWS EC2, Google Cloud, etc.)
- Windows Installer (.msi) generation or Wix Toolset integration
- Code-signing certificate for .exe (Windows Defender SmartScreen warnings acceptable for internal distribution)
- Mobile responsiveness or responsive design (desktop-only, fixed width)
- Historical trend analysis, month-over-month comparisons, or data archiving
- Automated PowerPoint (.pptx) generation or screenshot APIs
- Docker containerization or container registry (ACR) push
- Bootstrap or Tailwind CSS framework (custom CSS only)
- Entity Framework, ORM frameworks, or database migrations
- Newtonsoft.Json / Json.NET (System.Text.Json only)
- Advanced state management (MVVM patterns, Redux, etc.) beyond component parameters

## Non-Functional Requirements

| Requirement | Target | Rationale |
|---|---|---|
| **Page Load Time** | <2 seconds | Executives expect responsive UX; metrics loading must not create impression of slowness |
| **Component Render Time** | <10ms (measured in browser DevTools) | Smooth interactive experience; no jank or visual delays |
| **Executable Size** | 110-140MB | Self-contained .NET 8 runtime + application; reasonable for email or shared drive distribution |
| **Cold Start Time** | <5 seconds | Kestrel server initialization + browser auto-launch must complete within acceptable waiting window |
| **Browser Compatibility** | Chrome 100+, Edge 100+, Firefox 100+ | Executives use standard corporate browser installations |
| **Print/Screenshot Parity** | 95%+ visual match to design template | Dashboard export to PowerPoint must not require design cleanup or manual adjustments |
| **Concurrent Local Users** | 5-20 simultaneous connections on localhost | Single machine (Kestrel default configuration); no multi-machine cluster required |
| **Data Refresh Frequency** | Manual (application restart) | No polling, FileSystemWatcher, or SignalR required; static data model sufficient |
| **Security Model** | Internal tool (Windows file-level ACL) | No encryption, TLS, or authentication; Windows NTFS permissions control data.json access |
| **Code Size** | <500 total lines of code | Single-project structure; minimal cognitive overhead for maintenance and future enhancements |
| **JSON Data File Size** | <1MB | System.Text.Json deserializes <1MB files in <50ms; no performance bottleneck |
| **Windows Versions** | Windows 10 (v1809+) and Windows 11 | Standard corporate/developer desktop environments; no Windows 7/8 support required |

## Success Metrics

| Metric | Target | Evidence |
|---|---|---|
| **All user stories completed** | 6/6 stories pass acceptance criteria | Each story's checklist items verified and demonstrated |
| **Visual fidelity** | 95%+ match between dashboard screenshot and OriginalDesignConcept.html | Side-by-side comparison of desktop, print preview, and PDF export |
| **Deployment success** | .exe runs on clean Windows 10/11 without .NET pre-install | Tested on machine with .NET runtime removed; browser opens automatically |
| **Performance targets met** | Page load <2s, render <10ms, cold start <5s | Browser DevTools metrics + stopwatch validation |
| **Code quality** | <500 LOC, single project, unit tests pass | Line count audit + test suite execution |
| **Print/screenshot quality** | PowerPoint insertion workflow completes in <5 minutes without manual design adjustments | User workflow: open dashboard → F12 → Print → Save PDF → Insert into PowerPoint |
| **JSON validation** | Malformed data.json displays error (no crash); valid data.json loads correctly | Test with sample valid and invalid JSON files |
| **Documentation** | User guide covers "How to Run Dashboard" and "How to Export to PowerPoint" | Deliverable checklist: guide.pdf exists and is clear to non-technical executive |

## Constraints & Assumptions

### Technical Constraints

- **Windows-only deployment:** Application must be compiled and delivered as win-x64 self-contained .exe; no Linux, macOS, or web-hosted variants in MVP
- **Local-only execution:** No multi-machine sync, cloud backend, or network services; data is read from local disk only
- **Static data model:** No real-time updates, database connections, or external data sources; data.json is the single source of truth
- **Single-project scope:** One dashboard instance per .exe; no multi-project selector or project switcher UI
- **Blazor Server architecture:** Server-side rendering only; no WebAssembly (WASM) or client-side rendering in MVP
- **Fixed layout design:** 1200px max-width; no responsive breakpoints or mobile optimization
- **No framework CSS:** Custom CSS only (~200 lines); no Bootstrap, Tailwind, or Material Design framework
- **JSON serialization:** System.Text.Json only; no Newtonsoft.Json or third-party serializers

### Timeline Assumptions

- **Development duration:** 3-4 weeks (Phase 1: component structure + data loading; Phase 2: visualization + styling; Phase 3: deployment + testing)
- **Team expertise:** C# and .NET 8 knowledge required; Blazor learning curve ~1-2 days for experienced .NET developers
- **OriginalDesignConcept.html availability:** Design artifact must be provided and accessible before Phase 1 begins
- **No design iterations:** MVP assumes single-pass design translation from OriginalDesignConcept.html; no iterative refinement or A/B testing

### Data & Environment Assumptions

- **Executives have standard Windows machines:** Windows 10 (v1809+) or Windows 11 with Chrome, Edge, or Firefox installed
- **Project data is static:** Milestones, task counts, and metadata do not change during user viewing; manual data.json edit + restart is acceptable
- **Concurrent user load is low:** 5-20 executives accessing dashboard simultaneously; no enterprise scale-out required
- **No sensitive PII in sample data:** Dashboard contains non-confidential project metadata (milestone names, task counts, status); no passwords, API keys, or personal information
- **Windows file permissions are sufficient:** data.json access controlled via NTFS ACL; no encryption at rest or additional secrets management needed
- **.NET 8 SDK available on developer machine:** Team can build, test, and publish application locally; no Docker or CI/CD pipeline required for MVP

### Dependency Assumptions

- **ChartJs Blazor 4.1.2 stability:** NuGet package remains compatible with Blazor Server in .NET 8.0.x; no breaking changes expected during development
- **Kestrel production-readiness:** Built-in Kestrel server (included in Blazor Server template) is suitable for local execution; no IIS, nginx, or reverse proxy needed
- **System.Text.Json capability:** Built-in JSON serializer meets all MVP deserialization requirements; no schema validation or advanced transformation needed
- **Browser auto-launch:** Process.Start("http://localhost:5000") works on Windows 10/11 with default browser settings; no special configuration required
- **No external APIs:** Dashboard does not call external services, REST endpoints, or cloud APIs; all data originates from data.json