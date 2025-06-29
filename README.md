# Image & Video Viewer

A .NET 8 WPF application for viewing images and videos with navigation capabilities.

## Features

- **Folder Selection**: Browse and select folders containing images and videos
- **Image Display**: View images with proper scaling and aspect ratio preservation
- **Navigation**: Navigate through media files using arrow keys or UI buttons
- **Sorting Options**: Sort media files by Name, Date Modified, or Random order
- **Format Support**: Supports PNG, JPG, JPEG, GIF, BMP, TIFF, MP4 formats
- **GIF Animation**: Animated GIF support with MediaElement and loop option
- **Video Playback**: MP4 video support with play/pause/stop controls and loop option
- **Loop Control**: Toggle looping for animated GIFs and videos
- **Media Information**: Display file name, size, and current position

## Requirements

- .NET 8.0 or later
- Windows OS (WPF application)

## Building and Running

```bash
dotnet build
dotnet run
```

## Usage

1. Click "Select Folder" to choose a directory containing images and videos
2. Use arrow keys (Left/Right) or navigation buttons to browse media files
3. GIF files will automatically animate when displayed
4. MP4 videos will auto-play when selected
5. Use video controls (Play/Pause/Stop) for MP4 playback
6. Press Spacebar to toggle video play/pause
7. Use the "Sort by" dropdown to change sorting: Name (alphabetical), Date (chronological), or Random
8. Use the "Loop" checkbox to enable/disable looping for GIFs and videos
9. Media information is shown in the status bar

## Architecture

- **MVVM Pattern**: Clean separation of concerns
- **Services**: ImageService handles file operations for images and videos
- **Models**: ImageInfo for metadata, SortOption enum for sorting methods
- **Media Support**: Separate handling for static images, animated GIFs, and MP4 videos
- **Sorting System**: Dynamic sorting with index preservation during sort changes
- **Error Handling**: Graceful handling of corrupt files and errors

## Project Structure

```
src/ImageViewer/
├── ImageViewer.csproj
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── Models/
│   └── ImageInfo.cs
├── Services/
│   └── ImageService.cs
└── ViewModels/
    └── ImageViewerViewModel.cs
```