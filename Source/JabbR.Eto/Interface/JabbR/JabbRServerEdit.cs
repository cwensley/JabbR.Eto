using System;
using Eto.Forms;
using JabbR.Eto.Model;
using JabbR.Eto.Model.JabbR;
using Eto;
using Eto.Drawing;

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
		Button authButton;
		
		public JabbRServerEdit (JabbRServer server, DynamicLayout layout)
		{
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
				statusLabel.Visible = authenticated;
				authButton.Text = authenticated ? "Re-authenticate" : "Authenticate";
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
			layout.AddRow (null, StatusLabel (), null);
			layout.AddRow (null, AuthButton(), null);
			layout.Add (null);

			return layout.Container;
		}
		
		Control StatusLabel ()
		{
			var control = statusLabel = new Label {
				Text = "Authenticated",
				TextColor = Colors.Green,
				HorizontalAlign = HorizontalAlign.Center
			};
			return control;
		}
			
		Control AuthButton ()
		{
			var control = authButton = new Button { Text = "Authenticate" };
			control.Click += delegate {
				var dlg = new JabbRAuthDialog(server);
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
			control.Bind ("Checked", server, "UseSocialLogin", DualBindingMode.OneWay);
			control.CheckedChanged += delegate {
				SetVisibility ();
			};
			return control;
			
		}
		
		Control EditAddress ()
		{
			var control = new TextBox ();
			control.Bind ("Text", server, "Address", DualBindingMode.OneWay);
			return control;
		}
		
		Control EditUserName ()
		{
			var control = new TextBox ();
			control.Bind ("Text", server, "UserName", DualBindingMode.OneWay);
			return control;
		}
		
		Control EditPassword ()
		{
			var control = new PasswordBox ();
			control.Bind ("Text", server, "Password", DualBindingMode.OneWay);
			return control;
		}
	}
}

