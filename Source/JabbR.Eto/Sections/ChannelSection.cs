using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;

namespace JabbR.Eto.Sections
{
	public class ChannelSection : Panel
	{
		WebView history;
		TextBox text;
		JabbRClient client;
		
		public string RoomName { get; set; }
		
		public ChannelSection ()
		{
			history = new WebView();
			
			var layout = new DynamicLayout(this, Padding.Empty, Size.Empty);
			
			layout.Add (history, yscale: true);
			layout.Add (DockLayout.CreatePanel (MessageEntry(), new Padding(10)));
		}
		
		public void Initialize(JabbRClient client, LogOnInfo info)
		{
			client.MessageReceived += (message, room) => {
				if (room == RoomName) {
					var msg = string.Format ("[{0}] {1}: {2}", message.When, message.User.Name, message.Content);
					history.LoadHtml("<html><head></head><body><div>" + msg + "</div></body></html>");
				}
			};
		}

		Control MessageEntry()
		{
			text = new TextBox();
			text.KeyDown += (sender, e) => {
				if (e.KeyData == Key.Enter) {
					ProcessCommand(text.Text);
					text.Text = string.Empty;
				}
			};
			return text;
		}
		
		void ProcessCommand(string command)
		{
			if (command.StartsWith ("/")) {
				
			}
			else if (!string.IsNullOrEmpty (RoomName)) {
				client.Send (command, RoomName);
			}
		}
	}
}

