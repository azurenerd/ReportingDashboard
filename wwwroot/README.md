# wwwroot Directory Structure

This directory contains all static assets served by the ASP.NET Core application.

## Directory Layout

- **css/** - Stylesheets including print-optimized dashboard styling
- **js/** - JavaScript utilities for interactive dashboard components
- **data/** - JSON configuration files for project data
- **index.html** - Blazor Server entry point

## Asset Loading

- `index.html` serves as the root document for the Blazor Server application
- All static files are served without compression for screenshot clarity
- CSS is optimized for 1024x768 minimum and 1920x1080 target resolution
- Print media queries ensure clean PowerPoint-ready output

## Font Stack

Global font stack prioritizes cross-browser consistency:
- System fonts: -apple-system, BlinkMacSystemFont
- Fallback: "Segoe UI", Roboto, sans-serif

## Color Scheme

Reserved for color definitions in site.css:
- Primary: Executive dashboard brand color
- Success: Green for completed milestones
- Warning: Red for at-risk items
- Info: Blue for in-progress items
- Neutral: Gray for future items