using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using KioskBrowser.WebView;
using Microsoft.Web.WebView2.Core;

namespace KioskBrowser;

public partial class MainWindow
{
    private readonly WebViewComponent _webViewComponent;

    public MainWindow()
    {
        InitializeComponent();

        _webViewComponent = new WebViewComponent();
        
        DataContext =  new MainViewModel(_webViewComponent, CloseWindow);
    }

    private static string CacheFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KioskBrowser");

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        var args = Environment.GetCommandLineArgs();
            
        SetButtonStates();
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        KeyDown += (_, eventArgs) => {
            if (eventArgs.Key == Key.Escape && Titlebar.Visibility != Visibility.Visible)
                CloseWindow(); };
            
        if (!_webViewComponent.IsInstalled)
            return;
            
        var args = Environment.GetCommandLineArgs();

        if (args.Length < 2)
        {
            Shutdown("Information","No parameters. Browser window will close.");
            return;
        }
        var url = args[1];

        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, CacheFolderPath);
            await WebView.EnsureCoreWebView2Async(environment);
                
            WebView.Source = new UriBuilder(url).Uri;
        }
        catch (Exception)
        {
            Shutdown("Error Occurred", "An error occurred when starting the browser. Browser window will close.");
        }
    }


    private void CloseWindow()
    {
        if( Titlebar.Visibility != Visibility.Visible)
            Application.Current.Shutdown();
    }

    private void Shutdown(string caption, string message)
    {
        MessageBox.Show(this, message, caption);
        Application.Current.Shutdown();
    }

    private void Hyperlink_OnClick(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo {
            FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703",
            UseShellExecute = true});

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        SetButtonStates();
    }

    private void SetButtonStates()
    {
        restoreButton.Visibility = WindowState == WindowState.Maximized ? Visibility.Visible : Visibility.Collapsed;
        maximizeButton.Visibility = WindowState == WindowState.Maximized ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnCloseButtonClick(object sender, RoutedEventArgs e) => Close();
}