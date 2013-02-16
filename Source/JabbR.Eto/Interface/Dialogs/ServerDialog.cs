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
		bool allowConnect;
		
		public Server Server { get; private set; }
		
		public bool AllowCancel
		{
			get { return cancelButton.Enabled; }
			set { cancelButton.Enabled = cancelButton.Visible = value; }
		}
		
		public ServerDialog (Server server, bool isNew, bool allowConnect)
		{
			this.isNew = isNew;
			this.allowConnect = allowConnect && !isNew;
			this.Server = server;
			this.Title = "Add Server";
			this.MinimumSize = new Size (300, 0);
			this.DataContext = server;
			
			var layout = new DynamicLayout (this);
			
			layout.BeginVertical ();
			
			layout.AddRow (new Label { Text = "Server Name"}, ServerName ());
			
			// generate server-specific edit controls
			server.GenerateEditControls (layout, isNew);
			
			layout.AddRow (null, AutoConnectButton());
			
			layout.EndBeginVertical ();

			layout.AddRow (Connect (), Disconnect (), null, cancelButton = this.CancelButton (), this.OkButton ("Save", () => SaveData()));
			
			layout.EndVertical ();
			
			SetVisibility ();
		}
		

		bool SaveData (bool allowCancel = true)
		{
			UpdateBindings ();
			if (Server.CheckAuthentication(this, allowCancel, true) && Server.PreSaveSettings (this)) {
				return true;
			}
			else
				return false;
		}
		
		void SetVisibility()
		{
			connectButton.Visible = allowConnect && !Server.IsConnected;
			disconnectButton.Visible = allowConnect && Server.IsConnected;
		}
		
		Control AutoConnectButton ()
		{
			var control = new CheckBox { Text = "Connect on Startup" };
			control.Bind ("Checked", "ConnectOnStartup", DualBindingMode.OneWay);
			return control;
		}
		
		Control ServerName ()
		{
			var control = new TextBox ();
			control.Bind ("Text", "Name", DualBindingMode.OneWay);
			return control;
		}

		Control Connect ()
		{
			var control = connectButton = new Button {
				Text = "Connect"
			};
			control.Click += (sender, e) => {
				if (SaveData (false)) {
					try {
						Server.Connect ();
						this.Close (DialogResult.Ok);
					} catch (Exception ex) {
						var msg = string.Format("Error connecting to server {0}", ex.GetBaseException().Message);
						MessageBox.Show (this, msg, MessageBoxType.Error);
					}

				}
			};
			return control;
		}
		
		Control Disconnect ()
		{
			var control = disconnectButton = new Button {
				Text = "Disconnect"
			};
			control.Click += (sender, e) => {
				try {
					Server.Disconnect ();
					this.Close (DialogResult.Ok);
				} catch (Exception ex) {
					var msg = string.Format ("Error disconnecting from server {0}", ex.GetBaseException ().Message);
					MessageBox.Show (this, msg, MessageBoxType.Error);
				}
				this.Close (DialogResult.Ok);
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

