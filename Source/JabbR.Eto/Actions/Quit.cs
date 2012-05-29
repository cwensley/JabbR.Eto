using System;
using Eto.Forms;


namespace JabbR.Eto.Actions
{
	public class Quit : ButtonAction
	{
		public const string ActionID = "Quit";
		
		public Quit ()
		{
			this.ID = ActionID;
			this.ToolBarText = "Quit";
			this.MenuText = "&Quit";
			this.Accelerator = Application.Instance.CommonModifier | Key.Q;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			var form = Application.Instance.MainForm as MainForm;
			if (form != null)
				form.Disconnect();
			Application.Instance.Quit ();
		}
	}
}

