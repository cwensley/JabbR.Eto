using Eto;
using Eto.Forms;
using JabbR.Eto.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JabbR.Eto.Actions
{
	public class ShowPreferences : ButtonAction
	{
		Configuration config;

		public const string ActionID = "ShowPreferences";

		public ShowPreferences (Configuration config)
		{
			this.ID = ActionID;
			this.config = config;

			if (Generator.Current.ID == Generators.Mac)
			{
				this.MenuText = "Preferences...";
				this.Accelerator = Application.Instance.CommonModifier | Key.Comma;
			}
			else
			{
				this.MenuText = "Options...";
			}
		}

		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);

			var dialog = new Interface.Dialogs.PreferencesDialog (config);
			if (dialog.ShowDialog (Application.Instance.MainForm) == DialogResult.Ok)
			{
				// update things!
			}
		}
	}
}
