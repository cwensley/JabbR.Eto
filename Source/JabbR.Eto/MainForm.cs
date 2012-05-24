using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Eto.Dialogs;
using JabbR.Eto.Sections;
using JabbR.Client;

namespace JabbR.Eto
{
	public class MainForm : Form
	{
		TopSection top;
		LoginSection login;
		bool loggedIn;
		
		public MainForm ()
		{
			this.ClientSize = new Size (800, 600);
			top = new TopSection ();
			login = new LoginSection ();
			login.LoggedIn += HandleLoggedIn;
			UpdateSection ();
			CreateActions ();
		}

		void HandleLoggedIn (object sender, EventArgs e)
		{
			loggedIn = true;
			top.Initialize(login.Client, login.LogOnInfo);
			UpdateSection ();
		}
		
		void UpdateSection ()
		{
			if (loggedIn)
				this.AddDockedControl (top);
			else
				this.AddDockedControl (login);
		}
		
		void CreateActions ()
		{
			var args = new GenerateActionArgs ();
			
			Application.Instance.GetSystemActions (args, true);
			
			
			args.Actions.Add (new Actions.Quit ());
			args.Actions.Add (new Actions.About ());
			
			var file = args.Menu.FindAddSubMenu ("&File", 100);
			
			file.Actions.Add (Actions.About.ActionID, 100);
			file.Actions.Add (Actions.Quit.ActionID, 900);
			
			this.Menu = args.Menu.GenerateMenuBar ();
		}

	}
}

