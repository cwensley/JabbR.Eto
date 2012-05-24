using System;
using Eto.Forms;

namespace JabbR.Eto
{
	public class JabbRApplication : Application
	{
		public JabbRApplication ()
		{
		}
		
		public override void OnInitialized (EventArgs e)
		{
			base.OnInitialized (e);
			this.MainForm = new MainForm();
		}
	}
}

