// Entry point - renders the full dashboard
// Rendering functions will be implemented by subsequent tasks

async function init(): Promise<void> {
    const app = document.getElementById('app');
    if (!app) {
        console.error('Could not find #app element');
        return;
    }

    app.innerHTML = '<p style="padding:44px;color:#888;">Dashboard loading…</p>';
}

document.addEventListener('DOMContentLoaded', init);