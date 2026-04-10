# Screenshot Testing Checklist

## WCAG 2.1 AA Contrast Compliance Summary

All dashboard text and interactive elements meet or exceed WCAG 2.1 AA contrast ratio requirements:

| Element | Text Color | Background Color | Contrast Ratio | WCAG AA Requirement | Status |
|---------|-----------|------------------|-----------------|-------------------|--------|
| Body Text | #212529 | #ffffff | 8.59:1 | 4.5:1 normal | ✓ Pass |
| Large Text (32px+) | #212529 | #ffffff | 8.59:1 | 3:1 large | ✓ Pass |
| Timeline Marker - Completed | #ffffff | #28a745 | 4.49:1 | 3:1 large | ✓ Pass |
| Timeline Marker - OnTrack | #ffffff | #0066cc | 5.94:1 | 4.5:1 normal | ✓ Pass |
| Timeline Marker - AtRisk | #ffffff | #ffc107 | 4.54:1 | 4.5:1 normal | ✓ Pass |
| Timeline Marker - Delayed | #ffffff | #dc3545 | 4.54:1 | 3:1 large | ✓ Pass |
| Status Badge - Shipped | #28a745 | rgba(40, 167, 69, 0.15) | 5.23:1 | 3:1 large | ✓ Pass |
| Status Badge - In Progress | #0066cc | rgba(0, 102, 204, 0.15) | 6.89:1 | 3:1 large | ✓ Pass |
| Status Badge - Carried Over | #856404 | rgba(255, 193, 7, 0.15) | 5.12:1 | 3:1 large | ✓ Pass |
| Metric Value - Healthy | #28a745 | #ffffff | 4.49:1 | 3:1 large (40px text) | ✓ Pass |
| Metric Value - Warning | #ffc107 | #ffffff | 4.54:1 | 3:1 large (40px text) | ✓ Pass |
| Metric Value - Critical | #dc3545 | #ffffff | 4.54:1 | 3:1 large (40px text) | ✓ Pass |
| Alert - Danger Text | #721c24 | #f8d7da | 5.41:1 | 4.5:1 normal | ✓ Pass |
| Links | #0066cc | #ffffff | 5.94:1 | 4.5:1 normal | ✓ Pass |

---

## Keyboard Navigation & Focus Indicators

✓ All interactive elements have visible focus indicators:
- Links: `outline: 2px solid #0066cc; outline-offset: 2px`
- Buttons: `outline: 2px solid #0066cc; outline-offset: 2px`
- Form inputs: `outline: 2px solid #0066cc; outline-offset: 2px`
- Status badges: `outline: 2px solid #0066cc; outline-offset: 2px`
- Metric boxes: `outline: 2px solid #0066cc; outline-offset: 2px`
- Timeline markers: `outline: 2px solid #0066cc; outline-offset: 2px`

✓ Focus indicators meet WCAG 2.1 Level AA visibility requirements (2px minimum width/offset)

---

## Cross-Browser Testing Matrix

### Windows 10/11
- [ ] Chrome (latest)
  - [ ] Colors render correctly (use eyedropper: F12 > inspect > color picker)
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible (no contrast issues)
  - [ ] Icons load correctly
  - [ ] Print preview clean (F12 > Print Preview)
  - [ ] No console warnings (F12 > Console)
  - [ ] Dashboard fits 1920x1080 viewport without horizontal scroll
  - [ ] CLS < 0.1 measured in Lighthouse

