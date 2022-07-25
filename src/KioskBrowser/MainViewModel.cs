using KioskBrowser.WebView;

namespace KioskBrowser;

public class MainViewModel
{
	private readonly WebViewComponent _webViewComponent;
	private readonly Action _close;

	public MainViewModel(WebViewComponent webViewComponent, Action close)
	{
		_webViewComponent = webViewComponent;
		_close = close;
	}
	public DelegateCommand CloseWindowCommand => new(_ => { _close(); });

	public string Title => $"{MainWindow.Globals.APP_NAME}";
	public string app_icon => $"{MainWindow.Globals.APP_ICON}";

}