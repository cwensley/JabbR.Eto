using System;
using Eto.Forms;
using JabbR.Eto.Interface.Dialogs;
using JabbR.Eto.Interface;
using JabbR.Eto.Model;
using System.Diagnostics;

namespace JabbR.Eto.Actions
{
	public class EditServer : ButtonAction
	{
		public const string ActionID = "EditServer";
		Channels channels;
		Configuration config;
		
		public EditServer (Channels channels, Configuration config)
		{
			this.channels = channels;
			this.config = config;
			this.ID = ActionID;
			this.MenuText = "Edit Server...";
		}
		
		public override bool Enabled {
			get {
				return base.Enabled && channels.SelectedServer != null;
			}
			set {
				base.Enabled = value;
			}
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			var server = channels.SelectedServer;
			if (server != null) {
				using (var dialog = new ServerDialog(server, false)) {
					dialog.DisplayMode = DialogDisplayMode.Attached;
					var ret = dialog.ShowDialog (Application.Instance.MainForm);
					if (ret == DialogResult.Ok) {
						Debug.WriteLine (string.Format ("Edited Server, Name: {0}", server.Name));
					}
				}
			}
		}
	}
}

