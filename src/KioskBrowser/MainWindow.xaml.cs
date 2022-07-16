﻿using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using KioskBrowser.WebView;
using Microsoft.Web.WebView2.Core;

namespace KioskBrowser;

public partial class MainWindow
{
    private readonly WebViewComponent _webViewComponent;

    public static class Globals
    {
        public static readonly String APP_ID = "your app id"; // Unmodifiable
        public static readonly String APP_FOLDER_NAME = "your app folder name"; // Unmodifiable
        public static readonly String APP_NAME = "your app name"; // Unmodifiable
        public static readonly String TENANT_ID = "your teant id"; // Unmodifiable
        public static readonly String APP_USERAGENT = "Your useragent here";
        public static readonly String BASE_URL = "https://apps.powerapps.com/play/" + APP_ID + "?tenantId=" + TENANT_ID + "&source=iframe&hidenavbar=true&"; // Unmodifiable
        public static readonly String APP_REQUEST_LANG = "en-AU";
        public static readonly String USER_DATA_FOLDER = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_FOLDER_NAME);
    }

    public MainWindow()
    {
        InitializeComponent();

        _webViewComponent = new WebViewComponent();
        
        DataContext =  new MainViewModel(_webViewComponent, CloseWindow);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        var args = Environment.GetCommandLineArgs();
            
        SetButtonStates();
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        var options = new CoreWebView2EnvironmentOptions
        {
            AllowSingleSignOnUsingOSPrimaryAccount = true,
            Language = $"{Globals.APP_REQUEST_LANG}"
        };

        if (!_webViewComponent.IsInstalled)
            return;
            
        var environment = await CoreWebView2Environment.CreateAsync(null, Globals.USER_DATA_FOLDER, options).ConfigureAwait(true);

        WebView.Loaded += async (sender, e) =>
        {
            await WebView.EnsureCoreWebView2Async(environment).ConfigureAwait(true);
            WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            WebView.CoreWebView2.Settings.UserAgent = $"{Globals.APP_USERAGENT}";
            WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            WebView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
            WebView.CoreWebView2.Settings.IsWebMessageEnabled = false;
            WebView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
            WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
        };
        var args = "";
        if (Environment.GetCommandLineArgs().Length > 1)
        {
            args = Regex.Replace(Environment.GetCommandLineArgs()[1], @"kioskbrowser:\b", "", RegexOptions.IgnoreCase);
            //Add code to check for gpu pram
            if (args.StartsWith("gpu"))
            {
                this.WebView.Source = new System.Uri($"edge://gpu", System.UriKind.Absolute);
                return;
            }
            else
            {
                this.WebView.Source = new System.Uri($"{Globals.BASE_URL}{args}", System.UriKind.Absolute);
                return;
            }
            return;
        }
        else
        {
            this.WebView.Source = new System.Uri($"{Globals.BASE_URL}", System.UriKind.Absolute);
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