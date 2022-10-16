using System.IO;
using System.Windows;
using System.Threading;
using Microsoft.Web.WebView2.Core;
using static KioskBrowser.MainWindow;

namespace KioskBrowser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
	static Mutex mutex = new Mutex(true, @KioskBrowser.MainWindow.Globals.APP_ID);

	public App()
	{
		if (mutex.WaitOne(TimeSpan.Zero, true))
		{
			try
			{
				if (!Directory.Exists(Globals.USER_DATA_FOLDER))
				{
					Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
				}
				try
				{
					var version = CoreWebView2Environment.GetAvailableBrowserVersionString();
					// Do something with `version` if needed.
				}
				catch (WebView2RuntimeNotFoundException exception)
				{
					// Handle the runtime not being installed.
					// `exception.Message` is very nicely specific: It (currently at least) says "Couldn't find a compatible Webview2 Runtime installation to host WebViews."
				MessageBox.Show(exception.Message);
				Environment.Exit(0);
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}
		else
		{
			if (!Directory.Exists(Globals.USER_DATA_FOLDER))
			{
				Directory.CreateDirectory(Globals.USER_DATA_FOLDER);
			}
			string filePath = @KioskBrowser.MainWindow.Globals.USER_DATA_FOLDER + @"\temp.txt";
			var outString = KioskBrowser.MainWindow.RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
			using (StreamWriter outputFile = new StreamWriter(filePath))
			{
				outputFile.WriteLine(outString);
			}
			Environment.Exit(0);
		}
	}
	}