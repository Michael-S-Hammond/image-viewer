using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using ImageViewer.Models;
using ImageViewer.Services;

namespace ImageViewer;

public class ImageViewerViewModel : INotifyPropertyChanged
{
    private readonly ImageService _imageService;
    private List<ImageInfo> _images = new();
    private int _currentImageIndex = -1;
    private BitmapImage? _currentImageSource;
    private Uri? _currentGifSource;
    private Uri? _currentVideoSource;
    private string _currentFolderPath = string.Empty;
    private bool _isLoopEnabled = true;
    private SortOption _currentSortOption = SortOption.Name;
    private ThemeOption _currentTheme = ThemeOption.Dark;

    public ImageViewerViewModel()
    {
        _imageService = new ImageService();
        ApplyTheme(); // Apply default theme on initialization
    }

    public BitmapImage? CurrentImageSource
    {
        get => _currentImageSource;
        private set
        {
            _currentImageSource = value;
            OnPropertyChanged();
        }
    }

    public Uri? CurrentGifSource
    {
        get => _currentGifSource;
        private set
        {
            _currentGifSource = value;
            OnPropertyChanged();
        }
    }

    public Uri? CurrentVideoSource
    {
        get => _currentVideoSource;
        private set
        {
            _currentVideoSource = value;
            OnPropertyChanged();
        }
    }

    public bool IsAnimatedGif => _currentImageIndex >= 0 && _currentImageIndex < _images.Count 
        && _images[_currentImageIndex].Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase);

    public bool IsVideo => _currentImageIndex >= 0 && _currentImageIndex < _images.Count 
        && _images[_currentImageIndex].Extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase);

    public bool IsStaticImage => !IsAnimatedGif && !IsVideo;

    public string CurrentFolderPath
    {
        get => _currentFolderPath;
        private set
        {
            _currentFolderPath = value;
            OnPropertyChanged();
        }
    }

    public int CurrentImageIndex
    {
        get => _currentImageIndex + 1;
        private set => OnPropertyChanged();
    }

    public int TotalImages => _images.Count;

    public bool CanNavigatePrevious => _currentImageIndex > 0;
    public bool CanNavigateNext => _currentImageIndex < _images.Count - 1;

    public string WindowTitle => _currentImageIndex >= 0 && _currentImageIndex < _images.Count 
        ? $"Image Viewer - {_images[_currentImageIndex].FileName}" 
        : "Image Viewer";

    public bool IsLoopEnabled
    {
        get => _isLoopEnabled;
        set
        {
            _isLoopEnabled = value;
            OnPropertyChanged();
        }
    }

    public SortOption CurrentSortOption
    {
        get => _currentSortOption;
        set
        {
            if (_currentSortOption != value)
            {
                _currentSortOption = value;
                OnPropertyChanged();
                ApplySorting();
            }
        }
    }

    public ThemeOption CurrentTheme
    {
        get => _currentTheme;
        set
        {
            if (_currentTheme != value)
            {
                _currentTheme = value;
                OnPropertyChanged();
                ApplyTheme();
            }
        }
    }

    public void LoadImagesFromFolder(string folderPath)
    {
        try
        {
            _images = _imageService.LoadImagesFromFolder(folderPath);
            CurrentFolderPath = folderPath;
            
            if (_images.Count > 0)
            {
                _images = SortImages(_images, _currentSortOption);
                _currentImageIndex = 0;
                LoadCurrentImage();
            }
            else
            {
                _currentImageIndex = -1;
                CurrentImageSource = null;
                CurrentGifSource = null;
                CurrentVideoSource = null;
            }

            UpdateNavigationProperties();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading images: {ex.Message}", "Error", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    public void NavigatePrevious()
    {
        if (CanNavigatePrevious)
        {
            _currentImageIndex--;
            LoadCurrentImage();
            UpdateNavigationProperties();
        }
    }

    public void NavigateNext()
    {
        if (CanNavigateNext)
        {
            _currentImageIndex++;
            LoadCurrentImage();
            UpdateNavigationProperties();
        }
    }

    private void ApplySorting()
    {
        if (_images.Count == 0) return;

        var currentFileName = _currentImageIndex >= 0 && _currentImageIndex < _images.Count 
            ? _images[_currentImageIndex].FileName 
            : string.Empty;

        _images = SortImages(_images, _currentSortOption);

        if (!string.IsNullOrEmpty(currentFileName))
        {
            var newIndex = _images.FindIndex(img => img.FileName == currentFileName);
            if (newIndex >= 0)
            {
                _currentImageIndex = newIndex;
            }
            else
            {
                _currentImageIndex = 0;
            }
        }
        else
        {
            _currentImageIndex = 0;
        }

        LoadCurrentImage();
        UpdateNavigationProperties();
    }

    private List<ImageInfo> SortImages(List<ImageInfo> images, SortOption sortOption)
    {
        return sortOption switch
        {
            SortOption.Name => images.OrderBy(x => x.FileName, StringComparer.OrdinalIgnoreCase).ToList(),
            SortOption.Date => images.OrderBy(x => x.DateModified).ToList(),
            SortOption.Random => images.OrderBy(x => Guid.NewGuid()).ToList(),
            _ => images
        };
    }

    private void ApplyTheme()
    {
        var app = System.Windows.Application.Current;
        if (app == null) return;

        var themeUri = _currentTheme switch
        {
            ThemeOption.Dark => new Uri("/Themes/DarkTheme.xaml", UriKind.Relative),
            ThemeOption.Light => new Uri("/Themes/LightTheme.xaml", UriKind.Relative),
            _ => new Uri("/Themes/DarkTheme.xaml", UriKind.Relative)
        };

        // Remove existing theme dictionaries
        var existingThemes = app.Resources.MergedDictionaries
            .Where(d => d.Source?.OriginalString.Contains("Theme") == true)
            .ToList();

        foreach (var theme in existingThemes)
        {
            app.Resources.MergedDictionaries.Remove(theme);
        }

        try
        {
            var newTheme = new System.Windows.ResourceDictionary { Source = themeUri };
            app.Resources.MergedDictionaries.Add(newTheme);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error applying theme: {ex.Message}", "Theme Error", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void LoadCurrentImage()
    {
        if (_currentImageIndex >= 0 && _currentImageIndex < _images.Count)
        {
            try
            {
                var currentImage = _images[_currentImageIndex];
                
                if (currentImage.Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    CurrentGifSource = new Uri(currentImage.FilePath);
                    CurrentImageSource = null;
                    CurrentVideoSource = null;
                }
                else if (currentImage.Extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
                {
                    CurrentVideoSource = new Uri(currentImage.FilePath);
                    CurrentImageSource = null;
                    CurrentGifSource = null;
                }
                else
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(currentImage.FilePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    
                    CurrentImageSource = bitmap;
                    CurrentGifSource = null;
                    CurrentVideoSource = null;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading media: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                CurrentImageSource = null;
                CurrentGifSource = null;
                CurrentVideoSource = null;
            }
        }
    }

    private void UpdateNavigationProperties()
    {
        OnPropertyChanged(nameof(CurrentImageIndex));
        OnPropertyChanged(nameof(TotalImages));
        OnPropertyChanged(nameof(CanNavigatePrevious));
        OnPropertyChanged(nameof(CanNavigateNext));
        OnPropertyChanged(nameof(IsAnimatedGif));
        OnPropertyChanged(nameof(IsVideo));
        OnPropertyChanged(nameof(IsStaticImage));
        OnPropertyChanged(nameof(WindowTitle));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}