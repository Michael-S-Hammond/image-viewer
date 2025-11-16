using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using ImageViewer.Models;

namespace ImageViewer;

public partial class MainWindow : Window
{
    private readonly ImageViewerViewModel _viewModel;
    private bool _isDragging = false;
    private Point _lastMousePosition;
    private bool _isFullScreen = false;
    private WindowStyle _previousWindowStyle;
    private WindowState _previousWindowState;
    private ResizeMode _previousResizeMode;

    // Track last shown content for cross fade
    private BitmapImage? _lastImageSource;
    private Uri? _lastGifSource;
    private Uri? _lastVideoSource;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new ImageViewerViewModel();
        DataContext = _viewModel;
        UpdateThemeMenuChecks();
        UpdateSortMenuChecks();
        UpdateTransitionMenuChecks();

        // Listen for property changes to trigger transitions
        _viewModel.PropertyChanged += (s, e) =>
        {
            // Only process when a specific source property is set to non-null (not when cleared)
            // This ensures we only run once per image change
            bool shouldProcess = false;
            if (e.PropertyName == nameof(_viewModel.CurrentImageSource) && _viewModel.CurrentImageSource != null)
                shouldProcess = true;
            else if (e.PropertyName == nameof(_viewModel.CurrentGifSource) && _viewModel.CurrentGifSource != null)
                shouldProcess = true;
            else if (e.PropertyName == nameof(_viewModel.CurrentVideoSource) && _viewModel.CurrentVideoSource != null)
                shouldProcess = true;

            if (!shouldProcess)
                return;

            // For cross fade, immediately set ALL viewboxes to opacity 0 FIRST
            if (_viewModel.CurrentTransition == TransitionEffect.CrossFade)
            {
                ImageViewbox.Opacity = 0.0;
                GifViewbox.Opacity = 0.0;
                VideoViewbox.Opacity = 0.0;
            }

            // Capture current state to previous containers before transition (for cross fade)
            CapturePreviousState();

            // For cross fade, apply transition synchronously
            if (_viewModel.CurrentTransition == TransitionEffect.CrossFade)
            {
                // Force layout update first
                ImageDisplayGrid.UpdateLayout();
                // Apply transition immediately
                ApplyTransition();
            }
            else
            {
                // For other transitions, use dispatcher to ensure UI is updated
                Dispatcher.BeginInvoke(new Action(() => ApplyTransition()), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        };

        // Ensure window can receive keyboard focus
        Loaded += (s, e) => EnsureWindowFocus();
    }

    private void MenuItem_OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Open Image Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.LoadImagesFromFolder(dialog.FolderName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening folder: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPrevious_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigatePrevious();
        EnsureWindowFocus();
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigateNextManual();
        EnsureWindowFocus();
    }

    private void BtnReverse_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ReverseOrder();
    }

    private void BtnSlideShow_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleSlideShow();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // Handle F11 for fullscreen toggle
        if (e.Key == Key.F11)
        {
            ToggleFullScreen();
            e.Handled = true;
            return;
        }

        // Handle Esc to exit fullscreen
        if (e.Key == Key.Escape && _isFullScreen)
        {
            ToggleFullScreen();
            e.Handled = true;
            return;
        }

