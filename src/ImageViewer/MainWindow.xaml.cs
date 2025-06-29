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
    }

    private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Image Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                _viewModel.LoadImagesFromFolder(dialog.FolderName);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting folder: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPrevious_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigatePrevious();
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigateNext();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Left:
                _viewModel.NavigatePrevious();
                break;
            case Key.Right:
                _viewModel.NavigateNext();
                break;
            case Key.Space:
                if (_viewModel.IsVideo)
                {
                    ToggleVideoPlayback();
                }
                break;
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
        if (_viewModel.IsLoopEnabled)
        {
            VideoPlayer.Position = TimeSpan.Zero;
            VideoPlayer.Play();
        }
    }

    private void GifPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsLoopEnabled)
        {
            GifPlayer.Position = TimeSpan.Zero;
            GifPlayer.Play();
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

    private void UpdateThemeMenuChecks()
    {
        MenuDarkTheme.IsChecked = _viewModel.CurrentTheme == ThemeOption.Dark;
        MenuLightTheme.IsChecked = _viewModel.CurrentTheme == ThemeOption.Light;
    }
}