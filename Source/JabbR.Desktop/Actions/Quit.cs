using System;
using Eto.Forms;

namespace JabbR.Desktop.Actions
{
    public class Quit : ButtonAction
    {
        public const string ActionID = "Quit";
        
        public Quit()
        {
            this.ID = ActionID;
            this.ToolBarText = "Quit";
            this.MenuText = "&Quit";
            this.Accelerator = Application.Instance.CommonModifier | Key.Q;
        }
        
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Application.Instance.Quit();
        }
    }
}

