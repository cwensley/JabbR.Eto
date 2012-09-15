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

namespace JabbR.Eto.Interface.JabbR
{
	public class JabbRAuthDialog : Dialog
	{
		WebView web;
		JabbRServer server;
		HttpServer webserver;
		const string AppName = "jabbr";

		public Uri LocalhostTokenUrl { get; private set; }
		
		public string UserID { get; set; }
		
		public JabbRAuthDialog (JabbRServer server)
		{
			this.server = server;
			this.ClientSize = new Size (408, 174);
			this.Resizable = true;
			this.Title = "JabbR Login";
			
			webserver = new HttpServer ();
			LocalhostTokenUrl = new Uri (webserver.Url, "Authorize");
			webserver.ReceivedRequest += (sender, e) => {
				Console.WriteLine ("Req: {0}", e.Request.RawUrl);
				if (e.Request.Url == LocalhostTokenUrl) {
					Console.WriteLine ("Woooo!!!");
					e.Cancel = true;
					var reader = new StreamReader (e.Request.InputStream);
					var tokenString = reader.ReadToEnd ();
					var tokens = tokenString.Split ('=');
					if (tokens.Length == 2 && tokens [0] == "token") {
						Application.Instance.AsyncInvoke (delegate {
							var getUserTask = this.GetUserId (tokens [1]);
							getUserTask.ContinueWith (task => {
								Application.Instance.AsyncInvoke (delegate {
									this.UserID = task.Result;
									this.DialogResult = DialogResult.Ok;
									this.Close ();
								});
							}, TaskContinuationOptions.OnlyOnRanToCompletion);
							
							getUserTask.ContinueWith (task => {
								Console.WriteLine ("Error: {0}", task.Exception);
								MessageBox.Show (this, "Cannot get User ID from token", MessageBoxButtons.OK, MessageBoxType.Error);
							}, TaskContinuationOptions.OnlyOnFaulted);
							
							web.LoadHtml (Assembly.GetExecutingAssembly ().GetManifestResourceStream ("JabbR.Eto.Interface.JabbR.GetUserID.html"), null);
						});
					}
				}
			};
			
			web = new WebView ();
			this.AddDockedControl (web);
			webserver.SetHtml (AuthHtml (true), null);
			web.Url = webserver.Url;
			
			HandleEvent (ClosedEvent);
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
			var htmlStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("JabbR.Eto.Interface.JabbR.AuthHtml.html");
			var html = new StreamReader (htmlStream).ReadToEnd ();
			string forceReauthString = forceReauth.ToString (CultureInfo.InvariantCulture).ToLower ();
			return string.Format (html, AppName, LocalhostTokenUrl, forceReauthString);
		}
		
		Task<string> GetUserId (string token)
		{
			var completion = new TaskCompletionSource<string> ();
			
			var tokenAuthentication = Task.Factory.StartNew (() => AuthenticateToken (token));
			tokenAuthentication.ContinueWith (failedTokenTask => {
				completion.TrySetException (failedTokenTask.Exception);
			}, TaskContinuationOptions.OnlyOnFaulted);
			
			tokenAuthentication.ContinueWith (tokenTask => {
				Console.WriteLine ("User ID: {0}", tokenTask.Result);
				completion.TrySetResult (tokenTask.Result);
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
			return completion.Task;
		}
		
		string AuthenticateToken (string token)
		{
			var cookieContainer = new CookieContainer ();
			var address = server.Address.TrimEnd ('/');
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
			Debug.WriteLine ("Got User: {0}", jsonObject ["userId"]);
			
			return (string)jsonObject ["userId"];
		}
	}
}

