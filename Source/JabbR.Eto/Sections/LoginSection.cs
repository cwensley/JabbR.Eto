using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;

namespace JabbR.Eto.Sections
{
	public class LoginSection : Panel
	{
		TextBox userName;
		PasswordBox password;
		
		public event EventHandler<EventArgs> LoggedIn;

		protected virtual void OnLoggedIn (EventArgs e)
		{
			if (LoggedIn != null)
				LoggedIn (this, e);
		}
		
		public JabbRClient Client { get; private set; }

		public LogOnInfo LogOnInfo { get; private set; }
		
		public LoginSection ()
		{
			var layout = new DynamicLayout (this);
			
			layout.Add (null);
			
			layout.AddRow (null, LoginControls (), null);
			
			layout.Add (null);
		}
		
		Control LoginControls ()
		{
			var layout = new DynamicLayout (new GroupBox{ Text = "JabbR Login"});
			layout.BeginVertical ();
			layout.AddRow (new Label{ Text = "Login" }, LoginEntry ());
			layout.AddRow (new Label{ Text = "Password" }, PasswordEntry ());
			
			layout.EndBeginVertical (Padding.Empty);
			layout.AddRow (null, LoginButton ());
			layout.EndVertical ();
			
			return layout.Container;
		}

		Control LoginEntry ()
		{
			var control = userName = new TextBox {
				Size = new Size(200, -1),
				Text = "testclient"
			};
			return control;
		}

		Control PasswordEntry ()
		{
			var control = password = new PasswordBox {
				Size = new Size(200, -1),
				Text = "password"
			};
			return control;
		}
		
		Control LoginButton ()
		{
			var control = new Button{
				Text = "Login"
			};
			control.Click += HandleLogin;
			return control;
		}

		void HandleLogin (object sender, EventArgs e)
		{
			var loginButton = sender as Button;
			loginButton.Enabled = userName.Enabled = password.Enabled = false;
			string server = "http://jabbr-staging.apphb.com/";
			
			this.Client = new JabbRClient (server);
			Console.WriteLine ("User: {0}, Password: {1}", userName.Text, password.Text);
			this.Client.Connect (userName.Text, password.Text).ContinueWith (task => {
				this.LogOnInfo = task.Result;
				
				Console.WriteLine ("Logged on successfully. You are currently in the following rooms:");
				foreach (var room in LogOnInfo.Rooms) {
					Console.WriteLine (room.Name);
					Console.WriteLine (room.Private);
				}

				//Console.WriteLine ("User id is {0}. Don't share this!", LogOnInfo.UserId);
				Application.Instance.Invoke (delegate {
					OnLoggedIn (e);
				});
			}
			);
		}
	}
}

