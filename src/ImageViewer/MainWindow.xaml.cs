using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ImageViewer.Models;

namespace ImageViewer;

public partial class MainWindow : Window
{
    private readonly ImageViewerViewModel _viewModel;

    public static readonly RoutedCommand SortByNameCommand = new RoutedCommand();
    public static readonly RoutedCommand SortByDateCommand = new RoutedCommand();
    public static readonly RoutedCommand SortByRandomCommand = new RoutedCommand();

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new ImageViewerViewModel();
        DataContext = _viewModel;
        UpdateThemeMenuChecks();
        UpdateSortMenuChecks();
        
        // Set up command bindings
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, MenuItem_OpenFolder_Click));
        CommandBindings.Add(new CommandBinding(SortByNameCommand, MenuItem_SortName_Click));
        CommandBindings.Add(new CommandBinding(SortByDateCommand, MenuItem_SortDate_Click));
        CommandBindings.Add(new CommandBinding(SortByRandomCommand, MenuItem_SortRandom_Click));
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, MenuItem_Exit_Click));
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
    }

    private void BtnNext_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigateNext();
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
            GifPlayer.Position = TimeSpan.FromMilliseconds(1);
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
}