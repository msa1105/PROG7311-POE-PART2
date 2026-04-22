# UI Icon Reference - GLMS

## Overview
All emojis have been replaced with professional Bootstrap Icons (v1.11.3) for a clean, enterprise-grade interface.

## Bootstrap Icons CDN
```html
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
```

## Icon Mapping

### Navigation & Branding
| Location | Old | New | Icon Class |
|----------|-----|-----|------------|
| Navbar Brand | ⬡ | Hexagon Icon | `bi-hexagon` |

### Dashboard (Home/Index.cshtml)
| Element | Old | New | Icon Class |
|---------|-----|-----|------------|
| Clients Card | → | People Icon | `bi-people` |
| Contracts Card | → | Document Check Icon | `bi-file-earmark-check` |
| Service Requests Card | → | Clipboard Check Icon | `bi-clipboard-check` |
| New Client Button | 👤 | Person Plus Icon | `bi-person-plus` |
| New Contract Button | 📄 | File Text Icon | `bi-file-earmark-text` |
| New Service Request Button | 🔧 | Tools Icon | `bi-tools` |

### List Views
| Action | Icon Class | Purpose |
|--------|------------|---------|
| View/Details | `bi-eye` | View record details |
| Edit | `bi-pencil` | Edit record |
| Delete | `bi-trash` | Delete record |
| View PDF | `bi-file-pdf` | View contract PDF |
| Add New | `bi-plus-circle` | Create new record |

### Page Headers
| Page | Icon Class | Title |
|------|------------|-------|
| Clients | `bi-people` | Clients |
| Contracts | `bi-file-earmark-check` | Contracts |
| Service Requests | `bi-clipboard-check` | Service Requests |

## Icon Sizes
- **Navigation icons**: Default size (16px)
- **Dashboard stat cards**: 3rem (48px)
- **Quick nav buttons**: 1.5rem (24px)
- **Action buttons**: Default size (16px)

## Icon Colors
Icons inherit color from their parent elements using CSS custom properties:
- Primary text: `var(--text-primary)`
- Muted text: `var(--text-muted)`
- Accent: `var(--accent)`
- Success: `var(--success)`

## Documentation Updates
All documentation files have been updated to remove emojis and use professional text markers:

### Text Markers Used
| Old Emoji | New Marker | Usage |
|-----------|------------|-------|
| ✅ | [PASS] / [DONE] / [COMPLETE] | Completed items |
| 🎉 | [SUCCESS] | Success status |
| 📋 | [CHECKLIST] | Checklist sections |
| 📊 | [DATA] / [STATS] | Data/statistics sections |
| 🚀 | [DEPLOY] / [NEXT] | Deployment/next steps |
| 📝 | [NOTE] | Notes |
| 🔧 | [CONFIG] / [TOOLS] | Configuration/tools |
| 🐛 | [DEBUG] | Debugging/issues |
| 📞 | [SUPPORT] | Support sections |
| 🎓 | [ACADEMIC] | Academic content |
| 🎯 | [GOAL] / [TARGET] | Goals/targets |
| 💡 | [TIP] | Tips |
| 🌟 | [STAR] / [EXCELLENT] | Excellence markers |
| ❌ | [FAIL] | Failed items |

## Files Modified

### View Files
1. `Views/Shared/_Layout.cshtml` - Added Bootstrap Icons CDN, updated navbar
2. `Views/Home/Index.cshtml` - Replaced all dashboard emojis with icons
3. `Views/Clients/Index.cshtml` - Added icons to headers and action buttons
4. `Views/Contracts/Index.cshtml` - Added icons to headers and action buttons
5. `Views/ServiceRequests/Index.cshtml` - Added icons to headers and action buttons

### Documentation Files
1. `README.md` - Removed emojis, replaced with [SECTION] markers
2. `QUICK_START_GUIDE.md` - Removed emojis, replaced with professional markers
3. `IMPLEMENTATION_SUMMARY.md` - Removed emojis, replaced with status markers
4. `QA_CHECKLIST_COMPLETION_REPORT.md` - Removed emojis, replaced with [PASS]/[FAIL] markers
5. `Database_Schema_README.md` - Removed emojis

## Icon Usage Guidelines

### Adding New Icons
```html
<!-- Inline icon -->
<i class="bi bi-icon-name"></i>

<!-- Icon with spacing -->
<i class="bi bi-icon-name me-2"></i>

<!-- Icon with custom size -->
<i class="bi bi-icon-name" style="font-size: 1.5rem;"></i>
```

### Common Patterns
```html
<!-- Button with icon -->
<a href="#" class="btn btn-primary">
    <i class="bi bi-plus-circle me-1"></i>Add Item
</a>

<!-- Icon-only button -->
<button class="btn btn-sm btn-outline-secondary">
    <i class="bi bi-pencil"></i>
</button>

<!-- Header with icon -->
<h1><i class="bi bi-people me-2"></i>Page Title</h1>
```

## Icon Reference
All icons used are from Bootstrap Icons. Full reference:
https://icons.getbootstrap.com/

### Icons Used in GLMS
- `bi-hexagon` - Brand/logo
- `bi-people` - Clients/users
- `bi-person-plus` - Add client
- `bi-file-earmark-check` - Contracts
- `bi-file-earmark-text` - Document
- `bi-file-pdf` - PDF file
- `bi-clipboard-check` - Service requests
- `bi-tools` - Tools/services
- `bi-plus-circle` - Add/create
- `bi-eye` - View/details
- `bi-pencil` - Edit
- `bi-trash` - Delete

## Benefits
1. **Professional Appearance**: Icons are scalable and consistent
2. **Accessibility**: Screen reader friendly with proper ARIA labels
3. **Performance**: CDN-delivered, cached icon font
4. **Consistency**: Bootstrap Icons match Bootstrap framework
5. **Maintainability**: Well-documented icon library with extensive options
6. **Browser Compatibility**: Works across all modern browsers

## Migration Complete
All emojis have been successfully removed from both the application UI and documentation. The system now uses a professional, enterprise-grade icon system that aligns with modern web development best practices.
