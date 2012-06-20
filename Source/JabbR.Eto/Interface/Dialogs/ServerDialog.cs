using System;
using Eto.Forms;
using JabbR.Eto.Model;
using Eto.Drawing;
using Eto;

namespace JabbR.Eto.Interface.Dialogs
{
	public class ServerDialog : Dialog
	{
		public Server Server { get; private set; }
		
		Button cancelButton;
		
		public bool AllowCancel
		{
			get { return cancelButton.Enabled; }
			set { cancelButton.Enabled = cancelButton.Visible = value; }
		}
		
		public ServerDialog (Server server)
		{
			this.Server = server;
			this.Title = "Add Server";
			this.MinimumSize = new Size (300, 0);
			
			var layout = new DynamicLayout (this);
			
			layout.BeginVertical ();
			
			layout.AddRow (new Label { Text = "Server Name"}, ServerName ());
			
			// generate server-specific edit controls
			server.GenerateEditControls (layout);
			
			layout.AddRow (null, AutoConnectButton());
			
			layout.EndBeginVertical ();
			
			layout.AddRow (null, CancelButton(), SaveButton ());
			
			layout.EndVertical ();
		}
		
		Control AutoConnectButton ()
		{
			var control = new CheckBox { Text = "Connect on Startup" };
			control.Bind ("Checked", Server, "ConnectOnStartup", DualBindingMode.OneWay);
			return control;
		}
		
		Control ServerName ()
		{
			var control = new TextBox ();
			control.Bind ("Text", Server, "Name", DualBindingMode.OneWay);
			return control;
		}

		Control CancelButton ()
		{
			var control = cancelButton = new Button { Text = "Cancel" };
			this.AbortButton = control;
			control.Click += (sender, e) => {
				DialogResult = DialogResult.Cancel;
				Close ();
			};
			return control;
		}
		
		Control SaveButton ()
		{
			var control = new Button { Text = "Save" };
			this.DefaultButton = control;
			control.Click += (sender, e) => {
				UpdateBindings ();
				DialogResult = DialogResult.Ok;
				Close ();
			};
			return control;
		}
	}
}

