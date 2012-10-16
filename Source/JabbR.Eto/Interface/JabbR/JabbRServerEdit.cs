using System;
using Eto.Forms;
using JabbR.Eto.Model;
using JabbR.Eto.Model.JabbR;
using Eto;
using Eto.Drawing;
using SignalR.Client;

namespace JabbR.Eto.Interface.JabbR
{
	public class JabbRServerEdit
	{
		JabbRServer server;
		Container authSection;
		Container loginSection;
		Container socialSection;
		CheckBox useSocialLogin;
		Label statusLabel;
		TextBox janrainAppName;
		TextBox serverAddress;
		Button authButton;
		bool isNew;
		
		public JabbRServerEdit (JabbRServer server, DynamicLayout layout, bool isNew)
		{
			this.isNew = isNew;
			this.server = server;
			layout.AddRow (new Label { Text = "Address" }, EditAddress ());
			layout.EndBeginVertical ();
			layout.AddRow (UseSocialLogin ());
			layout.Add (authSection = new Panel { MinimumSize = new Size(0, 100) });
			layout.EndBeginVertical ();
			LoginSection();
			SocialSection();
			
			SetVisibility ();
		}
		
		void SetVisibility ()
		{
			var useSocial = useSocialLogin.Checked ?? false;
			if (useSocial) {
				authSection.AddDockedControl (socialSection);
				var authenticated = !string.IsNullOrEmpty (server.UserId);
				if (authenticated) {
					authButton.Text = "Re-authenticate";
					statusLabel.Text = "Authenticated";
					statusLabel.TextColor = Colors.Green;
				}
				else {
					authButton.Text = "Authenticate";
					statusLabel.Text = "Not Authenticated";
					statusLabel.TextColor = Colors.Red;
				}
					
			}
			else
				authSection.AddDockedControl (loginSection);
		}
		
		Control LoginSection ()
		{
			var layout = new DynamicLayout (loginSection = new GroupBox{ Text = "Login"});
			
			layout.Add (null);
			layout.AddRow (new Label { Text = "UserName" }, EditUserName ());
			layout.AddRow (new Label { Text = "Password" }, EditPassword ());
			layout.Add (null);

			return layout.Container;
		}
		Control SocialSection ()
		{
			var layout = new DynamicLayout (socialSection = new GroupBox{ Text = "Janrain"});

			layout.Add (null);
			layout.AddSeparateRow (new Label { Text = "App Name" }, JanrainAppName ());
			layout.Add (null);
			layout.BeginVertical ();
			layout.AddRow (null, StatusLabel (), null);
			layout.AddRow (null, AuthButton(), null);
			layout.EndVertical ();
			layout.Add (null);

			return layout.Container;
		}
		
		Control JanrainAppName ()
		{
			var control = janrainAppName = new TextBox ();
			control.Bind ("Text", "JanrainAppName", DualBindingMode.OneWay);
			return control;
		}
		
		Control StatusLabel ()
		{
			var control = statusLabel = new Label {
				HorizontalAlign = HorizontalAlign.Center
			};
			return control;
		}
			
		Control AuthButton ()
		{
			var control = authButton = new Button { Text = "Authenticate" };
			control.Click += delegate {
				var dlg = new JabbRAuthDialog(serverAddress.Text, janrainAppName.Text);
				dlg.DisplayMode = DialogDisplayMode.Attached;
				var result = dlg.ShowDialog (control);
				if (result == DialogResult.Ok) {
					server.UserId = dlg.UserID;
					SetVisibility ();
				}
			};
			return control;
		}
		
		Control UseSocialLogin ()
		{
			var control = useSocialLogin = new CheckBox { Text = "Use Social Login" };
			control.Bind ("Checked", "UseSocialLogin", DualBindingMode.OneWay);
			control.CheckedChanged += delegate {
				SetVisibility ();
			};
			return control;
			
		}
		
		Control EditAddress ()
		{
			var control = serverAddress = new TextBox ();
			control.Bind ("Text", "Address", DualBindingMode.OneWay);
			return control;
		}
		
		Control EditUserName ()
		{
			var control = new TextBox ();
			control.Bind ("Text", "UserName", DualBindingMode.OneWay);
			return control;
		}
		
		Control EditPassword ()
		{
			var control = new PasswordBox ();
			control.Bind ("Text", "Password", DualBindingMode.OneWay);
			return control;
		}
	}
}

