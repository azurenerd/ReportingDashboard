# PM Specification: My Project

# Product Specification: Executive Reporting Dashboard

## Executive Summary
A single-page Blazor Server web application that renders a project milestone and progress dashboard from a local `data.json` file, designed for screenshot capture into executive PowerPoint decks.

## Business Goals
Provide program managers a clean, self-contained local dashboard to communicate project status—milestones, shipped items, in-progress work, and carryover—without requiring cloud infrastructure or authentication overhead.

## User Stories

- **As a PM, I want to see a timeline of major milestones** at the top of the page so I can quickly orient executives on the project's big rocks and their target dates. *AC: Timeline renders milestone names, dates, and completion status from `data.json`.*

- **As a PM, I want categorized status sections (Shipped, In Progress, Carried Over)** so executives can see what was delivered, what's active, and what slipped. *AC: Each section lists items with title, owner, and status indicator sourced from `data.json`.*

- **As a PM, I want an overall project health summary** displayed prominently so the project's RAG status is immediately visible. *AC: Dashboard shows a color-coded health indicator (Green/Yellow/Red) and a one-line summary.*

- **As a PM, I want to edit only `data.json` to update the dashboard** so I don't need to modify code between reporting cycles. *AC: All displayed content is driven entirely by the JSON config file.*

- **As a PM, I want the page to be screenshot-friendly** so I can paste clean visuals into PowerPoint. *AC: Layout fits a single browser viewport with no scrolling required for core content; clean typography and whitespace.*

## Scope
**In scope:** Single-page Blazor Server app, local `data.json` reader, milestone timeline, status sections, .sln project structure, fictional sample data, design aligned to `OriginalDesignConcept.html`.
**Out of scope:** Authentication, cloud deployment, database, multi-page navigation, real-time collaboration, printing/export features.

## Non-Functional Requirements
The app must run locally on .NET 8 with no external dependencies, render fully in under 2 seconds, and produce a visually polished layout suitable for direct screenshot capture at 1920×1080 resolution.