using System;
using Eto.Forms;

namespace JabbR.Desktop.Actions
{
    public class Quit : Command
    {
        public const string ActionID = "Quit";
        
        public Quit()
        {
            this.ID = ActionID;
            this.ToolBarText = "Quit";
            this.MenuText = "&Quit";
            this.Shortcut = Application.Instance.CommonModifier | Keys.Q;
        }

        public override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);
            Application.Instance.Quit();
        }
    }
}

