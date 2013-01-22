using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Eto.Model.JabbR;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using JabbR.Client.Models;
using System.Diagnostics;
using Eto;
using JabbR.Eto.Interface.Dialogs;

namespace JabbR.Eto.Interface.JabbR
{
	public class JabbRAuthDialog : Dialog
	{
		WebView web;
		HttpServer webserver;
		Size defaultSize = new Size (408, 174 + 40 + 20);
		Size expandedSize = new Size (800, 500);
		bool isLocal = true;

		Uri LocalhostTokenUrl { get; set; }
		
		public string UserID { get; set; }
		
		public string ServerAddress { get; set; }
		public string AppName { get; set; }
		
		public JabbRAuthDialog (string serverAddress, string appName)
		{
			this.ServerAddress = serverAddress;
			this.AppName = appName;
			this.DisplayMode = DialogDisplayMode.Attached;
			
			this.ClientSize = defaultSize;
			this.Resizable = true;
			this.Title = "JabbR Login";
			
			var baseDir = Path.Combine (EtoEnvironment.GetFolderPath(EtoSpecialFolder.ApplicationResources), "Styles", "default");
			webserver = new HttpServer (baseDir);
			LocalhostTokenUrl = new Uri (webserver.Url, "Authorize");
			webserver.StaticContent.Add ("/", AuthHtml(true));
			webserver.StaticContent.Add ("/Authorize", GetUserIDHtml());
			webserver.ReceivedRequest += HandleReceivedRequest;
			
			
			web = new WebView ();
			web.DocumentLoaded += HandleDocumentLoaded;
			web.Url = webserver.Url;
			
			var layout = new DynamicLayout(this);
			layout.Add (web, yscale: true);
			layout.AddSeparateRow (Padding.Empty).Add (null, this.CancelButton ());
			
			HandleEvent (ClosedEvent);
		}
		
		void HandleReceivedRequest (object sender, HttpServerRequestEventArgs e)
		{
			if (e.Request.Url == LocalhostTokenUrl) {
				var reader = new StreamReader (e.Request.InputStream);
				var tokenString = reader.ReadToEnd ();
				var tokens = tokenString.Split ('=');
				if (tokens.Length == 2 && tokens [0] == "token") {
					Application.Instance.AsyncInvoke (delegate {
						var getUserTask = this.GetUserId (tokens [1]);
						getUserTask.ContinueWith (task => {
							Application.Instance.AsyncInvoke (delegate {
								this.UserID = task.Result;
								this.Close (DialogResult.Ok);
							});
						}, TaskContinuationOptions.OnlyOnRanToCompletion);
						
						getUserTask.ContinueWith (task => {
							MessageBox.Show (this, "Cannot get User ID from token", MessageBoxButtons.OK, MessageBoxType.Error);
						}, TaskContinuationOptions.OnlyOnFaulted);
					});
				}
			}
		}

		void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			var newIsLocal = string.IsNullOrEmpty (e.Uri.AbsolutePath) || e.Uri.IsLoopback;
			if (isLocal != newIsLocal) {
				isLocal = newIsLocal;
				var newSize = isLocal ? defaultSize : expandedSize;
				var location = this.Location;
				var rect = new Rectangle(location, this.ClientSize);
				rect.Inflate ((newSize.Width - rect.Width) / 2, (newSize.Height - rect.Height) / 2);
				if (Generator.IsMac ())
					rect.Y = location.Y;
				this.Location = rect.Location;
				this.ClientSize = rect.Size;
			}
		}

		public override void OnClosed (EventArgs e)
		{
			base.OnClosed (e);
			if (webserver != null) {
				webserver.Dispose ();
				webserver = null;
			}
		}
		
		string AuthHtml (bool forceReauth)
		{
			var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("JabbR.Eto.Interface.JabbR.AuthHtml.html");
			var html = new StreamReader (stream).ReadToEnd ();
			html = html.Replace ("$TOKEN_URL$", LocalhostTokenUrl.ToString ());
			html = html.Replace ("$APP_NAME$", AppName);
			html = html.Replace ("$FORCE_REAUTH$", forceReauth.ToString (CultureInfo.InvariantCulture).ToLower ());
			return html;
		}
		
		string GetUserIDHtml ()
		{
			var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("JabbR.Eto.Interface.JabbR.GetUserID.html");
			return new StreamReader (stream).ReadToEnd ();
		}
		
		Task<string> GetUserId (string token)
		{
			var completion = new TaskCompletionSource<string> ();
			
			var tokenAuthentication = Task.Factory.StartNew (() => AuthenticateToken (token));
			tokenAuthentication.ContinueWith (failedTokenTask => {
				completion.TrySetException (failedTokenTask.Exception);
			}, TaskContinuationOptions.OnlyOnFaulted);
			
			tokenAuthentication.ContinueWith (tokenTask => {
				completion.TrySetResult (tokenTask.Result);
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
			return completion.Task;
		}
		
		string AuthenticateToken (string token)
		{
			var cookieContainer = new CookieContainer ();
			var address = this.ServerAddress.TrimEnd ('/');
			var request = (HttpWebRequest)System.Net.WebRequest.Create (address + "/Auth/Login.ashx");
			request.CookieContainer = cookieContainer;
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			
			string data = "token=" + token;
			byte[] postBytes = Encoding.UTF8.GetBytes (data);
			
			request.ContentLength = postBytes.Length;
			var dataStream = request.GetRequestStream ();
			dataStream.Write (postBytes, 0, postBytes.Length);
			dataStream.Close ();
			var response = request.GetResponse ();
			response.Close ();
			
			var cookies = cookieContainer.GetCookies (new Uri (address));
			string cookieValue = cookies [0].Value;
			
			var jsonObject = JObject.Parse (cookieValue);
			
			return (string)jsonObject ["userId"];
		}
	}
}