        // Handle Ctrl key combinations
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.O:
                    MenuItem_OpenFolder_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.D1:
                    MenuItem_SortName_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.D2:
                    MenuItem_SortDate_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
                case Key.D3:
                    MenuItem_SortRandom_Click(this, new RoutedEventArgs());
                    e.Handled = true;
                    break;
            }
        }
        // Handle Alt+F4 for exit (though this is usually handled by Windows)
        else if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.F4)
        {
            MenuItem_Exit_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
        // Handle existing navigation keys and zoom/pan
        else
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (_viewModel.IsZoomed)
                    {
                        _viewModel.Pan(50, 0);
                        e.Handled = true;
                    }
                    else
                    {
                        _viewModel.NavigatePrevious();
                        EnsureWindowFocus();
                        e.Handled = true;
                    }
                    break;
                case Key.Right:
                    if (_viewModel.IsZoomed)
                    {
                        _viewModel.Pan(-50, 0);
                        e.Handled = true;
                    }
                    else
                    {
                        _viewModel.NavigateNextManual();
                        EnsureWindowFocus();
                        e.Handled = true;
                    }
                    break;
                case Key.Up:
                    if (_viewModel.IsZoomed)
                    {
                        _viewModel.Pan(0, 50);
                        e.Handled = true;
                    }
                    break;
                case Key.Down:
                    if (_viewModel.IsZoomed)
                    {
                        _viewModel.Pan(0, -50);
                        e.Handled = true;
                    }
                    break;
                case Key.Add:
                case Key.OemPlus:
                    _viewModel.ZoomIn();
                    e.Handled = true;
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    _viewModel.ZoomOut();
                    e.Handled = true;
                    break;
                case Key.D0:
                case Key.NumPad0:
                    _viewModel.ResetZoom();
                    e.Handled = true;
                    break;
                case Key.Space:
                    if (_viewModel.IsVideo)
                    {
                        ToggleVideoPlayback();
                    }
                    break;
            }
        }
    }

    private void BtnPlay_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Play();
    }

    private void BtnPause_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Pause();
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Stop();
    }

    private void ToggleVideoPlayback()
    {
        if (VideoPlayer.CanPause)
        {
            VideoPlayer.Pause();
        }
        else
        {
            VideoPlayer.Play();
        }
    }

    private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Play();
    }

    private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        HandleMediaEnded(() =>
        {
            VideoPlayer.Position = TimeSpan.Zero;
            VideoPlayer.Play();
        });
    }

    private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        HandleMediaEnded(() =>
        {
            GifPlayer.Position = TimeSpan.FromMilliseconds(1);
            GifPlayer.Play();
        });
    }

    private void HandleMediaEnded(Action restartMedia)
    {
        if (_viewModel.IsLoopEnabled)
        {
            // If slideshow is running, let ViewModel handle loop counting
            if (_viewModel.IsSlideShowRunning)
            {
                _viewModel.OnMediaEnded();

                // Only restart if we haven't reached the required loops
                if (_viewModel.CurrentMediaLoopCount < _viewModel.SlideShowLoopCount)
                {
                    restartMedia();
                }
            }
            else
            {
                // Normal loop behavior when slideshow is not running
                restartMedia();
            }
        }
        else if (_viewModel.IsSlideShowRunning)
        {
            // If looping is disabled but slideshow is running, advance immediately
            _viewModel.OnMediaEnded();
        }
    }

    private void MenuItem_Dark_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTheme = ThemeOption.Dark;
        UpdateThemeMenuChecks();
    }

    private void MenuItem_Light_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTheme = ThemeOption.Light;
        UpdateThemeMenuChecks();
    }

    private void MenuItem_Energy_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTheme = ThemeOption.Energy;
        UpdateThemeMenuChecks();
    }

    private void MenuItem_SortName_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentSortOption = SortOption.Name;
        UpdateSortMenuChecks();
    }

    private void MenuItem_SortDate_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentSortOption = SortOption.Date;
        UpdateSortMenuChecks();
    }

    private void MenuItem_SortRandom_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentSortOption = SortOption.Random;
        UpdateSortMenuChecks();
    }

    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItem_FullScreen_Click(object sender, RoutedEventArgs e)
    {
        ToggleFullScreen();
    }

    private void MenuItem_TransitionNone_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTransition = TransitionEffect.None;
        UpdateTransitionMenuChecks();
    }

    private void MenuItem_TransitionFade_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTransition = TransitionEffect.Fade;
        UpdateTransitionMenuChecks();
    }

    private void MenuItem_TransitionSlide_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTransition = TransitionEffect.Slide;
        UpdateTransitionMenuChecks();
    }

    private void MenuItem_TransitionDissolve_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTransition = TransitionEffect.Dissolve;
        UpdateTransitionMenuChecks();
    }

    private void MenuItem_TransitionCrossFade_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTransition = TransitionEffect.CrossFade;
        UpdateTransitionMenuChecks();
    }

    private void UpdateTransitionMenuChecks()
    {
        MenuTransitionNone.IsChecked = _viewModel.CurrentTransition == TransitionEffect.None;
        MenuTransitionFade.IsChecked = _viewModel.CurrentTransition == TransitionEffect.Fade;
        MenuTransitionSlide.IsChecked = _viewModel.CurrentTransition == TransitionEffect.Slide;
        MenuTransitionDissolve.IsChecked = _viewModel.CurrentTransition == TransitionEffect.Dissolve;
        MenuTransitionCrossFade.IsChecked = _viewModel.CurrentTransition == TransitionEffect.CrossFade;
    }

    private void UpdateThemeMenuChecks()
    {
        MenuDarkTheme.IsChecked = _viewModel.CurrentTheme == ThemeOption.Dark;
        MenuLightTheme.IsChecked = _viewModel.CurrentTheme == ThemeOption.Light;
        MenuEnergyTheme.IsChecked = _viewModel.CurrentTheme == ThemeOption.Energy;
    }

    private void UpdateSortMenuChecks()
    {
        MenuSortName.IsChecked = _viewModel.CurrentSortOption == SortOption.Name;
        MenuSortDate.IsChecked = _viewModel.CurrentSortOption == SortOption.Date;
        MenuSortRandom.IsChecked = _viewModel.CurrentSortOption == SortOption.Random;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Handle navigation keys at the preview level to ensure they're always captured
        if (e.Key == Key.Left || e.Key == Key.Right)
        {
            // Let the main KeyDown handler process these
            return;
        }
    }

    private void EnsureWindowFocus()
    {
        // Ensure the window has keyboard focus for navigation
        if (!IsKeyboardFocusWithin)
        {
            Focus();
        }
    }

    private void ToggleFullScreen()
    {
        if (_isFullScreen)
        {
            // Exit fullscreen
            WindowStyle = _previousWindowStyle;
            WindowState = _previousWindowState;
            ResizeMode = _previousResizeMode;

            // Show menu and controls
            Menu.Visibility = Visibility.Visible;
            BottomControls.Visibility = Visibility.Visible;

            _isFullScreen = false;
        }
        else
        {
            // Enter fullscreen
            _previousWindowStyle = WindowStyle;
            _previousWindowState = WindowState;
            _previousResizeMode = ResizeMode;

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;

            // Hide menu and controls
            Menu.Visibility = Visibility.Collapsed;
            BottomControls.Visibility = Visibility.Collapsed;

            _isFullScreen = true;
        }
    }

    private void ImageBorder_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            _viewModel.ZoomIn();
        }
        else
        {
            _viewModel.ZoomOut();
        }
        e.Handled = true;
    }

    private void ImageBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click detected - toggle zoom to fill
            _viewModel.ResetZoom();
            e.Handled = true;
        }
        else if (_viewModel.IsZoomed)
        {
            // Single click while zoomed - start dragging
            _isDragging = true;
            _lastMousePosition = e.GetPosition(ImageBorder);
            ImageBorder.CaptureMouse();
            ImageBorder.Cursor = Cursors.Hand;
            e.Handled = true;
        }
    }

    private void ZoomToFillWindow()
    {
        if (_viewModel.IsStaticImage && MainImage.Source is BitmapSource bitmapSource)
        {
            double imageWidth = bitmapSource.PixelWidth;
            double imageHeight = bitmapSource.PixelHeight;
            double windowWidth = ImageBorder.ActualWidth;
            double windowHeight = ImageBorder.ActualHeight;

            if (imageWidth > 0 && imageHeight > 0 && windowWidth > 0 && windowHeight > 0)
            {
                // Calculate what the Viewbox's current scale is (Uniform fit)
                double viewboxFitScale = Math.Min(windowWidth / imageWidth, windowHeight / imageHeight);

                // Calculate the scale needed to fill (instead of fit)
                double viewboxFillScale = Math.Max(windowWidth / imageWidth, windowHeight / imageHeight);

                // The additional zoom we need is the ratio between fill and fit
                double additionalZoom = viewboxFillScale / viewboxFitScale;

                _viewModel.ZoomLevel = Math.Min(10.0, additionalZoom);
                _viewModel.PanX = 0;
                _viewModel.PanY = 0;
            }
        }
        else if (_viewModel.IsAnimatedGif && GifPlayer.NaturalVideoWidth > 0)
        {
            double videoWidth = GifPlayer.NaturalVideoWidth;
            double videoHeight = GifPlayer.NaturalVideoHeight;
            double windowWidth = ImageBorder.ActualWidth;
            double windowHeight = ImageBorder.ActualHeight;

            if (videoWidth > 0 && videoHeight > 0 && windowWidth > 0 && windowHeight > 0)
            {
                double viewboxFitScale = Math.Min(windowWidth / videoWidth, windowHeight / videoHeight);
                double viewboxFillScale = Math.Max(windowWidth / videoWidth, windowHeight / videoHeight);
                double additionalZoom = viewboxFillScale / viewboxFitScale;

                _viewModel.ZoomLevel = Math.Min(10.0, additionalZoom);
                _viewModel.PanX = 0;
                _viewModel.PanY = 0;
            }
        }
        else if (_viewModel.IsVideo && VideoPlayer.NaturalVideoWidth > 0)
        {
            double videoWidth = VideoPlayer.NaturalVideoWidth;
            double videoHeight = VideoPlayer.NaturalVideoHeight;
            double windowWidth = ImageBorder.ActualWidth;
            double windowHeight = ImageBorder.ActualHeight;

            if (videoWidth > 0 && videoHeight > 0 && windowWidth > 0 && windowHeight > 0)
            {
                double viewboxFitScale = Math.Min(windowWidth / videoWidth, windowHeight / videoHeight);
                double viewboxFillScale = Math.Max(windowWidth / videoWidth, windowHeight / videoHeight);
                double additionalZoom = viewboxFillScale / viewboxFitScale;

                _viewModel.ZoomLevel = Math.Min(10.0, additionalZoom);
                _viewModel.PanX = 0;
                _viewModel.PanY = 0;
            }
        }
    }

    private void ImageBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ImageBorder.ReleaseMouseCapture();
            ImageBorder.Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    private void ImageBorder_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _viewModel.IsZoomed)
        {
            Point currentPosition = e.GetPosition(ImageBorder);
            double deltaX = (currentPosition.X - _lastMousePosition.X) * 2.0;
            double deltaY = (currentPosition.Y - _lastMousePosition.Y) * 2.0;

            _viewModel.Pan(deltaX, deltaY);

            _lastMousePosition = currentPosition;
            e.Handled = true;
        }
    }

    private void ApplyTransition()
    {
        // Determine which element to animate based on content type
        UIElement? targetElement = null;
        if (_viewModel.IsStaticImage)
            targetElement = ImageViewbox;
        else if (_viewModel.IsAnimatedGif)
            targetElement = GifViewbox;
        else if (_viewModel.IsVideo)
            targetElement = VideoViewbox;

        if (targetElement == null)
            return;

        // Remove any existing animations
        targetElement.BeginAnimation(UIElement.OpacityProperty, null);
        targetElement.RenderTransform = new System.Windows.Media.TranslateTransform();
        targetElement.RenderTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, null);

        switch (_viewModel.CurrentTransition)
        {
            case TransitionEffect.None:
                // No animation
                targetElement.Opacity = 1.0;
                break;

            case TransitionEffect.Fade:
                {
                    var fadeAnimation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(1000),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    targetElement.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
                }
                break;

            case TransitionEffect.Slide:
                {
                    targetElement.Opacity = 1.0;
                    var slideTransform = new System.Windows.Media.TranslateTransform();
                    targetElement.RenderTransform = slideTransform;

                    var slideAnimation = new DoubleAnimation
                    {
                        From = ImageBorder.ActualWidth,
                        To = 0.0,
                        Duration = TimeSpan.FromMilliseconds(750),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    slideTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideAnimation);
                }
                break;

            case TransitionEffect.Dissolve:
                {
                    var dissolveAnimation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(750),
                        EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut, Exponent = 3 }
                    };
                    targetElement.BeginAnimation(UIElement.OpacityProperty, dissolveAnimation);
                }
                break;

            case TransitionEffect.CrossFade:
                {
                    // Determine which previous element to fade out (just check visibility, not content type)
                    UIElement? previousElement = null;
                    if (PreviousImageViewbox.Visibility == Visibility.Visible)
                        previousElement = PreviousImageViewbox;
                    else if (PreviousGifViewbox.Visibility == Visibility.Visible)
                        previousElement = PreviousGifViewbox;
                    else if (PreviousVideoViewbox.Visibility == Visibility.Visible)
                        previousElement = PreviousVideoViewbox;

                    // Immediately set opacity to 0 to prevent flash
                    targetElement.Opacity = 0.0;

                    // Fade in the new image
                    var fadeInAnimation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(750),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                    };
                    targetElement.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);

                    // Fade out the previous image if it exists
                    if (previousElement != null)
                    {
                        var fadeOutAnimation = new DoubleAnimation
                        {
                            From = 1.0,
                            To = 0.0,
                            Duration = TimeSpan.FromMilliseconds(750),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                        };
                        fadeOutAnimation.Completed += (s, e) =>
                        {
                            // Hide and clear previous elements after animation
                            HidePreviousElements();
                        };
                        previousElement.BeginAnimation(UIElement.OpacityProperty, fadeOutAnimation);
                    }
                    else
                    {
                        // No previous element, just fade in (first image)
                        // Clean up will happen on next transition
                    }
                }
                break;
        }
    }

    private void CapturePreviousState()
    {
        // Only capture if we're using cross fade transition
        if (_viewModel.CurrentTransition != TransitionEffect.CrossFade)
        {
            HidePreviousElements();
            UpdateLastShownContent();
            return;
        }

        // Use the tracked last shown content for cross fade
        // This is captured BEFORE the new content is bound
        if (_lastImageSource != null)
        {
            PreviousImage.Source = _lastImageSource;
            PreviousImageViewbox.Visibility = Visibility.Visible;
            PreviousImageViewbox.Opacity = 1.0;
            // Hide other previous elements
            PreviousGifViewbox.Visibility = Visibility.Collapsed;
            PreviousVideoViewbox.Visibility = Visibility.Collapsed;
        }
        else if (_lastGifSource != null)
        {
            PreviousGifPlayer.Source = _lastGifSource;
            PreviousGifViewbox.Visibility = Visibility.Visible;
            PreviousGifViewbox.Opacity = 1.0;
            // Hide other previous elements
            PreviousImageViewbox.Visibility = Visibility.Collapsed;
            PreviousVideoViewbox.Visibility = Visibility.Collapsed;
        }
        else if (_lastVideoSource != null)
        {
            PreviousVideoPlayer.Source = _lastVideoSource;
            PreviousVideoViewbox.Visibility = Visibility.Visible;
            PreviousVideoViewbox.Opacity = 1.0;
            // Hide other previous elements
            PreviousImageViewbox.Visibility = Visibility.Collapsed;
            PreviousGifViewbox.Visibility = Visibility.Collapsed;
        }
        else
        {
            // No previous content to capture (first image)
            HidePreviousElements();
        }

        // Update tracking for next transition
        UpdateLastShownContent();
    }

    private void UpdateLastShownContent()
    {
        // Store current content for next transition
        _lastImageSource = _viewModel.CurrentImageSource;
        _lastGifSource = _viewModel.CurrentGifSource;
        _lastVideoSource = _viewModel.CurrentVideoSource;
    }

    private void HidePreviousElements()
    {
        PreviousImageViewbox.Visibility = Visibility.Collapsed;
        PreviousGifViewbox.Visibility = Visibility.Collapsed;
        PreviousVideoViewbox.Visibility = Visibility.Collapsed;

        PreviousImage.Source = null;
        PreviousGifPlayer.Source = null;
        PreviousVideoPlayer.Source = null;
    }
}