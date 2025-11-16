# Image & Video Viewer - Feature Roadmap

## Current Features

- Folder browsing with support for images (PNG, JPG, BMP, TIFF, GIF) and videos (MP4)
- Navigation (previous/next with circular wrapping)
- Three themes (Dark, Light, Energy)
- Sorting (by name, date, random)
- Slideshow with configurable timing and loop count
- Transition effects (None, Fade, Slide, Dissolve, Cross Fade)
- Zoom and pan functionality
- Full-screen mode (F11)
- Video/GIF playback controls with looping
- Comprehensive keyboard shortcuts
- Progress bar for slideshow
- Reverse order functionality

## Proposed Features

### High Priority Enhancements

#### 1. File Operations
- Delete current image/video (with confirmation dialog)
- Copy/Move files to another folder
- Rename files
- Open containing folder in Explorer
- Set as desktop wallpaper (for images)

#### 2. Image Editing
- Rotate (90°, 180°, 270°)
- Flip (horizontal/vertical)
- Crop functionality
- Brightness/contrast adjustments
- Save edited images

#### 3. Metadata Display
- EXIF data viewer for photos (camera settings, GPS, date taken)
- File size and dimensions in status bar
- Video duration and resolution
- Thumbnail grid view/filmstrip

#### 4. Enhanced Navigation
- Jump to first/last image (Home/End keys)
- Jump to specific image number (Ctrl+G)
- Thumbnail navigation panel
- Recent folders list
- Bookmarks/favorites

#### 5. Additional Format Support
- WebP images
- SVG vector graphics
- RAW image formats (CR2, NEF, ARW)
- More video formats (AVI, MKV, MOV, WebM)
- HEIC/HEIF images

### Medium Priority Enhancements

#### 6. Comparison Features
- Side-by-side comparison mode
- Before/after view for edits
- Diff highlighting for similar images

#### 7. Organization Tools
- Rating system (1-5 stars)
- Tagging/labeling
- Filter by rating/tags
- Smart collections based on criteria

#### 8. Slideshow Enhancements
- Shuffle mode
- Background music support
- Save slideshow settings per folder

#### 9. Viewing Options
- Fit to width/height options
- Actual size (100%) view
- Background color customization
- Borderless window mode
- Always on top option

#### 10. Image Information Overlay
- Toggle-able info overlay (filename, resolution, date)
- Histogram display
- Color picker tool

### Nice-to-Have Features

#### 11. Performance Optimization
- Preload next/previous images
- Thumbnail caching
- Virtual scrolling for large collections
- Memory usage optimization

#### 12. Export/Share
- Batch export/resize
- Print functionality
- Share to email/social media
- Create contact sheets

#### 13. Search and Filter
- Search by filename
- Filter by file type
- Filter by date range
- Duplicate image detection

#### 14. Accessibility
- Screen reader support
- High contrast themes
- Configurable font sizes
- Color blind friendly themes

#### 15. Advanced Features
- Multi-monitor support
- Dual-pane view
- Compare two folders
- Slideshow export to video
- Plugin system for extensions
- Configurable keyboard shortcuts
- Gesture support (for touch screens)

### Quick Wins (Easiest to Implement)

#### 16. Small UI Improvements
- Double-click to toggle full screen (in addition to F11)
- Context menu (right-click) with common actions
- Drag and drop folder support
- Window size/position persistence
- Zoom percentage display
- Auto-hide controls in full screen (show on mouse move)

## Implementation Priority

### Tier 1 - Quick Wins
1. File operations (delete, rename, open folder)
2. Rotation/flip functionality
3. Metadata display (file size, dimensions)
4. Context menu
5. Drag and drop support
6. Jump to first/last (Home/End keys)

### Tier 2 - High Value Features
1. EXIF data viewer
2. Thumbnail navigation panel
3. Additional format support (WebP, more video formats)
4. Recent folders list
5. Enhanced slideshow (transitions, shuffle)
6. Image editing (crop, brightness/contrast)

### Tier 3 - Advanced Features
1. Rating and tagging system
2. Comparison mode
3. Search and filter
4. Performance optimizations
5. Export/share functionality
6. Multi-monitor support

## Keyboard Shortcuts Reference

### Current Shortcuts
- `Ctrl+O` - Open folder
- `Ctrl+1` - Sort by name
- `Ctrl+2` - Sort by date
- `Ctrl+3` - Sort by random
- `F11` - Toggle full screen
- `Esc` - Exit full screen
- `Left/Right` - Navigate previous/next (or pan when zoomed)
- `Up/Down` - Pan vertically when zoomed
- `+/-` - Zoom in/out
- `0` - Reset zoom
- `Space` - Toggle video playback
- `Alt+F4` - Exit application

### Proposed Additional Shortcuts
- `Delete` - Delete current file
- `F2` - Rename current file
- `Ctrl+E` - Open in Explorer
- `Ctrl+C` - Copy file
- `Ctrl+X` - Cut file
- `Ctrl+V` - Paste file
- `Home` - Jump to first image
- `End` - Jump to last image
- `Ctrl+G` - Go to image number
- `Ctrl+R` - Rotate clockwise
- `Ctrl+Shift+R` - Rotate counter-clockwise
- `Ctrl+I` - Toggle info overlay
- `F5` - Refresh folder
- `Ctrl+T` - Toggle thumbnail panel
- `1-5` - Set rating (when rating system implemented)
