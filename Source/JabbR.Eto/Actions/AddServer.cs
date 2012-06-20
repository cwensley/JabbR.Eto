using System;
using Eto.Forms;
using JabbR.Eto.Interface.Dialogs;
using JabbR.Eto.Model.JabbR;

namespace JabbR.Eto.Actions
{
	public class AddServer : ButtonAction
	{
		public const string ActionID = "AddServer";
		
		public AddServer ()
		{
			this.ID = ActionID;
			this.MenuText = "Add Server...";
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			var server = new JabbRServer { Name = "JabbR.net", Address = "http://jabbr.net" };
			using (var dialog = new ServerDialog(server)) {
				var ret = dialog.ShowDialog (Application.Instance.MainForm);
				if (ret == DialogResult.Ok) {
					Console.WriteLine ("Added Server, Name: {0}", server.Name);
					var config = JabbRApplication.Instance.Configuration;
					config.AddServer (server);
				}
			}
			
		}
	}
}

