# wwwroot - Static Assets Directory

## Overview

This directory contains all static assets served by the Blazor Server application. The dashboard uses Pico CSS (~10KB minified) for minimal, elegant styling and Chart.js for timeline visualization. Static files are served with intelligent caching: data.json never caches (no-cache) for fresh data on each page load, all other assets cache for 1 day.

## Directory Structure