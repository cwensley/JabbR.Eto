using System;
using Eto.Forms;
using Eto;
using System.IO;

namespace JabbR.Eto
{
	public interface IJabbRApplication : IApplication
	{
		string BadgeLabel { get; set; }
	}
	
	public class JabbRApplication : Application
	{
		IJabbRApplication handler;
		public static new JabbRApplication Instance
		{
			get { return Application.Instance as JabbRApplication; }
		}
		
		public JabbRApplication ()
			: base(Generator.Detect, typeof(IJabbRApplication))
		{
			this.Style = "application";
			this.Name = "JabbR.Eto";
			HandleEvent (TerminatingEvent);
			handler = (IJabbRApplication)Handler;
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
			this.BadgeLabel = null;
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
		
		public string BadgeLabel {
			get { return handler.BadgeLabel; }
			set { handler.BadgeLabel = value; }
		}
	}
}