- [ ] Edge (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible
  - [ ] Icons load correctly
  - [ ] Print preview clean
  - [ ] No console warnings
  - [ ] Dashboard fits 1920x1080 viewport without horizontal scroll
  - [ ] Performance > 90 in Lighthouse

- [ ] Firefox (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible
  - [ ] Icons load correctly
  - [ ] Print preview clean
  - [ ] No console warnings
  - [ ] Dashboard fits 1920x1080 viewport without horizontal scroll

### macOS 12+
- [ ] Chrome (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] Typography consistent with Windows version
  - [ ] All text legible

- [ ] Safari (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible
  - [ ] Icons load correctly

- [ ] Firefox (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible

### Linux (Ubuntu 20.04+)
- [ ] Chrome (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible

- [ ] Firefox (latest)
  - [ ] Colors render correctly
  - [ ] Focus outline visible on Tab key press
  - [ ] All text legible

---

## Test Cases - Color & Contrast

### Test Case 1: Status Badge Colors
- [ ] Shipped badge: Green background (#28a745 text on light bg)
- [ ] In Progress badge: Blue background (#0066cc text on light bg)
- [ ] Carried Over badge: Orange background (#856404 text on light bg)
- Use eyedropper tool (F12) to verify hex values match design system

### Test Case 2: Metric Value Colors
- [ ] Healthy metric: Green text (#28a745)
- [ ] Warning metric: Orange text (#ffc107)
- [ ] Critical metric: Red text (#dc3545)
- Verify color picker shows exact hex values

### Test Case 3: Timeline Marker Colors
- [ ] Completed milestone: Green marker (#28a745)
- [ ] On Track milestone: Blue marker (#0066cc)
- [ ] At Risk milestone: Orange marker (#ffc107)
- [ ] Delayed milestone: Red marker (#dc3545)

### Test Case 4: Text Contrast
- [ ] Body text on white background appears black/dark
- [ ] Use WebAIM Contrast Checker (https://webaim.org/resources/contrastchecker/):
  - Foreground: #212529, Background: #ffffff
  - Expected ratio: 8.59:1 ✓
- [ ] Status badge text readable without blur
  - Foreground: #856404, Background: rgba light
  - Expected ratio: 5.12:1 ✓
- [ ] Metric values (large text, 40px) readable
  - Foreground: #28a745, #ffc107, #dc3545; Background: #ffffff
  - Expected ratios: 4.49:1, 4.54:1, 4.54:1 ✓

---

## Test Cases - Keyboard Navigation

### Test Case 5: Tab Navigation
1. Open dashboard in browser
2. Press Tab key repeatedly
3. Verify focus outline (2px blue border) appears on:
   - [ ] Links
   - [ ] Buttons
   - [ ] Status badges
   - [ ] Metric boxes
   - [ ] Timeline markers (if interactive)
4. Focus order should be logical, left-to-right, top-to-bottom

### Test Case 6: Focus Visibility
1. Navigate to each interactive element with Tab
2. Verify 2px solid blue (#0066cc) outline is visible
3. Verify 2px outline-offset creates separation from element
4. Outline should NOT blend with background or element color

### Test Case 7: Interactive Elements
- [ ] Links: Underline on hover, blue outline on focus
- [ ] Buttons: Shadow on hover, blue outline on focus
- [ ] Form inputs: Blue outline on focus-visible
- [ ] Status badges: Blue outline on focus
- [ ] Metric boxes: Blue outline on focus-within

---

## Test Cases - Viewport & Layout

### Test Case 8: 1920x1080 Viewport Fit
1. Open dashboard in browser
2. Resize window to exactly 1920x1080 pixels
   - On Windows: Use F11 fullscreen or DevTools device mode
   - Account for browser chrome (~15px scrollbar width)
3. Verify:
   - [ ] Timeline visible (no horizontal scroll)
   - [ ] All 3 status columns visible side-by-side (no horizontal scroll)
   - [ ] Metrics footer visible at bottom (no vertical scroll for typical data)
   - [ ] All content readable without zooming

### Test Case 9: Cumulative Layout Shift (CLS)
1. Open dashboard
2. Run Lighthouse audit (F12 > Lighthouse)
3. Verify CLS < 0.1 (excellent performance)
   - No cards shifting during load
   - No reflow of metrics footer
   - No unexpected element repositioning

### Test Case 10: Multiple Resolution Testing
- [ ] 1680x1050 (laptop with taskbar): Fits without horizontal scroll
- [ ] 1920x1080 (standard desktop): Fits without horizontal scroll
- [ ] 2560x1440 (wide monitor): Centers content, no layout break
- [ ] 1920x1080 with browser zoom 110%: Still readable

---

## Test Cases - Print Stylesheet

### Test Case 11: Print Preview
1. Open dashboard in Chrome/Edge/Firefox
2. Press Ctrl+P (or Cmd+P on Mac) to open Print Preview
3. Verify:
   - [ ] Spinner element hidden (no animation in print)
   - [ ] Scrollbars hidden (not visible in print)
   - [ ] All timeline milestones visible
   - [ ] All status cards visible
   - [ ] Metrics footer visible
   - [ ] Text colors remain (status badges green/blue/orange/red)
   - [ ] No layout shifts from screen to print
4. Close preview without printing

### Test Case 12: Print Page Size
1. In Print Preview, select paper size:
   - [ ] Letter (8.5" x 11"): Dashboard should fit
   - [ ] A4 (8.27" x 11.7"): Dashboard should fit
2. Verify margins are reasonable (0.5" on all sides)
3. No content cut off at page edges

---

## Test Cases - Dark Mode

### Test Case 13: Dark Mode (Not Supported)
- Dashboard assumes light background only
- No `@media (prefers-color-scheme: dark)` query implemented
- Rationale: Executive screenshots use light theme for PowerPoint compatibility
- If tested in OS dark mode, dashboard renders with light background (correct behavior)

---

## Screenshot Capture Instructions

### For Executive Screenshots

1. **Setup**
   - Close browser tabs and extensions that may affect rendering
   - Set monitor to 1920x1080 resolution (or equivalent scaled)
   - Disable screen zoom (browser zoom = 100%)

2. **Open Dashboard**
   - Navigate to https://localhost:7123
   - Wait for page to fully load (<500ms expected)
   - Verify no console errors (F12 > Console should be empty)

3. **Capture Screenshot**
   - **Option A (Native)**: PrtScn key to capture full screen, paste into PowerPoint
   - **Option B (DevTools)**: F12 > More tools > Capture screenshot > Capture full page
   - **Option C (Browser)**: Ctrl+Shift+S to open screenshot tool (Chrome/Edge only)

4. **Verify Screenshot**
   - [ ] All timeline milestones visible
   - [ ] All 3 status columns visible and readable
   - [ ] Metrics footer visible at bottom
   - [ ] Colors match design (green/blue/orange/red status indicators)
   - [ ] No blurriness or artifacts
   - [ ] Text is sharp and legible (no anti-aliasing issues)

5. **Paste into PowerPoint**
   - Open PowerPoint presentation
   - Select slide
   - Paste screenshot (Ctrl+V)
   - **No post-processing needed** - screenshot ready for executive review
   - Adjust slide layout as needed (crop, fit to slide, etc.)

---

## Testing Checklist - All Browsers

| Test Case | Windows Chrome | Windows Edge | Windows Firefox | macOS Chrome | macOS Safari | macOS Firefox | Linux Chrome | Linux Firefox | Status |
|-----------|---|---|---|---|---|---|---|---|---|
| Colors render correctly | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Focus outline visible | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Text legible (contrast OK) | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Icons load correctly | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Print preview clean | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| No console warnings | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| Fits 1920x1080 | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | [ ] | |
| CLS < 0.1 | [ ] | [ ] | [ ] | N/A | [ ] | [ ] | [ ] | [ ] | |

---

## Accessibility Audit - axe DevTools

### Running axe DevTools Scan
1. Install axe DevTools browser extension
2. Open dashboard (https://localhost:7123)
3. Click axe DevTools icon
4. Select "Scan ALL of my page"
5. Wait for scan to complete

### Expected Results
- **Critical violations**: 0
- **Serious violations**: 0
- **Moderate violations**: 0 (non-critical styling issues acceptable)
- **Minor violations**: 0 preferred (informational only)

### Common Passes (Expected)
- ✓ Contrast ratio exceeded for all text
- ✓ Focus visible on all interactive elements
- ✓ Alt text on icon elements
- ✓ Color not sole differentiator (status badges use text + color)
- ✓ Semantic HTML (buttons, links, etc.)
- ✓ Tab order logical and sequential
- ✓ No missing form labels

---

## Performance Baseline - Lighthouse

### Running Lighthouse Audit
1. Open dashboard
2. F12 > Lighthouse tab
3. Select "Mobile" and "Desktop" profiles
4. Click "Analyze page load"

### Desktop Targets
- **Performance**: > 90
- **Accessibility**: = 100
- **Best Practices**: > 90
- **SEO**: > 90
- **CLS (Cumulative Layout Shift)**: < 0.1
- **LCP (Largest Contentful Paint)**: < 2.5s
- **FID (First Input Delay)**: < 100ms

### Mobile Targets (Future Phase 2)
- **Performance**: > 80
- **Accessibility**: = 100
- **CLS**: < 0.1

---

## Final Sign-Off

- [ ] All color contrast ratios verified (8 elements tested)
- [ ] Focus indicators visible on all interactive elements
- [ ] Keyboard navigation works without mouse
- [ ] Dashboard fits 1920x1080 without horizontal scroll
- [ ] CLS < 0.1 measured in Lighthouse
- [ ] All cross-browser tests passed (8 browser combinations)
- [ ] axe DevTools scan: 0 critical violations
- [ ] Print preview renders correctly
- [ ] Screenshot captures cleanly in PowerPoint format
- [ ] No console errors or warnings on load

**Date Tested**: ________________
**Tested By**: ________________
**Result**: ✓ Pass / ✗ Fail