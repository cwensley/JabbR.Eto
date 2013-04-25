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
using ac = Awesomium.Core;
using awc = Awesomium.Windows.Controls;
using swm = System.Windows.Media;


namespace JabbR.Eto.Wpf.Controls
{
	public class AwesomiumSharpWebViewHandler : WpfFrameworkElement<awc.WebControl, WebView>, IWebView
	{
		HttpServer server;

		public AwesomiumSharpWebViewHandler ()
		{
			Control = new awc.WebControl ();
			Control.NavigationInfo = ac.NavigationInfo.Verbose;
		}

		public override void AttachEvent (string handler)
		{
			switch (handler)
			{
			case WebView.DocumentLoadedEvent:
				Control.LoadingFrameComplete += (sender, e) => {
					Control.Dispatcher.BeginInvoke (new Action (() => {
						Widget.OnDocumentLoaded (new WebViewLoadedEventArgs (this.Url));
					}));
				};
				break;
			case WebView.DocumentLoadingEvent:
				Control.BeginNavigation += (sender, e) => {
					// this doesn't actually work, but is needed for our use!
				//Control.LoadingFrame += (sender, e) => {
					var args = new WebViewLoadingEventArgs (e.Url, true);
					Widget.OnDocumentLoading (args);
					if (args.Cancel)
						Control.Stop (); // does not work!!
				};
				break;
			case WebView.OpenNewWindowEvent:
				Control.ShowCreatedWebView += (sender, e) => {
					var args = new WebViewNewWindowEventArgs (e.TargetURL, null);
					Widget.OnOpenNewWindow (args);
					e.Cancel = args.Cancel;
				};
				break;
			case WebView.DocumentTitleChangedEvent:
				Control.TitleChanged += (sender, e) => {
					Control.Dispatcher.BeginInvoke (new Action (() => {
						Widget.OnDocumentTitleChanged (new WebViewTitleEventArgs (e.Title));
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
			get { return Control.Source; }
			set { Control.Source = value; }
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
				Control.LoadHTML(html);
		}

		public void GoBack ()
		{
			Control.GoBack ();
		}

		public bool CanGoBack
		{
			get { return Control.CanGoBack (); }
		}

		public void GoForward ()
		{
			Control.GoForward ();
		}

		public bool CanGoForward
		{
			get { return Control.CanGoForward (); }
		}

		public void Stop ()
		{
			Control.Stop ();
		}

		public void Reload ()
		{
			Control.Reload (true);
		}

		public string DocumentTitle
		{
			get { return Control.Title; }
		}

		public string ExecuteScript (string script)
		{
			/**/
			Control.ExecuteJavascript(script);
			return null;
			/**
			return Convert.ToString (Control.EvaluateScript (script));
			/**/
		}

		public void ShowPrintDialog ()
		{
			//Control.Print ();
		}


		public override Color BackgroundColor
		{
			get { return Colors.Transparent; }
			set { }
		}

	}
}
