using System;
using Eto.Forms;
using Eto;
using System.IO;

namespace JabbR.Eto
{
	public class JabbRApplication : Application
	{
		public JabbRApplication ()
		{
			this.Style = "application";
			this.Name = "JabbR.Eto";
			HandleEvent (TerminatingEvent);
		}
		
		string SettingsFileName
		{
			get { 
				var path = EtoEnvironment.GetFolderPath (EtoSpecialFolder.ApplicationSettings);
				return Path.Combine (path, "settings.xml");
			}
		}
		
		public override void OnInitialized (EventArgs e)
		{
			base.OnInitialized (e);
			var form = new MainForm();
			if (File.Exists (SettingsFileName)) {
				form.LoadXml (SettingsFileName);
			}
			
			this.MainForm = form;
			this.MainForm.Show ();
		}
		
		public override void OnTerminating (System.ComponentModel.CancelEventArgs e)
		{
			base.OnTerminating (e);
			var form = this.MainForm as IXmlReadable;
			if (form != null)
				form.SaveXml (SettingsFileName, "jabbreto");
		}
	}
}

