# Architecture

## Overview & Goals

This architecture describes the delivery of a single static artifact — `status.html` — into the repository root of an existing C# .NET 8 / Blazor Server solution. The design intentionally treats the deliverable as a **repository-level static sibling artifact** that coexists with, but is fully decoupled from, the Blazor Server application. No runtime code paths, middleware, routing, DI registrations, build steps, or deployment pipelines are added or modified.

**Primary goals:**

1. Deliver a valid HTML5 document (`status.html`) at the repository root containing exactly one `<h1>Project Status</h1>` and one `<p>All systems nominal.</p>`.
2. Preserve the existing Blazor Server app (`.sln`, `.csproj`, `Program.cs`, `wwwroot/`, `appsettings.json`) byte-for-byte unchanged.
3. Enforce strict scope discipline: one added file, zero modified files, zero deleted files, zero new tests.
4. Ensure zero runtime/operational footprint: the file is never compiled, routed, served by Kestrel, or referenced by Razor components or middleware.
5. Meet WCAG 2.1 AA via semantic HTML alone (`<h1>`, `<p>`, `lang="en"`) and UTF-8 encoding via `<meta charset="UTF-8">`.
6. Keep the file under 1 KB and render in under 50 ms on any modern browser.

**Non-goals (explicit):** No CSS, no JavaScript, no data sources, no Blazor integration, no alignment with `OriginalDesignConcept.html`, no navigation, no tests, no CI changes.

**Architectural style:** "Static sibling artifact." The file is a Git-tracked text blob with no runtime binding to the solution.

---

## System Components

The system contains exactly **one new logical component** and several **existing components that must remain untouched**. The architecture is defined primarily by what is *excluded* from it.

### Component 1: `status.html` (new, in scope)

- **Type:** Static HTML5 document (plain text file, UTF-8 encoded).
- **Location:** Repository root — `./status.html` (sibling of `.sln`, `AgentSquad.Runner/`, `OriginalDesignConcept.html`, etc.). **Not** under `src/`, **not** under any project folder, **not** under any `wwwroot/`.
- **Responsibilities:**
  - Present a human-readable project status when opened in a web browser or previewed via the Git web UI.
  - Declare document language (`lang="en"`) and character encoding (`<meta charset="UTF-8">`) for correct rendering and screen-reader announcement.
  - Supply a browser tab title (`<title>Project Status</title>`).
  - Render exactly two visible elements: one `<h1>` and one `<p>`.
- **Interfaces:**
  - **Input interface:** None. The file consumes no input at rest or at render time.
  - **Output interface:** DOM produced by the browser from the static byte stream. No JS APIs, no postMessage, no forms.
  - **Network interface:** None. Zero outbound requests (no fonts, stylesheets, images, scripts, analytics, favicons).
- **Dependencies:**
  - **Build-time:** None.
  - **Runtime:** A user-agent (any modern browser) capable of parsing HTML5 and UTF-8. No .NET runtime, no Kestrel, no SignalR, no JS engine dependency.
  - **Repository:** None — the file is standalone text.
- **Data:** One static, hard-coded string literal (`All systems nominal.`) and one static heading literal (`Project Status`). No reads, no writes, no persistence.
- **Lifecycle:** Created once by a single PR. Edited only by future tickets that are explicitly out of scope today.

### Component 2: Existing Blazor Server solution (unchanged, out of scope for modification)

- **Type:** C# .NET 8 Blazor Server application under `.sln`.
- **Role in this architecture:** **Passive neighbor.** Must compile, start, and behave identically before and after this change.
- **Interfaces with `status.html`:** **None.** No routing, no static file middleware reference, no Razor component import, no `@page` directive, no `_Host.cshtml` mention, no `App.razor` link, no `appsettings.json` entry, no MSBuild content include.
- **Dependencies:** Unchanged — .NET SDK 8.0.x, ASP.NET Core 8, existing NuGet references.
- **Data:** Unchanged — existing `agentsquad_azurenerd_ReportingDashboard.db`, `sme-definitions.json`, `report.*.json`, `appsettings.json`, `appsettings.Development.json` remain untouched.

### Component 3: `OriginalDesignConcept.html` (existing, unrelated, untouched)

