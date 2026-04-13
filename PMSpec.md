# PM Specification: My Project

# Product Specification: Executive Reporting Dashboard

## Executive Summary

A single-page Blazor Server web application that renders a visually polished project status dashboard from a local `data.json` file, designed for screenshot capture into executive PowerPoint decks.

## Business Goals

Provide program managers a simple, local tool to generate consistent, professional project status views showing milestones, shipped work, in-progress items, and carryover—eliminating manual slide formatting.

## User Stories

- **As a PM, I want a milestone timeline at the top of the page** so executives can see key project big rocks and their target dates at a glance. *AC: Timeline renders horizontally with milestone names, dates, and completion status from data.json.*

- **As a PM, I want categorized work item sections (Shipped, In Progress, Carried Over)** so stakeholders understand current sprint/month status. *AC: Each section displays items with title, owner, and status indicator; sections collapse gracefully when empty.*

- **As a PM, I want an overall project health summary** so executives can assess status in seconds. *AC: Dashboard shows project name, reporting period, RAG status, and a one-line summary.*

- **As a PM, I want to edit only data.json to update the dashboard** so I can refresh content without code changes. *AC: All displayed data is driven from data.json; app hot-reloads on file change.*

- **As a PM, I want the page to be screenshot-friendly** so captured images look clean in PowerPoint. *AC: Layout fits 1920×1080 without scrolling; no interactive-only elements; clean typography.*

## Scope

**In Scope:** Static single-page Blazor Server app, local data.json reader, milestone timeline component, work item sections, sample fictional project data, .sln project structure, design aligned with OriginalDesignConcept.html and ReportingDashboardDesign.png.

**Out of Scope:** Authentication, cloud deployment, database, multi-project support, real-time collaboration, export/print functionality.

## Non-Functional Requirements

The app must run locally on .NET 8 with no external dependencies or cloud services. Page must render fully within 2 seconds and display correctly in Chromium-based browsers at 1920×1080 resolution.