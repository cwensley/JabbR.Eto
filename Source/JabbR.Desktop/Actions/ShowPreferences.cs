using Eto;
using Eto.Forms;
using JabbR.Desktop.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JabbR.Desktop.Interface.Dialogs;

namespace JabbR.Desktop.Actions
{
    public class ShowPreferences : ButtonAction
    {
        Configuration config;
        public const string ActionID = "ShowPreferences";

        public ShowPreferences(Configuration config)
        {
            this.ID = ActionID;
            this.config = config;

            if (Generator.Current.IsMac)
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
                JabbRApplication.Instance.SaveConfiguration ();
            }
        }
    }
}
