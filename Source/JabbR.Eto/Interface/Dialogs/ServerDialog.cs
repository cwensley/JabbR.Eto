using System;
using Eto.Forms;
using JabbR.Eto.Model;
using Eto.Drawing;
using Eto;
using System.Linq;

namespace JabbR.Eto.Interface.Dialogs
{
	public class ServerDialog : Dialog
	{
		Button cancelButton;
		Button connectButton;
		Button disconnectButton;
		bool isNew;
		
		public Server Server { get; private set; }
		
		public bool AllowCancel
		{
			get { return cancelButton.Enabled; }
			set { cancelButton.Enabled = cancelButton.Visible = value; }
		}
		
		public ServerDialog (Server server, bool isNew)
		{
			this.isNew = isNew;
			this.Server = server;
			this.Title = "Add Server";
			this.MinimumSize = new Size (300, 0);
			
			var layout = new DynamicLayout (this);
			
			layout.BeginVertical ();
			
			layout.AddRow (new Label { Text = "Server Name"}, ServerName ());
			
			// generate server-specific edit controls
			server.GenerateEditControls (layout, isNew);
			
			layout.AddRow (null, AutoConnectButton());
			
			layout.EndBeginVertical ();

			layout.AddRow (Connect (), Disconnect (), null, cancelButton = this.CancelButton (), this.OkButton ("Save", SaveData));
			
			layout.EndVertical ();
			
			SetVisibility ();
		}

		bool SaveData ()
		{
			UpdateBindings ();
			return true;
		}
		
		void SetVisibility()
		{
			connectButton.Visible = !isNew && !Server.IsConnected;
			disconnectButton.Visible = !isNew && Server.IsConnected;
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

		Control Connect ()
		{
			var control = connectButton = new Button {
				Text = "Connect"
			};
			control.Click += (sender, e) => {
				Server.Connect ();
				var parent = control.ParentWindow as Dialog;
				parent.Close (DialogResult.Ok);
			};
			return control;
		}
		
		Control Disconnect ()
		{
			var control = disconnectButton = new Button {
				Text = "Disconnect"
			};
			control.Click += (sender, e) => {
				Server.Disconnect ();
				var parent = control.ParentWindow as Dialog;
				parent.Close (DialogResult.Ok);
			};
			return control;
		}
		
		public override void OnClosed (EventArgs e)
		{
			base.OnClosed (e);
			if (DialogResult == DialogResult.Ok)
				UpdateBindings ();
		}
	}
}