- **Type:** Pre-existing HTML design reference for a different workstream (Privacy Automation Release Roadmap dashboard).
- **Role in this architecture:** **Explicitly excluded reference.** It is neither a dependency of, nor a visual template for, `status.html`. No CSS, fonts (Segoe UI), color palette (`#0078D4`, `#34A853`, `#F4B400`, `#EA4335`, etc.), grid layout, flex layout, SVG timeline, or heatmap structure from this file may be copied, imported, linked, or imitated.
- **Interfaces with `status.html`:** **None, by design.**

### Component 4: Git repository (existing, receives one net-new file)

- **Responsibility:** Version and host the new `status.html` alongside existing files.
- **Interface:** Standard Git/GitHub PR workflow.
- **Change:** Exactly one file added (`status.html`); zero files modified; zero files deleted.

---

## Component Interactions

By design, there are **no runtime interactions** between `status.html` and any other component in the repository. The only interactions are human/tooling-level, and they occur at rest or on user action outside the Blazor Server runtime.

### Interaction diagram (logical)

```
+------------------+      git commit / PR       +------------------------+
|  Author (human)  | -------------------------> |  Git repository (root) |
+------------------+                            |  status.html (new)     |
                                                |  .sln, src/, ... (unchanged)
                                                |  OriginalDesignConcept.html (unchanged)
                                                +-----------+------------+
                                                            |
                                                            | (static file at rest;
                                                            |  no pipeline, no deploy)
                                                            v
                              +-----------------------------+------------------------------+
                              |                                                            |
                              v                                                            v
              +----------------------------+                              +-----------------------------+
              | Reviewer (human)           |                              | Reader (human)              |
              | - opens PR diff            |                              | - double-clicks status.html |
              | - verifies exactly 1 added |                              |   OR views via GitHub       |
              |   file                     |                              | - browser parses HTML5      |
              | - greps for disallowed     |                              | - renders h1 + p with UA    |
              |   tokens                   |                              |   default styles            |
              +----------------------------+                              | - issues ZERO network req.  |
                                                                          +-----------------------------+

              +------------------------------------------------------------------------------------+
              | Blazor Server app (unchanged): no interaction with status.html at any lifecycle    |
              | phase — build, startup, request handling, shutdown. `dotnet build` output          |
              | is bit-identical w.r.t. this change.                                               |
              +------------------------------------------------------------------------------------+
```

### Data flow

- **At rest:** `status.html` is a ~300-byte UTF-8 text blob tracked by Git. No data flows into or out of it.
- **At render:** The browser reads the local file (or raw GitHub bytes), parses HTML5, constructs DOM, applies user-agent stylesheet, paints. No fetches are issued.
- **At build:** `dotnet build` ignores `status.html` entirely (it is not part of any `.csproj` `ItemGroup` and is not under any project directory).

### Communication patterns

- **Synchronous, in-process:** Browser-local HTML parse → paint.
- **Asynchronous / RPC / SignalR / HTTP:** **None.** The file does not participate in any Blazor Server circuit.
- **Inter-component:** None. `status.html` and the Blazor Server app are fully isolated.

---

## Data Model

There is **no data model**. The deliverable contains no entities, no relationships, no persistence, no schema, no migrations.

### Entities

- **None.** No classes, no records, no DTOs, no EF Core models introduced.

### Storage

- **Primary storage:** Git object store (one new blob tracked in the working tree at `./status.html`).
- **Runtime storage:** None. No database, no file I/O, no cache, no cookies, no `localStorage`, no `sessionStorage`, no IndexedDB.
- **Existing storage untouched:** `agentsquad_azurenerd_ReportingDashboard.db` (SQLite file already in `AgentSquad.Runner/`) is not read, written, referenced, or migrated by this change.

### Content manifest (authoritative byte content of `status.html`)

The file MUST contain exactly the following logical content (whitespace may vary only in ways that do not introduce extra elements or text nodes visible to the user):

```html
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>Project Status</title>
</head>
<body>
  <h1>Project Status</h1>
  <p>All systems nominal.</p>
</body>
</html>
```

**Logical element tree:**

| Node | Attribute(s) | Text content |
|---|---|---|
| `<!DOCTYPE html>` | — | — |
| `<html>` | `lang="en"` | — |
| `<head>` | — | — |
| `<meta>` (child of head) | `charset="UTF-8"` | — |
| `<title>` (child of head) | — | `Project Status` |
| `<body>` | — | — |
| `<h1>` (child of body) | — | `Project Status` |
| `<p>` (child of body) | — | `All systems nominal.` |

