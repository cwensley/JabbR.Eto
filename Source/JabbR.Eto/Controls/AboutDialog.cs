using System;
using Eto.Forms;

namespace JabbR.Eto.Dialogs
{
	public class AboutDialog : Dialog
	{
		public AboutDialog ()
		{
			var layout = new DynamicLayout(this);
			
			layout.AddCentered (CloseButton());
		}
		
		Control CloseButton()
		{
			var control = new Button{ Text = "Close" };
			control.Click += delegate {
				Close ();
			};
			return control;
		}
	}
}

