using System;
using Eto.Forms;
using JabbR.Desktop.Interface.Dialogs;

namespace JabbR.Desktop.Actions
{
    public class About : ButtonAction
    {
        public const string ActionID = "About";
        
        public About()
        {
            this.ID = ActionID;
            this.MenuText = "About JabbR";
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            
            var dlg = new AboutDialog();
            dlg.ShowDialog(Application.Instance.MainForm);
        }
    }
}

