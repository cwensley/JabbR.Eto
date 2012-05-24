using System;
using Eto.Forms;


namespace JabbR.Eto.Actions
{
	public class About : ButtonAction
	{
		public const string ActionID = "About";
		
		public About ()
		{
			this.ID = ActionID;
			this.MenuText = "About JabbR.Eto";
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
		}
	}
}

