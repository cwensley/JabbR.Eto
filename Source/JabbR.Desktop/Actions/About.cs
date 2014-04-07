using System;
using Eto.Forms;
using JabbR.Desktop.Interface.Dialogs;

namespace JabbR.Desktop.Actions
{
    public class About : Command
    {
        public const string ActionID = "About";
        
        public About()
        {
            this.ID = ActionID;
            this.MenuText = "About JabbR";
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            var dlg = new AboutDialog();
            dlg.ShowDialog(Application.Instance.MainForm);
        }
    }
}

