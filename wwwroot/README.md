# wwwroot - Static Assets Directory

## Overview

This directory contains all static assets served by the Blazor Server application. The dashboard loads these assets with intelligent caching configured in Program.cs: data.json never caches (no-cache) for fresh data on each load, all other assets cache for 1 day.

## Directory Structure