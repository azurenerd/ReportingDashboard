# Integration Test Checklist

## Pre-Launch Verification

### Static Assets

- [ ] `wwwroot/index.html` exists and loads
- [ ] `wwwroot/css/site.css` exists (25+ KB)
- [ ] `wwwroot/css/print.css` exists (8+ KB)
- [ ] `wwwroot/js/utils.js` exists
- [ ] `wwwroot/js/charts.js` exists
- [ ] `wwwroot/js/dashboard.js` exists
- [ ] `wwwroot/data/data.json` exists with valid JSON
- [ ] `wwwroot/favicon.ico` exists (optional)
- [ ] All files readable and accessible

### Program.cs Configuration

- [ ] `app.UseStaticFiles()` configured
- [ ] Static file middleware includes MIME type mappings
- [ ] Cache-Control headers configured correctly
- [ ] No syntax errors in Program.cs

### Project File Configuration

- [ ] AgentSquad.Runner.csproj references Dashboard project
- [ ] All NuGet packages restore without errors
- [ ] No missing dependencies

## Startup Verification

### Application Startup

- [ ] `dotnet run` completes without errors
- [ ] No port binding conflicts
- [ ] "Now listening on: http://localhost:5000" message appears
- [ ] No file not found exceptions in console
- [ ] No static file 404 warnings

### Initial Page Load

- [ ] HTTP 200 response for index.html
- [ ] HTML renders in browser
- [ ] Loading spinner appears briefly
- [ ] Dashboard loads within 3 seconds
- [ ] No JavaScript errors in console

## Asset Loading Tests

### CSS Files

- [ ] site.css loads: 200 OK status
- [ ] print.css loads: 200 OK status
- [ ] Bootstrap CDN loads: 200 OK status
- [ ] Google Fonts loads: 200 OK status
- [ ] No MIME type warnings
- [ ] Styles apply to DOM elements

### JavaScript Files

- [ ] blazor.server.js loads: 200 OK
- [ ] utils.js loads: 200 OK
- [ ] charts.js loads: 200 OK
- [ ] dashboard.js loads: 200 OK
- [ ] Chart.js CDN loads: 200 OK
- [ ] No syntax errors in console
- [ ] No undefined variable errors

### Data Files

- [ ] data.json loads: 200 OK
- [ ] Valid JSON structure (no parse errors)
- [ ] Contains required fields: project, metrics, milestones, workItems
- [ ] Displays in dashboard

## Visual Rendering Tests

### Dashboard Header

- [ ] Project title displays
- [ ] Subtitle visible
- [ ] Proper font and sizing
- [ ] Correct colors applied

### Metrics Grid

- [ ] 4 metric cards display
- [ ] Each card has colored top border (status-coded)
- [ ] Cards responsive to screen size
- [ ] Values populate from data.json
- [ ] Labels clear and uppercase
- [ ] Details visible below values

### Timeline Section

- [ ] Timeline title displays
- [ ] Milestones render horizontally
- [ ] Status-coded borders (green/blue/red/gray)
- [ ] Names and dates visible
- [ ] Horizontal scroll works on small screens
- [ ] No cutoff at edges

### Work Items Section

- [ ] 3 work item columns display
- [ ] Column headers with item counts visible
- [ ] Items list properly formatted
- [ ] Description text readable
- [ ] Colors consistent (status-coded)
- [ ] Responsive to screen size

## Responsive Design Tests

### 1024x768 (Minimum)

- [ ] All sections visible without horizontal scroll
- [ ] No text overlap or cutoff
- [ ] Metrics: 2-4 cards visible
- [ ] Work items: minimum 1 column
- [ ] Timeline: horizontal scroll available
- [ ] Text readable at normal distance
- [ ] Buttons clickable and accessible

### 1280x720 (PowerPoint HD)

- [ ] Metrics: 4-column layout visible
- [ ] Work items: 3-column layout visible
- [ ] Timeline: compact fit
- [ ] All content fits without scrolling
- [ ] Spacing appropriate for presentation
- [ ] No visual elements cut off

### 1920x1080 (4K Optimal)

- [ ] Spacing optimized
- [ ] Content centered with margins
- [ ] No excessive whitespace
- [ ] Typography scales appropriately
- [ ] Full visual hierarchy apparent
- [ ] Professional appearance maintained

## Print/Screenshot Tests

### Print Preview

- [ ] Colors preserved (print-color-adjust: exact)
- [ ] No shadows visible (replaced with borders)
- [ ] Page breaks avoid splitting sections
- [ ] All content fits on pages
- [ ] Text readable in print
- [ ] Links underlined appropriately

### Chrome Print (Ctrl+P)

- [ ] Save as PDF successful
- [ ] PDF opens correctly
- [ ] Colors exact (green #10b981, blue #3b82f6, etc.)
- [ ] No page breaks mid-section
- [ ] Metrics visible (2 columns in print)
- [ ] Work items visible (3 columns in print)

### PowerPoint Screenshot Simulation

- [ ] 1280x720 capture: clean output, all elements visible
- [ ] 1920x1080 capture: full optimization, no cutoff
- [ ] Colors accurate in screenshot
- [ ] Text rendering clean
- [ ] Suitable for executive presentation

## Performance Tests

### Load Time Metrics

- [ ] First Contentful Paint: < 1.5s
- [ ] Largest Contentful Paint: < 2.5s
- [ ] Time to Interactive: < 3s
- [ ] Total Blocking Time: < 200ms

### Cache Effectiveness

- [ ] site.css: max-age=2592000 (30 days)
- [ ] utils.js: max-age=2592000 (30 days)
- [ ] data.json: max-age=0 (no cache, fresh)
- [ ] Subsequent loads use cache

### Network Optimization

- [ ] No 404 errors
- [ ] No failed requests
- [ ] No CORS errors
- [ ] Reasonable file sizes

## Cross-Browser Compatibility

### Chrome (Latest)

- [ ] Pages load without errors
- [ ] CSS applies correctly
- [ ] All features work
- [ ] Print preview clean
- [ ] No console errors

### Edge (Latest)

- [ ] Pages load identically to Chrome
- [ ] Styling matches Chrome
- [ ] Print output matches Chrome
- [ ] No console errors

### Firefox (Latest, if tested)

- [ ] Pages load
- [ ] CSS applies (may vary slightly)
- [ ] Functionality works
- [ ] Print works

## Accessibility Tests

### Keyboard Navigation

- [ ] Tab key navigates through interactive elements
- [ ] Focus indicators visible (blue outline)
- [ ] Can access all content via keyboard
- [ ] No keyboard traps

### Color Contrast

- [ ] Text colors meet WCAG AA standards
- [ ] Status colors distinguishable
- [ ] Print colors visible on white background
- [ ] High contrast mode supported

### Screen Reader (if testing)

- [ ] Proper semantic HTML
- [ ] Headings properly marked
- [ ] Labels associated with content
- [ ] ARIA labels present where needed

## Security Tests

### MIME Type Safety

- [ ] X-Content-Type-Options: nosniff header set
- [ ] Correct MIME types for all assets
- [ ] No script injection vulnerabilities
- [ ] JSON files served as application/json

### Denial of Service Prevention

- [ ] No infinite loops or memory leaks
- [ ] Large data files load without hanging
- [ ] Responsive UI during loading

## Data Validation

### data.json Structure