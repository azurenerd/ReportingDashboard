# Project Data Structure Reference

## Overview
The `example-data.json` file demonstrates the complete data structure for the Executive Project Dashboard. This file serves as a template for users to populate with their own project data.

## Root Level Fields

### Project Metadata
- **name** (required): "Project Blue Sky" - Project name or codename. String, 1-200 characters.
- **description** (optional): Project summary. String, max 1000 characters. Provides context about project goals.
- **startDate** (optional): "2024-01-01" - Project start date in ISO 8601 format (YYYY-MM-DD).
- **targetEndDate** (optional): "2024-12-31" - Project target completion date in ISO 8601 format.

### Project Health Indicators
- **completionPercentage** (required): 65 - Overall completion percentage. Integer, 0-100 range.
- **healthStatus** (required): "OnTrack" - Project health status. Valid values: OnTrack, AtRisk, Blocked.
- **velocityThisMonth** (optional): 12 - Work items completed in current month. Integer, 0+.

## Milestones Array

Each milestone represents a major project deliverable or phase.

### Milestone Fields
- **name** (required): "Core Infrastructure" - Milestone title. String, 1-200 characters.
- **targetDate** (required): "2024-03-15" - Target completion date. ISO 8601 format.
- **status** (required): "Completed" - Current milestone status. Valid values:
  - **Completed**: Finished and delivered
  - **InProgress**: Currently active
  - **AtRisk**: At risk of missing target date
  - **Future**: Not yet started
- **description** (optional): Milestone summary. String, max 500 characters.

### Example Milestone Status Progression
- Core Infrastructure → **Completed** (Q1)
- Feature Development → **InProgress** (Q2)
- Performance Optimization → **AtRisk** (Q3)
- Security Hardening → **Future** (Q4)
- Final Release Prep → **Future** (Q4)

## Work Items Array

Work items represent individual tasks or features distributed across three status categories.

### Work Item Fields
- **title** (required): "User Authentication System" - Task name. String, 1-200 characters.
- **description** (optional): Task details. String, max 500 characters. Include scope and integration points.
- **status** (required): "Shipped" - Work item status. Valid values:
  - **Shipped**: Completed and deployed this month
  - **InProgress**: Currently being worked on
  - **CarriedOver**: Planned but not completed, carried to next period
- **assignedTo** (optional): "Team A" - Owner/responsible party. String, max 100 characters.

### Work Item Distribution (Example)
- **Shipped (5 items)**: Early-stage work completed January-March
  - User Authentication System
  - Database Schema Migration
  - API Gateway Implementation
  - Reporting Module
  - Client SDK Libraries

- **InProgress (4 items)**: Current sprint priorities
  - Real-time Notifications
  - Advanced Search Capabilities
  - Multi-tenancy Support
  - Analytics Pipeline

- **CarriedOver (3 items)**: Overdue or deferred items
  - Performance Benchmarking
  - Security Vulnerability Scanning
  - Disaster Recovery Plan

## Enum Reference

### MilestoneStatus (for milestones.status)
- `Completed` - Milestone delivered and closed
- `InProgress` - Actively being worked
- `AtRisk` - Likely to miss target date
- `Future` - Scheduled but not started

### WorkItemStatus (for workItems.status)
- `Shipped` - Released this month
- `InProgress` - In active development
- `CarriedOver` - Deferred to next period

### HealthStatus (for project healthStatus)
- `OnTrack` - Project proceeding as planned
- `AtRisk` - Project at risk due to resource/timeline issues
- `Blocked` - Project blocked by external dependency

## Validation Rules

All fields must conform to the JSON Schema in `data-schema.json`:
- ✅ UTF-8 encoding with no BOM
- ✅ All enum values must exactly match schema definitions (case-sensitive)
- ✅ Dates must be valid ISO 8601 format
- ✅ CompletionPercentage must be 0-100 inclusive
- ✅ Required fields cannot be null or empty
- ✅ No additional fields beyond schema definition

## System.Text.Json Compatibility

This file parses cleanly with C# System.Text.Json: