using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;
using Eto;
using SignalR.Client.Transports;

namespace JabbR.Eto.Sections
{
	public class LoginSection : Panel, IXmlReadable
	{
		public const string DefaultServer = "http://jabbr.net";
		//public const string DefaultServer = "http://jabbr-staging.apphb.com";
		
		TextBox userName;
		PasswordBox password;
		ProgressBar progress;
		Button loginButton;
		Container loginPanel;
		Container progressPanel;
		TextBox serverText;
		
		public event EventHandler<EventArgs> LoggedIn;

		protected virtual void OnLoggedIn (EventArgs e)
		{
			if (LoggedIn != null)
				LoggedIn (this, e);
		}
		
		public event EventHandler<EventArgs> Initialized;

		protected virtual void OnInitialized (EventArgs e)
		{
			if (Initialized != null)
				Initialized (this, e);
		}
		
		
		public ConnectionInfo Info { get; set; }
		
		public LoginSection ()
		{
			var layout = new DynamicLayout (this);
			
			layout.Add (null);
			
			layout.AddRow (null, LoginPanel (), null);
			layout.AddRow (null, ProgressPanel (), null);
			
			layout.Add (null);
			Update (true);
		}
		
		public void UpdateDisplay ()
		{
			if (string.IsNullOrEmpty(userName.Text))
				userName.Focus ();
			else
				password.Focus ();
		}
		
		Control LoginPanel()
		{
			var layout = new DynamicLayout(loginPanel = new GroupBox{ Text = "JabbR Login"});

			layout.BeginVertical ();
			layout.AddRow (new Label{ Text = "Server" }, ServerEntry ());
			layout.AddRow (new Label{ Text = "Login" }, LoginEntry ());
			layout.AddRow (new Label{ Text = "Password" }, PasswordEntry ());
			
			layout.EndBeginVertical (Padding.Empty);
			layout.AddRow (null, LoginButton ());
			layout.EndVertical ();
			return loginPanel;
		}
		
		Control ProgressPanel ()
		{
			var layout = new DynamicLayout(progressPanel = new Panel());
			layout.Add (new Label { Text = "Authenticating...", Font = new Font(FontFamily.Sans, 18, FontStyle.Bold) });
			layout.Add (progress = new ProgressBar { Indeterminate = true });
			return layout.Container;
		}

		Control ServerEntry ()
		{
			serverText = new TextBox {
				Size = new Size(200, -1),
				Text = DefaultServer
			};
			serverText.KeyDown += HandleKeyDown;
			return serverText;
		}

		
		Control LoginEntry ()
		{
			userName = new TextBox {
				Size = new Size(200, -1)
			};
			userName.KeyDown += HandleKeyDown;
			return userName;
		}

		void HandleKeyDown (object sender, KeyPressEventArgs e)
		{
			if (e.KeyData == Key.Enter) {
				HandleLogin ();
				e.Handled = true;
			}
		}

		Control PasswordEntry ()
		{
			password = new PasswordBox {
				Size = new Size(200, -1)
			};
			password.KeyDown += HandleKeyDown;
			return password;
		}
		
		Control LoginButton ()
		{
			loginButton = new Button{
				Text = "Login"
			};
			loginButton.Click += (sender, e) => {
				HandleLogin();
			};
			return loginButton;
		}
		
		void Update(bool enable)
		{
			progressPanel.Visible = !enable;
			loginPanel.Visible = enable;
		}

		void HandleLogin ()
		{
			if (!loginPanel.Visible) return;
			Update (false);
			
			this.Info = new ConnectionInfo();
			var client = Info.Client = new JabbRClient (serverText.Text, new LongPollingTransport());

			OnInitialized (EventArgs.Empty);
			
			client.Connect (userName.Text, password.Text).ContinueWith (task => {
				if (task.Exception != null) {
					Application.Instance.Invoke (delegate {
						Update (true);
						var ex = task.Exception; //.GetBaseException();
						MessageBox.Show (this, string.Format ("Unable to log in. Reason: {0}", ex.Message));
					});
					return;
				}
				var logOnInfo = Info.LogOnInfo = task.Result;
				
				Console.WriteLine ("Logged on successfully. You are currently in the following rooms:");
				foreach (var room in logOnInfo.Rooms) {
					Console.WriteLine (room.Name);
				}

				client.GetUserInfo ().ContinueWith (task2 => {
					if (task2.Exception != null) {
						Console.WriteLine ("Failed to login {0}", task2.Exception);
						Application.Instance.Invoke (delegate {
							Update (true);
						});
					}
					Info.CurrentUser = task2.Result;
					
					//Console.WriteLine ("User id is {0}. Don't share this!", LogOnInfo.UserId);
					Application.Instance.Invoke (delegate {
						OnLoggedIn (EventArgs.Empty);
						Update (true);
					});
				});
			}
			);
		}

		#region IXmlReadable implementation
		public void ReadXml (System.Xml.XmlElement element)
		{
			serverText.Text = element.GetAttribute ("server") ?? DefaultServer;
			userName.Text = element.GetAttribute ("userName");
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.SetAttribute ("server", serverText.Text);
			element.SetAttribute ("userName", userName.Text);
		}
		#endregion
	}
}

