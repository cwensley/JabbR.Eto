using System;
using Eto.Forms;
using JabbR.Eto.Model;
using JabbR.Eto.Model.JabbR;
using Eto;

namespace JabbR.Eto.Interface.JabbR
{
	public class JabbRServerEdit
	{
		JabbRServer server;
		
		public JabbRServerEdit (JabbRServer server, DynamicLayout layout)
		{
			this.server = server;
			layout.AddRow (new Label { Text = "Address" }, EditAddress ());
			layout.EndBeginVertical ();
			layout.Add (LoginInformation ());
			layout.EndBeginVertical ();
		}

		Control LoginInformation ()
		{
			var layout = new DynamicLayout (new GroupBox{ Text = "Login"});
			
			layout.AddRow (new Label { Text = "UserName" }, EditUserName ());
			layout.AddRow (new Label { Text = "Password" }, EditPassword ());

			return layout.Container;
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
			var bind = control.Bind ("Text", server, "Password", DualBindingMode.OneWay);
			return control;
		}
	}
}

