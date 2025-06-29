# Image Viewer

A .NET 8 WPF application for viewing images with navigation capabilities.

## Features

- **Folder Selection**: Browse and select folders containing images
- **Image Display**: View images with proper scaling and aspect ratio preservation
- **Navigation**: Navigate through images using arrow keys or UI buttons
- **Format Support**: Supports PNG, JPG, JPEG, GIF, BMP, TIFF formats
- **GIF Animation**: Animated GIF support with MediaElement
- **Image Information**: Display file name, size, and current position

## Requirements

- .NET 8.0 or later
- Windows OS (WPF application)

## Building and Running

```bash
dotnet build
dotnet run
```

## Usage

1. Click "Select Folder" to choose a directory containing images
2. Use arrow keys (Left/Right) or navigation buttons to browse images
3. GIF files will automatically animate when displayed
4. Image information is shown in the status bar

## Architecture

- **MVVM Pattern**: Clean separation of concerns
- **Services**: ImageService handles file operations
- **Models**: ImageInfo for metadata
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