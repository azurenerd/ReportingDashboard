# ProjectMetrics Component - Testing & Verification Guide

## Print/Screenshot Testing

### Test Resolutions
- **1024x768**: Minimum resolution (2 columns)
- **1280x720**: HD presentation (4 columns)
- **1920x1080**: Full HD presentation (4 columns)

### Browser Verification
- **Chrome**: Latest version
- **Edge**: Latest version
- Both browsers support print color preservation via CSS print rules.

### Print Output Verification
1. Open dashboard in target browser
2. Press Ctrl+P (Windows) / Cmd+P (Mac)
3. Set margins to 0.5 inch
4. Verify:
   - All 4 metric cards render with visible borders
   - Text remains black and readable
   - Progress circle and status badge colors preserve
   - No elements cut off

### Screenshot Capture
1. Open browser developer tools (F12)
2. Use device emulation for each resolution
3. Capture screenshot at each resolution
4. Verify spacing and readability

## Responsive Layout Testing

### Viewport Testing Checklist
- [ ] 1920x1080: 4 columns, large fonts
- [ ] 1280x720: 4 columns, medium fonts
- [ ] 1024x768: 2 columns, adjusted fonts
- [ ] 768x1024: 2 columns, tablet
- [ ] 375x667: 1 column, mobile

### Metric Card Verification
- [ ] Cards maintain aspect ratio
- [ ] Text remains centered
- [ ] Progress circle scales proportionally
- [ ] Status badge text doesn't wrap
- [ ] Spacing remains consistent

## Cross-Browser Compatibility

### Chrome/Chromium
- Test at each resolution
- Verify print color accuracy
- Check SVG rendering

### Edge
- Test at each resolution
- Verify print color accuracy
- Check SVG rendering with Edge-specific fixes

## Accessibility Testing
- [ ] Metric labels readable at minimum resolution
- [ ] Progress values clearly visible
- [ ] Status badge colors meet WCAG AA contrast
- [ ] No elements hidden at any resolution
- [ ] ARIA labels present on SVG elements

## Print Color Preservation
Verify in browser print preview:
- Green (#28a745) renders as solid color
- Red (#dc3545) renders as solid color
- Gray (#6c757d) renders as solid color
- Borders remain visible (2px black)
- Text remains black and readable

## Common Issues & Resolution
- If borders disappear in print: Verify `-webkit-print-color-adjust: exact` is set
- If colors wash out: Check browser print settings for "Background graphics"
- If layout breaks at resolution: Test @media query breakpoints
- If SVG doesn't render: Verify viewBox attribute is correct