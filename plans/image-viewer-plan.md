# .NET 8 C# Image Viewer Project Plan

## Project Structure
- **Framework**: .NET 8 WPF application
- **Location**: `src/ImageViewer/` directory
- **Project Type**: WPF Application targeting .NET 8

## Core Features
1. **Folder Selection**: FolderBrowserDialog to select image directory
2. **Image Display**: Central image viewer with proper scaling and aspect ratio preservation
3. **Navigation**: 
   - Arrow key support (Left/Right for previous/next)
   - Forward/Back buttons in UI
   - Support for images and GIFs
4. **File Filtering**: Support common formats (PNG, JPG, JPEG, GIF, BMP, TIFF)

## Architecture Components
1. **MainWindow.xaml**: Main UI with image display and navigation buttons
2. **ImageViewerViewModel**: MVVM pattern for business logic
3. **ImageService**: Handle file operations and image loading
4. **Models**: Image metadata and navigation state
5. **App.xaml**: Application entry point and global resources

## Key Implementation Details
- Use `System.Windows.Controls.Image` for display
- Implement `INotifyPropertyChanged` for data binding
- Handle GIF animation with `MediaElement` or specialized control
- Memory management for large images
- Keyboard event handling for arrow keys
- Error handling for corrupt/unsupported files

## Development Steps
1. Create .NET 8 WPF project structure
2. Design main window layout
3. Implement folder selection dialog
4. Create image loading and display logic
5. Add keyboard navigation
6. Implement forward/back buttons
7. Add GIF support
8. Error handling and polish