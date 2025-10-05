using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ImageViewer.Models;

namespace ImageViewer;

public partial class MainWindow : Window
{
    private readonly ImageViewerViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new ImageViewerViewModel();
        DataContext = _viewModel;
        UpdateThemeMenuChecks();
        UpdateSortMenuChecks();

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
        // Handle existing navigation keys
        else
        {
            switch (e.Key)
            {
                case Key.Left:
                    _viewModel.NavigatePrevious();
                    EnsureWindowFocus();
                    e.Handled = true;
                    break;
                case Key.Right:
                    _viewModel.NavigateNextManual();
                    EnsureWindowFocus();
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
}