**Cardinality constraints:** exactly one `<h1>`, exactly one `<p>`, exactly one `<title>`, exactly one `<meta charset>`. No other elements permitted in `<body>` or `<head>`.

---

## API Contracts

There are **no APIs**. This section documents that explicitly so no engineer introduces one.

### HTTP endpoints

- **None.** `status.html` is not served by Kestrel, not registered with `UseStaticFiles`, not mapped in `MapBlazorHub` or `MapFallbackToPage`, and not referenced by any controller or minimal API.

### Blazor / SignalR contracts

- **None.** No Razor component consumes, imports, or links to `status.html`. No SignalR hub method references it.

### File contract (the only "contract" that exists)

The file is itself the contract. The acceptance contract is:

- **Path contract:** `./status.html` (exact, case-sensitive, repo root).
- **Structural contract:** Well-formed HTML5, doctype present, `<html lang="en">`, `<meta charset="UTF-8">`, `<title>Project Status</title>`, `<h1>Project Status</h1>`, `<p>All systems nominal.</p>`.
- **Prohibition contract:** The file MUST NOT contain any of the following substrings (case-insensitive): `<style`, `<script`, `stylesheet`, `data.json`, `http://`, `https://`, `src=`, `href=`, `onclick`, `onload`, `fetch(`, `XMLHttpRequest`.
- **Size contract:** Serialized file size ≤ 1024 bytes.
- **Encoding contract:** UTF-8, no BOM preferred (BOM tolerated).

### Error handling

- **Not applicable.** The file performs no I/O and has no failure modes. A missing file yields whatever 404/listing behavior the host filesystem or Git web UI provides — this is explicitly outside the architecture boundary.

---

## Infrastructure Requirements

### Hosting

- **None.** The file is not hosted by any server. It is viewed either (a) directly from the local filesystem via `file://`, or (b) through GitHub's raw/preview views of the repo. Kestrel, IIS, Nginx, and any reverse proxy are out of scope.

### Networking

- **None.** The page issues zero network requests. No DNS, no TLS, no CORS, no CDN.

### Storage

- **Git only.** One new blob added to the repository's object store via the PR.
- **Local-only constraint honored:** Per the mandatory stack ("only local, no cloud services"), no cloud storage, no object store, no SaaS is introduced. The existing local SQLite DB in `AgentSquad.Runner/` is untouched.

### CI/CD

- **No changes.** No workflow files are added or modified. No build step, no lint step, no test step, no deploy step is introduced for `status.html`.
- **Existing `dotnet build` behavior is unchanged:** because `status.html` lives at the repository root (not inside any `.csproj` directory and not referenced by any `<Content>` or `<None>` item), MSBuild will not discover or process it.

### Local developer workflow

