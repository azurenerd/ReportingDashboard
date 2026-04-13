# PM Specification: My Project

# Product Specification: Executive Reporting Dashboard

## Executive Summary

A single-page Blazor Server web application that renders a project milestone and progress dashboard from a local `data.json` config file, designed for screenshot capture into executive PowerPoint decks.

## Business Goals

Provide program managers a lightweight, locally-hosted dashboard to communicate project status—milestones, shipped items, in-progress work, and carryover—to executive stakeholders without requiring cloud infrastructure or authentication.

## User Stories

- **As a PM, I want to see a visual milestone timeline at the top of the page** so I can quickly communicate key project big rocks and their target dates. *AC: Timeline renders milestones chronologically with status indicators (complete, in-progress, upcoming).*

- **As a PM, I want categorized sections for Shipped, In Progress, and Carried Over items** so executives can see work status at a glance. *AC: Each category displays item name, description, and relevant dates from data.json.*

- **As a PM, I want to edit a single data.json file to update all dashboard content** so I can refresh the report without code changes. *AC: Modifying data.json and refreshing the browser reflects changes immediately.*

- **As a PM, I want the dashboard styled for clean screenshot capture** so it renders well when pasted into PowerPoint. *AC: Layout fits a single page, uses high-contrast colors, and renders crisply at 1920×1080.*

- **As a PM, I want the design to follow and improve upon the existing OriginalDesignConcept.html template** so it maintains visual continuity. *AC: All agents read the HTML design file before implementation; output preserves the template's structure and aesthetic.*

## Scope

**In scope:** .NET 8 Blazor Server app, local-only hosting, single Razor page, data.json reader, milestone timeline component, status category sections, solution file structure. **Out of scope:** Authentication, cloud deployment, database, multi-user support, editing UI.

## Non-Functional Requirements

The app must launch locally via `dotnet run`, load in under 2 seconds, and render correctly in Chromium-based browsers. The solution must follow standard .sln/.csproj structure with no external service dependencies.