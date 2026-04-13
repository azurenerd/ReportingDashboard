# PM Specification: My Project

# Product Specification: Executive Reporting Dashboard

## Executive Summary
A single-page Blazor Server web application that displays project milestones, progress, and status for executive stakeholders. The dashboard reads from a local `data.json` configuration file and is designed for screenshot capture into PowerPoint decks.

## Business Goals
Provide a clean, at-a-glance project status view that communicates milestone timelines, shipped items, in-progress work, and carryover items to executive audiences without requiring live demos or complex tooling.

## User Stories

- **As a program manager**, I want to edit a `data.json` file to update project data so the dashboard reflects current status without code changes.
  - *AC: Changing `data.json` and refreshing the page shows updated content.*

- **As an executive viewer**, I want to see a milestone timeline at the top of the page so I can quickly understand key project dates and big rocks.
  - *AC: Timeline renders horizontally with milestone names, dates, and completion status.*

- **As a program manager**, I want categorized sections for Shipped, In Progress, and Carried Over items so status is immediately clear.
  - *AC: Each category is visually distinct with item counts and descriptions.*

- **As a program manager**, I want to take a browser screenshot that looks polished in a PowerPoint deck.
  - *AC: The page fits a single viewport, uses clean typography, and has no scrolling dependencies for core content.*

- **As a user**, I want to run the dashboard locally with `dotnet run` and no additional setup.
  - *AC: No database, authentication, or cloud service required.*

## Scope
**In scope:** Blazor Server app, `data.json` reader, milestone timeline component, status category sections, fictional sample data, `.sln` project structure, design based on `OriginalDesignConcept.html`.
**Out of scope:** Authentication, cloud deployment, database, real-time updates, multi-project support, export functionality.

## Non-Functional Requirements
- **Performance:** Page loads in under 2 seconds locally.
- **Compatibility:** Renders correctly in Chromium-based browsers at 1920×1080.
- **Simplicity:** Single project structure with no external dependencies beyond the .NET 8 SDK.
- **Maintainability:** All display data driven by `data.json`; no hardcoded project content in markup.