1. Developer creates `./status.html` at the repo root with the exact reference content.
2. Developer runs `dotnet build` on the existing `.sln` to confirm no regression (expected: identical output to pre-change baseline).
3. Developer opens `status.html` in a browser locally to confirm visual rendering.
4. Developer commits with message `Add status.html` (plus the standard `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer), opens PR.

### Environments

- **Development, staging, production:** Not applicable. There is no runtime artifact to deploy.

---

## Technology Stack Decisions

All decisions respect the mandatory stack: **C# .NET 8 with Blazor Server, only local, no cloud services, `.sln` project structure.**

| Concern | Decision | Justification |
|---|---|---|
| Solution stack | **C# .NET 8 + Blazor Server (existing, untouched)** | Mandated stack. The existing `.sln` and Blazor Server projects remain the canonical runtime surface for the repository; this change adds no runtime code, so the stack is satisfied without modification. |
| Deliverable format | **Plain HTML5 static file** | PM spec mandates a static `status.html` with no CSS/JS/data. HTML5 is the minimal, universally supported, WCAG-compatible format for a browser-renderable artifact. Permitted alongside the Blazor solution per the "static sibling artifact" pattern. |
| File location | **Repository root (`./status.html`)** | Explicitly required by PM acceptance criteria. Placement outside `wwwroot/` and outside any `.csproj` directory guarantees the file is not picked up by MSBuild, not served by `UseStaticFiles`, and not routed by Blazor. |
| Doctype | **`<!DOCTYPE html>` (HTML5)** | Required by PM spec; triggers standards mode in all modern browsers; smallest valid doctype. |
| Encoding | **UTF-8 via `<meta charset="UTF-8">`** | Required by PM spec for correct rendering on systems with non-Latin default encodings. Permitted because `<meta>` is neither CSS nor JS. |
| Language tag | **`<html lang="en">`** | Required by PM spec; satisfies WCAG 2.1 AA Success Criterion 3.1.1 (Language of Page). |
| Styling | **None — browser user-agent defaults only** | Explicitly required by PM. Eliminates scope creep, eliminates network fetches, trivially achieves the <1 KB and <50 ms NFRs. |
| Scripting | **None** | Explicitly required by PM. Zero attack surface; no JS engine involvement. |
| Data source | **None — static literals only** | Explicitly required by PM. No `data.json`, no fetch, no binding. |
| Blazor integration | **None — not routed, not compiled, not in `wwwroot/`** | Required to preserve the untouched-neighbor property of the existing app and to avoid routing conflicts. |
| Testing framework | **None** | Explicitly excluded by PM scope. Manual visual verification + `grep` of the committed file is the verification strategy. |
| CI / build tooling | **Unchanged** | No pipeline modifications permitted by scope. `dotnet build` output must be identical pre- and post-change. |
| Version control | **Git (existing repository)** | Already in use; standard PR workflow. |
| Hosting / runtime | **None** | File has no runtime footprint. Local file viewing and GitHub web UI preview satisfy all viewing scenarios. |
| Cloud services | **None** | Stack mandates "only local, no cloud services." Respected trivially — nothing is deployed. |

---

## Security Considerations

The file presents effectively **zero attack surface**. Security considerations consist primarily of enforcing the "no executable / no external reference" property.

### Authentication & Authorization

- **Not applicable.** The file is not served by the application and is not access-controlled beyond repository read permissions (governed by GitHub, not by this architecture).

### Data protection

- **No data handled.** No PII, no secrets, no tokens, no credentials, no cookies, no storage access. The only textual payload is the literal strings `Project Status` and `All systems nominal.`
- **Secret leakage review:** The file content is reviewed in the PR; both strings are benign and non-sensitive.

### Input validation

- **Not applicable.** The file accepts no input at any lifecycle phase.

### Output encoding / injection

- **Not applicable.** There is no templating and no dynamic content. The two visible strings are static literals embedded directly in HTML and contain no characters requiring HTML entity escaping (no `<`, `>`, `&`, `"`, `'` in visible text).

### Transport security

- **Not applicable.** No network traffic is generated by the page.

### Content Security

- **Static-only enforcement:** The file MUST contain no `<script>`, no inline event handlers (`onclick=`, `onload=`, etc.), no `<link>` (other than none), no `<style>`, no `<iframe>`, no `<object>`, no `<embed>`, no `<img>` with external `src`, no `javascript:` URIs.
- **External reference prohibition:** No `src=`, `href=` to any URL, no `@import`, no web-font linkage, no favicon reference.

### Supply chain

- **No dependencies introduced.** Zero new NuGet packages, npm packages, CDN references, or external assets.

### Review gate (security)

Code review MUST grep the added file for the disallowed tokens (`<style`, `<script`, `stylesheet`, `data.json`, `http://`, `https://`, `src=`, `href=`, `javascript:`, `on<event>=`) and reject the PR if any match is found.

---

## Scaling Strategy

**Scaling is not applicable to this deliverable.** The file is a static text artifact with no runtime footprint; it serves identically to one viewer or one million viewers.

### Concurrency

- **Readers:** Unbounded. Browser rendering is purely client-local.
- **Writers:** Effectively one-time (the creation PR). Subsequent edits are explicitly out of scope.

### Performance budget (NFR)

- **Render latency:** < 50 ms on any modern browser — trivially met, as the document is ~300 bytes with no external resources and no parser backtracking.
- **Payload size:** < 1 KB — trivially met.
- **Network:** Zero requests issued by the page itself.

### Horizontal scaling

- **Not applicable.** No server process hosts the file.

### Vertical scaling

- **Not applicable.** No compute is consumed at render time beyond negligible browser parse cost.

### Caching

- **Not applicable.** If the file is ever served by a future host (out of scope), standard HTTP cache headers managed by that host would apply. This architecture does not prescribe such a host.

### Load on the existing Blazor Server app

- **Zero.** The Blazor Server app does not handle, parse, serve, or reference `status.html`. Its scaling characteristics are unchanged by this architecture.

---

## Risks & Mitigations

| # | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| 1 | **Scope creep** — an implementer adds CSS, JS, a `<link>` to the Blazor layout, or imitates `OriginalDesignConcept.html` styling. | High | High (violates PM acceptance criteria, fails Story 3) | Enforce at code review: grep for `<style`, `<script`, `stylesheet`, `link rel`, `data.json`, `http://`, `https://`, `src=`, `href=`, `javascript:`. Reject PR if any present. Architecture document (this file) enumerates the prohibition explicitly. |
| 2 | **Wrong location** — file placed in `wwwroot/`, `src/`, `AgentSquad.Runner/`, or any project subfolder. | Medium | High (violates Story 1; may accidentally become a Kestrel-served asset and alter Blazor routing semantics) | Reviewer verifies `git diff --name-status` shows exactly `A status.html` at repo root. Architecture mandates sibling placement with `.sln`. |
| 3 | **Modification of existing files** — `.sln`, `.csproj`, `Program.cs`, `appsettings.json`, `wwwroot/`, or `OriginalDesignConcept.html` inadvertently edited. | Low | High (violates Story 3 & Story 4; introduces regression risk to Blazor app) | Reviewer verifies the PR diff contains exactly 1 file added, 0 modified, 0 deleted. `dotnet build` must produce identical output to baseline. |
| 4 | **Incorrect text** — `<h1>` or `<p>` text differs from spec (e.g., missing period, wrong capitalization, extra whitespace). | Medium | Medium (fails Story 1 acceptance criteria) | Copy reference content verbatim from the PM spec/architecture. Reviewer diffs against the canonical snippet character-for-character. |
| 5 | **Missing `<meta charset="UTF-8">`** — causes potential rendering issues on systems with non-Latin default encodings. | Low | Low | Architecture mandates the meta tag; reviewer verifies its presence. |
| 6 | **Missing `lang="en"`** — weakens accessibility (WCAG 2.1 AA SC 3.1.1). | Low | Medium | Architecture mandates `<html lang="en">`; reviewer verifies. |
| 7 | **Byte-order mark (BOM) or non-UTF-8 encoding** — could confuse some tooling. | Low | Low | Save the file as UTF-8 (preferably without BOM). Reviewer spot-checks with a hex viewer if needed. |
| 8 | **Introduction of tests** — violates explicit PM out-of-scope. | Medium | Medium | Architecture explicitly states "no tests." Reviewer rejects any new `*Tests` project, `*.Tests.csproj`, `xUnit`/`NUnit`/`MSTest` additions, or CI assertions touching `status.html`. |
| 9 | **Accidental inclusion in Blazor app static content** — someone adds `<Content Include="..\status.html" />` to a `.csproj`. | Low | Medium | Architecture forbids any `.csproj` edit. Reviewer confirms no `.csproj` is modified in the diff. |
| 10 | **Visual design confusion** — engineer imports Segoe UI / `#0078D4` / grid layout from `OriginalDesignConcept.html`. | Medium | High (fundamentally misunderstands the deliverable) | Architecture explicitly declares `OriginalDesignConcept.html` is NOT the visual reference for `status.html`. PM spec reiterates. Reviewer rejects any CSS referencing those tokens. |
| 11 | **Build regression** — unexpected change to `dotnet build` warnings/errors. | Very low | High | Baseline `dotnet build` before change; re-run after change; diffs must be nil. Because `status.html` is outside any `.csproj` directory, MSBuild ignores it by construction. |
| 12 | **Reviewer not available / bikeshedding** — delays the trivial merge. | Low | Low | PR description links to this architecture and the PM acceptance checklist; reviewer uses the checklist verbatim. Target: merge within one business day. |

### Residual risk

After applying the above mitigations, residual risk is **negligible**. The deliverable has no runtime, no data, no network, and no build participation; the only meaningful failure modes are human errors in content or placement, all of which are caught by a mechanical PR review checklist.