import './dashboard.css';
import { fetchRoadmap } from './api/roadmapApi';
import type { RoadmapData } from './models/types';

const app = document.getElementById('app')!;

function showLoading(): void {
    app.innerHTML = '<div class="loading">Loading dashboard…</div>';
}

function showError(message: string): void {
    const existing = app.querySelector('.error-banner');
    if (existing) existing.remove();
    const banner = document.createElement('div');
    banner.className = 'error-banner';
    banner.textContent = message;
    const retry = document.createElement('button');
    retry.textContent = 'Retry';
    retry.addEventListener('click', () => init());
    banner.appendChild(retry);
    app.prepend(banner);
}

function rerenderAll(_data: RoadmapData): void {
    // Component renderers will be wired in subsequent tasks:
    // renderHeader(), renderTimeline(), renderHeatmap()
    app.innerHTML = '<div class="loading">Dashboard components loading…</div>';
}

async function init(): Promise<void> {
    showLoading();
    try {
        const data = await fetchRoadmap();
        rerenderAll(data);
    } catch (err) {
        const message = err instanceof Error ? err.message : 'Unknown error';
        showError(`Could not connect to the dashboard API. ${message}`);
    }
}

init();