using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImageViewer.Models;
using ImageViewer.Services;
using ImageViewer.Helpers;

namespace ImageViewer;

public class ImageViewerViewModel : INotifyPropertyChanged, IDisposable
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
    private bool _isSlideShowRunning = false;
    private int _slideShowSeconds = 10;
    private int _slideShowLoopCount = 1;
    private DispatcherTimer? _slideShowTimer;
    private int _currentMediaLoopCount = 0;
    private double _progressValue = 0;
    private ProgressAnimationHelper? _progressAnimationHelper;
    private Action<double>? _progressUpdateCallback;
    private double _zoomLevel = 1.0;
    private double _panX = 0;
    private double _panY = 0;
    private Stretch _stretchMode = Stretch.Uniform;

    public ImageViewerViewModel()
    {
        _imageService = new ImageService();
        ApplyTheme(); // Apply default theme on initialization
        InitializeSlideShowTimer();
        InitializeProgressAnimation();
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

    public bool HasLoopableContent => IsAnimatedGif || IsVideo;

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

    public bool HasImages => _images.Count > 0;

    public bool CanNavigatePrevious => _images.Count > 0;
    public bool CanNavigateNext => _images.Count > 0;

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

    public bool IsSlideShowRunning
    {
        get => _isSlideShowRunning;
        private set
        {
            _isSlideShowRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SlideShowButtonText));
            OnPropertyChanged(nameof(IsProgressBarVisible));
        }
    }

    public int SlideShowSeconds
    {
        get => _slideShowSeconds;
        set
        {
            if (_slideShowSeconds != value && value > 0)
            {
                _slideShowSeconds = value;
                OnPropertyChanged();

                // Update timer interval if slideshow is running
                if (_isSlideShowRunning && _slideShowTimer != null)
                {
                    _slideShowTimer.Interval = TimeSpan.FromSeconds(_slideShowSeconds);

                    // Restart progress animation for static images with new duration
                    if (IsStaticImage)
                    {
                        StartProgressForCurrentImage();
                    }
                }
            }
        }
    }

    public int SlideShowLoopCount
    {
        get => _slideShowLoopCount;
        set
        {
            if (_slideShowLoopCount != value && value > 0)
            {
                _slideShowLoopCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProgressBarVisible));
            }
        }
    }

    public string SlideShowButtonText => _isSlideShowRunning ? "⏹ Stop Slideshow" : "▶ Start Slideshow";

    public int CurrentMediaLoopCount
    {
        get => _currentMediaLoopCount;
        private set
        {
            _currentMediaLoopCount = value;
            OnPropertyChanged();
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        set
        {
            _progressValue = value;
            OnPropertyChanged();
        }
    }

    public bool IsProgressBarVisible => _isSlideShowRunning &&
        (IsStaticImage || (HasLoopableContent && SlideShowLoopCount > 1));

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (_zoomLevel != value && value >= 0.1 && value <= 10.0)
            {
                _zoomLevel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsZoomed));
            }
        }
    }

    public double PanX
    {
        get => _panX;
        set
        {
            _panX = value;
            OnPropertyChanged();
        }
    }

    public double PanY
    {
        get => _panY;
        set
        {
            _panY = value;
            OnPropertyChanged();
        }
    }

    public bool IsZoomed => _zoomLevel > 1.0;

    public Stretch StretchMode
    {
        get => _stretchMode;
        set
        {
            if (_stretchMode != value)
            {
                _stretchMode = value;
                OnPropertyChanged();
            }
        }
    }

    public void ToggleZoomToFill()
    {
        if (_zoomLevel > 1.0)
        {
            // If zoomed in, reset to fit
            ResetZoom();
        }
        else
        {
            // Calculate zoom level needed to fill the window
            // This will be implemented in the code-behind where we have access to window dimensions
            // For now, just zoom to a reasonable fill level
            ZoomLevel = 1.5;
            PanX = 0;
            PanY = 0;
        }
    }

    public void ZoomIn()
    {
        ZoomLevel = Math.Min(10.0, _zoomLevel * 1.2);
    }

    public void ZoomOut()
    {
        ZoomLevel = Math.Max(0.1, _zoomLevel / 1.2);
    }

    public void ResetZoom()
    {
        ZoomLevel = 1.0;
        PanX = 0;
        PanY = 0;
    }

    public void Pan(double deltaX, double deltaY)
    {
        if (_zoomLevel > 1.0)
        {
            PanX += deltaX;
            PanY += deltaY;
        }
    }

    public void OnMediaEnded()
    {
        if (!_isSlideShowRunning) return;

        CurrentMediaLoopCount++;

        // Update progress for looping media
        if (HasLoopableContent && SlideShowLoopCount > 1)
        {
            ProgressValue = (double)CurrentMediaLoopCount / SlideShowLoopCount * 100;
        }

        // Check if we've completed the required loops for this media
        if (CurrentMediaLoopCount >= SlideShowLoopCount)
        {
            // Move to next image - timer will restart in StartProgressForCurrentImage
            NavigateNext();
        }
    }

    public void LoadImagesFromFolder(string folderPath)
    {
        StopSlideShow(); // Stop slideshow when loading new folder
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
        if (_images.Count == 0) return;

        StopSlideShow(); // Stop slideshow on manual navigation
        
        if (_currentImageIndex > 0)
        {
            _currentImageIndex--;
        }
        else
        {
            _currentImageIndex = _images.Count - 1;
        }
        
        LoadCurrentImage();
        UpdateNavigationProperties();
    }

    public void NavigateNext()
    {
        NavigateNext(false);
    }

    public void NavigateNextManual()
    {
        NavigateNext(true);
    }

    private void NavigateNext(bool stopSlideShow)
    {
        if (_images.Count == 0) return;

        if (stopSlideShow)
        {
            StopSlideShow(); // Stop slideshow on manual navigation
        }

        if (_currentImageIndex < _images.Count - 1)
        {
            _currentImageIndex++;
        }
        else
        {
            _currentImageIndex = 0;
        }

        LoadCurrentImage();
        UpdateNavigationProperties();
    }

    public void ReverseOrder()
    {
        if (_images.Count <= 1) return;

        StopSlideShow(); // Stop slideshow when reversing order

        var currentFileName = _currentImageIndex >= 0 && _currentImageIndex < _images.Count 
            ? _images[_currentImageIndex].FileName 
            : string.Empty;

        _images.Reverse();

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

    public void ToggleSlideShow()
    {
        if (_isSlideShowRunning)
        {
            StopSlideShow();
        }
        else
        {
            StartSlideShow();
        }
    }

    public void StartSlideShow()
    {
        if (_images.Count <= 1 || _slideShowTimer == null || _progressAnimationHelper == null) return;

        IsSlideShowRunning = true;
        _slideShowTimer.Interval = TimeSpan.FromSeconds(_slideShowSeconds);
        _slideShowTimer.Start();

        StartProgressForCurrentImage();
    }

    public void StopSlideShow()
    {
        if (_slideShowTimer == null || _progressAnimationHelper == null) return;

        IsSlideShowRunning = false;
        _slideShowTimer.Stop();
        _progressAnimationHelper.ResetValue();
        ProgressValue = 0;
    }

    private void StartProgressForCurrentImage()
    {
        if (_progressAnimationHelper == null) return;

        _progressAnimationHelper.ResetValue();
        ProgressValue = 0;

        if (IsStaticImage)
        {
            // Start smooth animation from 0 to 100 over the slideshow duration
            var duration = TimeSpan.FromSeconds(_slideShowSeconds);
            _progressAnimationHelper.StartAnimation(0, 100, duration);

            // Resume timer for static images
            if (_slideShowTimer != null && _isSlideShowRunning)
            {
                _slideShowTimer.Start();
            }
        }
        else if (HasLoopableContent && SlideShowLoopCount > 1)
        {
            // For looping media, progress is based on completed loops
            ProgressValue = (double)CurrentMediaLoopCount / SlideShowLoopCount * 100;

            // Pause timer while media is playing - it will advance via OnMediaEnded
            if (_slideShowTimer != null)
            {
                _slideShowTimer.Stop();
            }
        }
        else
        {
            // For single-loop media, pause timer while media is playing
            if (_slideShowTimer != null)
            {
                _slideShowTimer.Stop();
            }
        }
    }

    private void InitializeSlideShowTimer()
    {
        try
        {
            _slideShowTimer = new DispatcherTimer();
            _slideShowTimer.Tick += SlideShowTimer_Tick;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error initializing slideshow timer: {ex.Message}", "Initialization Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void InitializeProgressAnimation()
    {
        _progressAnimationHelper = new ProgressAnimationHelper();
        _progressUpdateCallback = value => ProgressValue = value;
        _progressAnimationHelper.SetUpdateCallback(_progressUpdateCallback);
    }

    private void SlideShowTimer_Tick(object? sender, EventArgs e)
    {
        // Only advance for static images - GIFs and videos handle their own timing
        if (IsStaticImage)
        {
            NavigateNext();
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
            ThemeOption.Energy => new Uri("/Themes/EnergyTheme.xaml", UriKind.Relative),
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

                // Reset loop count when loading new media
                CurrentMediaLoopCount = 0;

                // Reset zoom and pan when changing images
                ResetZoom();

                // Start progress tracking if slideshow is running
                if (_isSlideShowRunning)
                {
                    StartProgressForCurrentImage();
                }

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
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(CanNavigatePrevious));
        OnPropertyChanged(nameof(CanNavigateNext));
        OnPropertyChanged(nameof(IsAnimatedGif));
        OnPropertyChanged(nameof(IsVideo));
        OnPropertyChanged(nameof(IsStaticImage));
        OnPropertyChanged(nameof(HasLoopableContent));
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(IsProgressBarVisible));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _slideShowTimer?.Stop();
                _slideShowTimer = null;
                _progressAnimationHelper?.StopAnimation();
                _progressAnimationHelper = null;
            }

            _disposed = true;
        }
    }
}