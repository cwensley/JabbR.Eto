using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Eto.Dialogs;
using JabbR.Eto.Sections;
using JabbR.Client;
using Eto;
using System.Xml;
using System.IO;

namespace JabbR.Eto
{
	public class MainForm : Form, IXmlReadable
	{
		TopSection top;
		LoginSection login;
		bool loggedIn;
		
		
		public MainForm ()
		{
			this.ClientSize = new Size (800, 600);
			this.Style = "mainForm";
			top = new TopSection ();
			login = new LoginSection ();
			login.LoggedIn += HandleLoggedIn;
			login.Initialized += HandleInitialized;
			CreateActions ();
			UpdateActiveSection ();
			HandleEvent (ShownEvent);
		}
		
		public override void OnShown (EventArgs e)
		{
			base.OnShown (e);
			login.UpdateDisplay ();
		}
		
		void HandleInitialized (object sender, EventArgs e)
		{
			top.Initialize(login.Info);
		}

		void HandleLoggedIn (object sender, EventArgs e)
		{
			loggedIn = true;
			top.Connected();
			UpdateActiveSection ();
		}
		
		void UpdateActiveSection ()
		{
			if (loggedIn)
				this.AddDockedControl (top);
			else {
				this.AddDockedControl (login);
			}
		}
		
		void CreateActions ()
		{
			var args = new GenerateActionArgs ();
			
			Application.Instance.GetSystemActions (args, true);
			
			
			args.Actions.Add (new Actions.Quit ());
			args.Actions.Add (new Actions.About ());
			
			if (Generator.ID == "mac") {
				var application = args.Menu.FindAddSubMenu (Application.Instance.Name, 100);
				application.Actions.Add (Actions.About.ActionID, 100);
				application.Actions.Add (Actions.Quit.ActionID, 900);
			}
			else {
				var file = args.Menu.FindAddSubMenu ("&File", 100);
				file.Actions.Add (Actions.Quit.ActionID, 900);
				
				var help = args.Menu.FindAddSubMenu ("&Help", 900);
				help.Actions.Add (Actions.About.ActionID, 100);
			}
			
			this.Menu = args.Menu.GenerateMenuBar ();
		}

		public void Disconnect ()
		{
			if (login.Info != null)
				login.Info.Client.Disconnect ();
		}

		#region IXmlReadable implementation
		
		public void ReadXml (XmlElement element)
		{
			this.ClientSize = element.ReadChildSizeXml("clientSize") ?? new Size(800, 600);
			element.ReadChildXml ("top", top);
			element.ReadChildXml ("login", login);
		}

		public void WriteXml (XmlElement element)
		{
			element.WriteChildXml ("clientSize", this.ClientSize);
			element.WriteChildXml ("top", top);
			element.WriteChildXml ("login", login);
		}
		
		#endregion
	}
}

