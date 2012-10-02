using Eto.Drawing;
using Eto.Forms;
using Eto.Platform.CustomControls;
using Eto.Platform.Wpf.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using csw = CefSharp.Wpf;

namespace JabbR.Eto.Wpf.Controls
{
	public class CefSharpWebViewHandler : WpfFrameworkElement<csw.WebView, WebView>, IWebView, CefSharp.IRequestHandler
	{
		HttpServer server;
		
		public CefSharpWebViewHandler ()
		{
			Control = new csw.WebView
			{
				RequestHandler = this
			};
		}

		public override void AttachEvent (string handler)
		{
			switch (handler)
			{
			case WebView.DocumentLoadedEvent:
				Control.PropertyChanged += (sender, e) => {
					if (e.PropertyName == "IsLoading")
					{
						if (!Control.IsLoading)
							Control.Dispatcher.BeginInvoke (new Action (() => {
								Widget.OnDocumentLoaded (new WebViewLoadedEventArgs (this.Url));
							}));
					}
				};
				break;
			case WebView.DocumentLoadingEvent:
				break;
			case WebView.DocumentTitleChangedEvent:
				Control.PropertyChanged += (sender, e) => {
					if (e.PropertyName == "Title")
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
			get { return new Uri(Control.Address); }
			set { Control.Address = value.AbsoluteUri; }
		}

		public void LoadHtml (string html, Uri baseUri)
		{
			if (baseUri != null)
			{
				if (server == null)
					server = new HttpServer ();
				server.SetHtml (html, baseUri != null ? baseUri.LocalPath : null);
				this.Url = server.Url;
			}
			else
				Control.LoadHtml (html);
		}

		public void GoBack ()
		{
			Control.Back ();
		}

		public bool CanGoBack
		{
			get { return Control.CanGoBack; }
		}

		public void GoForward ()
		{
			Control.Forward ();
		}

		public bool CanGoForward
		{
			get { return Control.CanGoForward; }
		}

		public void Stop ()
		{
			Control.Stop ();
		}

		public void Reload ()
		{
			Control.Reload ();
		}

		public string DocumentTitle
		{
			get { return Control.Title; }
		}

		public string ExecuteScript (string script)
		{
			return Convert.ToString (Control.EvaluateScript (script, TimeSpan.FromSeconds(10)));
		}

		public void ShowPrintDialog ()
		{
			Control.Print ();
		}

		bool CefSharp.IRequestHandler.OnBeforeBrowse (CefSharp.IWebBrowser browser, CefSharp.IRequest request, CefSharp.NavigationType naigationvType, bool isRedirect)
		{
			
			var uri = new Uri (request.Url);
			if (uri.Scheme != "chrome-devtools")
			{
				var args = new WebViewLoadingEventArgs (uri);
				Widget.OnDocumentLoading (args);
				return args.Cancel;
			}
			return false;
		}

		bool CefSharp.IRequestHandler.OnBeforeResourceLoad (CefSharp.IWebBrowser browser, CefSharp.IRequestResponse requestResponse)
		{
			return false;
		}

		void CefSharp.IRequestHandler.OnResourceResponse (CefSharp.IWebBrowser browser, string url, int status, string statusText, string mimeType, System.Net.WebHeaderCollection headers)
		{
		}

		public override Color BackgroundColor
		{
			get { return Colors.Transparent; }
			set { }
		}

	}
}
