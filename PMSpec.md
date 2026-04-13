# PM Specification: My Project

# Product Specification: Executive Reporting Dashboard

## Executive Summary

A single-page Blazor Server web application that displays project milestone progress and status for executive reporting, reading all data from a local `data.json` configuration file. Designed for simplicity and screenshot-friendly presentation in PowerPoint decks.

## Business Goals

Provide a clean, visual project status page that program managers can screenshot for executive presentations, eliminating manual slide creation while ensuring consistent, data-driven reporting on milestones, shipped items, in-progress work, and carryover tasks.

## User Stories

- **As a PM, I want to see a milestone timeline at the top of the page** so I can quickly communicate key project big rocks and their target dates. *AC: Timeline renders horizontally with milestone names, dates, and completion status from data.json.*
- **As a PM, I want to view items shipped this month** so I can highlight team accomplishments. *AC: Shipped items display with title, description, and completion date.*
- **As a PM, I want to see in-progress work items** so executives understand current focus areas. *AC: In-progress section shows item name, owner, and percent complete.*
- **As a PM, I want carryover items from last month clearly identified** so I can address schedule risks. *AC: Carryover section displays items with original target date and reason for delay.*
- **As a PM, I want to screenshot the page directly into PowerPoint** so reporting is fast and clean. *AC: Page renders within standard screen resolution with no scrolling required for key sections.*

## Scope

**In Scope:** Single Blazor Server page, local `data.json` data source, milestone timeline component, status sections (shipped/in-progress/carryover), .sln project structure, example fictional project data. Design follows and improves upon `OriginalDesignConcept.html`.

**Out of Scope:** Authentication, cloud deployment, database, multi-project support, real-time updates, API integrations.

## Non-Functional Requirements

Page must load under 2 seconds locally, render cleanly at 1920×1080 for screenshots, use .NET 8 Blazor Server, and require no external dependencies or cloud services. All styling self-contained with no CDN references.