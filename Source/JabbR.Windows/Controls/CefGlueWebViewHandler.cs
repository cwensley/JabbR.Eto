
#if CEFGLUE

using Eto;
using Eto.Drawing;
using Eto.Forms;
using Eto.Platform.CustomControls;
using Eto.Platform.Wpf.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using xc = Xilium.CefGlue;
using swm = System.Windows.Media;
using xcw = Xilium.CefGlue.WPF;
using sw = System.Windows;


namespace JabbR.Windows.Controls
{
    public class CefGlueWebViewHandler : WpfFrameworkElement<xcw.WpfCefBrowser, WebView>, IWebView
	{
        double zoomLevel = 0.0;

        public override bool UseMousePreview { get { return true; } }

        class MyCefApp : xcw.WpfCefApp
        {
        }
		
        static CefGlueWebViewHandler ()
		{
            xc.CefRuntime.Load();

            var mainArgs = new xc.CefMainArgs(null);
            var cefApp = new MyCefApp();

            //var exitCode = xc.CefRuntime.ExecuteProcess(mainArgs, cefApp);
            //if (exitCode != -1) { return exitCode; }

            var cefSettings = new xc.CefSettings
            {
                SingleProcess = true,
                MultiThreadedMessageLoop = true,
                LogSeverity = xc.CefLogSeverity.Disable,
            };

            xc.CefRuntime.Initialize(mainArgs, cefSettings, cefApp);
		}

        public CefGlueWebViewHandler()
		{
			Control = new xcw.WpfCefBrowser ();
            Control.Loaded += (s, e) =>
            {
                // make the dpi match the screen
				var visual = sw.PresentationSource.FromVisual(Control);
				if (visual != null)
				{
					var m = visual.CompositionTarget.TransformToDevice;
					var dpiTransform = new swm.ScaleTransform(1 / m.M11, 1 / m.M22);
					if (dpiTransform.CanFreeze)
						dpiTransform.Freeze();
					Control.LayoutTransform = dpiTransform;
					zoomLevel = Math.Log(m.M11) / Math.Log(1.2);
				}
            };
            Control.LoadingStateChanged += (sender, e) =>
            {
                Control.Browser.GetHost().SetZoomLevel(zoomLevel);
            };
        }

		public override void AttachEvent (string handler)
		{
			switch (handler)
			{
			case WebView.DocumentLoadedEvent:
                    Control.LoadingStateChanged += (sender, e) =>
                    {
                        if (!Control.Browser.IsLoading)
                        {
                            Control.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                Widget.OnDocumentLoaded(new WebViewLoadedEventArgs(this.Url));
                            }));
                        }
                    };
				break;
			case WebView.DocumentLoadingEvent:
                Control.BeforeNavigation += (sender, e) =>
                {
                    var args = new WebViewLoadingEventArgs(new Uri(e.Request.Url), e.Frame.IsMain);
                    Widget.OnDocumentLoading(args);
                    e.Cancel = args.Cancel;
                };
                break;
            case WebView.OpenNewWindowEvent:
                Control.BeforePopup += (sender, e) =>
                {
                    var uri = new Uri(e.TargetUrl);
                    if (uri.Scheme != "chrome-devtools")
                    {
                        var args = new WebViewNewWindowEventArgs(uri, null);
                        Widget.OnOpenNewWindow(args);
                        e.Cancel = args.Cancel;
                    }
                };
				break;
			case WebView.DocumentTitleChangedEvent:
				Control.TitleChanged += (sender, e) => {
					Control.Dispatcher.BeginInvoke (new Action (() => {
						Widget.OnDocumentTitleChanged (new WebViewTitleEventArgs (Control.Title));
					}));
				};
				break;
			default:
				base.AttachEvent (handler);
				break;
			}
		}

		public Uri Url
		{
			get { return string.IsNullOrEmpty(Control.Address) ? null : new Uri(Control.Address); }
			set { Control.StartUrl = value.AbsoluteUri; }
		}

		public void LoadHtml (string html, Uri baseUri)
		{
            Control.LoadString(html, baseUri != null ? baseUri.LocalPath : null);
		}

		public void GoBack ()
		{
			Control.GoBack ();
		}

		public bool CanGoBack
		{
			get { return Control.CanGoBack(); }
		}

		public void GoForward ()
		{
			Control.GoForward ();
		}

		public bool CanGoForward
		{
			get { return Control.CanGoForward(); }
		}

		public void Stop ()
		{
            if (Control.Browser != null)
                Control.Browser.StopLoad();
		}

		public void Reload ()
		{
            if (Control.Browser != null)
                Control.Browser.Reload();
		}

		public string DocumentTitle
		{
			get { return Control.Title; }
		}

		public string ExecuteScript (string script)
		{
			/**/
            Control.ExecuteJavaScript(script);
			return null;
			/**
			return Convert.ToString (Control.EvaluateScript (script));
			/**/
		}

		public void ShowPrintDialog ()
		{
		}

		public override Color BackgroundColor
		{
			get { return Colors.Transparent; }
			set { }
		}


		public bool BrowserContextMenuEnabled
		{
			get;
			set;
		}
	}
}

#endif