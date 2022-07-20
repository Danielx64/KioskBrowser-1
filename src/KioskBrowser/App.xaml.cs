using System.IO;
using System.Windows;
using System.Threading;

namespace KioskBrowser;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
	static Mutex mutex = new Mutex(true, $"temp");
	public static class Globals
	{
		public static readonly String APP_FOLDER_NAME = "your app folder name"; // Unmodifiable
		public static readonly String USER_DATA_FOLDER = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), APP_FOLDER_NAME);
	}
	public App()
	{
		if (mutex.WaitOne(TimeSpan.Zero, true))
		{
			try
			{

			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}
		else
		{
			string filePath = @Globals.USER_DATA_FOLDER + @"\temp.txt";
			var outString = KioskBrowser.MainWindow.RemoveSpecialChars(Environment.GetCommandLineArgs()[1]);
			using (StreamWriter outputFile = new StreamWriter(filePath))
			{
				outputFile.WriteLine(outString);
			}
			Environment.Exit(0);
		}
	}
	}