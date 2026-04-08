# data.json Schema Documentation

## Overview
The `data.json` file is the primary configuration source for the Executive Project Reporting Dashboard. It contains project metadata, milestone definitions, and work item tracking in a simple JSON format.

## File Location & Encoding
- **Location**: `wwwroot/data.json` (relative to application root)
- **Encoding**: UTF-8 (no BOM - Byte Order Mark)
- **Line Endings**: LF (Unix-style) or CRLF (Windows-style) both supported

## Root Object Structure

The root of `data.json` is a single JSON object with the following shape: