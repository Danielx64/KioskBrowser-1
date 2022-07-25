using System.Diagnostics;
using System.IO;
using System.Threading;
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
		public static readonly String APP_ICON = "/Images/globe.png"; // Unmodifiable
		public static readonly String APP_FOLDER_NAME = "your app folder name"; // Unmodifiable
		public static readonly String APP_NAME = "your app name"; // Unmodifiable
		public static readonly String TENANT_ID = "your teant id"; // Unmodifiable
		public static readonly String APP_USERAGENT = "Your useragent here";
		public static readonly String URI_SCHEMA = "kioskbrowser";
		public static readonly String BASE_URL = "https://apps.powerapps.com/play/" + APP_ID + "?tenantId=" + TENANT_ID + "&source=iframe&hidenavbar=true&"; // Unmodifiable
		public static readonly String APP_REQUEST_LANG = "en-AU";
		public static readonly String USER_DATA_FOLDER = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_FOLDER_NAME);
	}

	public MainWindow()
	{
		InitializeComponent();
		AttachControlEventHandlers();
		_webViewComponent = new WebViewComponent();

		DataContext = new MainViewModel(_webViewComponent, CloseWindow);
	}
	[STAThread]
	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
	}
	private async void startBrowser(){
		var options = new CoreWebView2EnvironmentOptions
		{
			AllowSingleSignOnUsingOSPrimaryAccount = true,
			Language = $"{Globals.APP_REQUEST_LANG}"
		};


		if (!Directory.Exists(Globals.USER_DATA_FOLDER))
		{
			Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
		}
		var environment = await CoreWebView2Environment.CreateAsync(null, Globals.USER_DATA_FOLDER, options).ConfigureAwait(true);


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


		if (Environment.GetCommandLineArgs().Length > 1)
		{
			var outString = RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
			//Add code to check for gpu pram
			if (outString.StartsWith("gpu"))
			{
				WebView.Source = new System.Uri($"edge://gpu", System.UriKind.Absolute);
				return;
			}
			else
			{
				WebView.Source = new System.Uri($"{Globals.BASE_URL}{outString}", System.UriKind.Absolute);
				return;
			}
		}
		else
		{
			WebView.Source = new System.Uri($"{Globals.BASE_URL}", System.UriKind.Absolute);
		}
	}

	protected override async void OnContentRendered(EventArgs e)
	{

		base.OnContentRendered(e);
		startBrowser();
	}

	private void CloseWindow()
	{
		//WebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Network.clearBrowserCache", "{}");
		WebView.CoreWebView2.Profile.ClearBrowsingDataAsync();
		var milliseconds = 100;
		Thread.Sleep(milliseconds);
		Application.Current.Shutdown();
	}

	private void OnCloseButtonClick(object sender, RoutedEventArgs e) => CloseWindow();


	public void OnChanged(Object sender, FileSystemEventArgs e)
	{
		Dispatcher.Invoke(() =>
		{
			this.Activate();
			var milliseconds = 100;
			Thread.Sleep(milliseconds);
			if (e.ChangeType != WatcherChangeTypes.Changed)
			{
				return;
			}

			string filePath = @MainWindow.Globals.USER_DATA_FOLDER + @"\temp.txt";
			using (StreamReader inputFile = new(filePath))
			{
				if (MainWindow.RemoveSpecialChars(inputFile.ReadToEnd()).StartsWith("gpu"))
				{
					WebView.Source = new Uri($"edge://gpu", UriKind.Absolute);
				}
				else
				{
					WebView.Source = new Uri($"{Globals.BASE_URL}" + RemoveSpecialChars(inputFile.ReadToEnd()));
				}
			}
		});
	}
	public void AttachControlEventHandlers()
	{
		FileSystemWatcher fileSystemWatcher = new($"{Globals.USER_DATA_FOLDER}");
		var watcher = fileSystemWatcher;
		watcher.NotifyFilter = NotifyFilters.LastWrite;
		watcher.Changed += OnChanged;
		watcher.Filter = "temp.txt";
		watcher.IncludeSubdirectories = false;
		watcher.EnableRaisingEvents = true;
	}

	private void Exit_App(object sender, System.ComponentModel.CancelEventArgs e)
	{
		CloseWindow();
	}

	public static string RemoveSpecialChars(string str)
	{
		// Create  a string array and add the special characters you want to remove
		string[] chars = new string[] {"~", "`", "!", "@", "#", "$", "%", "^", "*", "(", ")", "_", "+", "}", "{", "]", "[", "|", "\"", ":", "'", ":", "?", ">", "<", "/", ".", ",", "\\" };

		//Iterate the number of times based on the String array length.
		for (int i = 0; i < chars.Length; i++)
		{
			if (str.Contains(chars[i]))
			{
				str = str.Replace(chars[i], "");
			}
		}
		str = str.Replace($"{Globals.URI_SCHEMA}", "");
		return str;
	}
}