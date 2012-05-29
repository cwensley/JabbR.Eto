using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;
using System.IO;
using Eto;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR.Eto.Messages;

namespace JabbR.Eto.Sections
{
	
	public class ChannelSection : Panel
	{
		WebView history;
		TextBox text;
		ConnectionInfo info;
		string lastHistoryMessageId;
		
		public event EventHandler<CommandEventArgs> Command;

		protected virtual void OnCommand (CommandEventArgs e)
		{
			if (Command != null)
				Command (this, e);
		}
		
		public string RoomName { get; private set; }
		
		public ChannelSection (ConnectionInfo info, string roomName)
		{
			this.RoomName = roomName;
			this.info = info;
			
			history = new WebView ();
			history.DocumentLoaded += HandleDocumentLoaded;
			
			var layout = new DynamicLayout (this, Padding.Empty, Size.Empty);
			
			layout.Add (history, yscale: true);
			layout.Add (DockLayout.CreatePanel (MessageEntry (), new Padding (10)));
		}

		void HandleDocumentLoading (object sender, WebViewLoadingEventArgs e)
		{
			e.Cancel = true;
			Application.Instance.Open (e.Uri.AbsoluteUri);
		}
		
		public override void OnLoadComplete (EventArgs e)
		{
			base.OnLoadComplete (e);
			
			
			var resourcePath = EtoEnvironment.GetFolderPath (EtoSpecialFolder.ApplicationResources);
			resourcePath = Path.Combine (
				resourcePath,
				"Styles",
				"default",
				"channel.html"
			);
			history.Url = new Uri (resourcePath);
		}

		void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			if (this.RoomName != null) {
				history.DocumentLoading += HandleDocumentLoading;
				// load up initial room history
				info.Client.GetRoomInfo(this.RoomName).ContinueWith(task => {
					LoadHistory (task.Result.RecentMessages);
				}, TaskContinuationOptions.OnlyOnRanToCompletion);
				
			}
		}
		
		void LoadHistory (IEnumerable<Message> messages)
		{
			if (messages == null) return;
			
			// TODO: send this as an array instead of adding one at a time
			foreach (var message in messages.OrderByDescending(r => r.When)) {
				AddMessage (new ChannelMessage (message.Id, message.When, message.User.Name, message.Content) {
					IsHistory = true
				});
				lastHistoryMessageId = message.Id;
			}
			
		}
		
		public void MessageReceived (Message message)
		{
			AddMessage (new ChannelMessage(message.Id, message.When, message.User.Name, message.Content));
		}
		
		public void UserJoined (User user)
		{
			AddMessage (new NotificationMessage {
				Time = DateTimeOffset.Now.ToString("h:MM:ss tt"),
				User = user.Name,
				Content = string.Format ("{0} just entered {1}", user.Name, RoomName)}
			);
		}

		public void UserLeft (User user)
		{
			AddMessage (new NotificationMessage {
				Time = DateTimeOffset.Now.ToString("h:MM:ss tt"),
				User = user.Name,
				Content = string.Format ("{0} left {1}", user.Name, RoomName)}
			);
		}
		
		void AddMessage (BaseMessage message)
		{
			var msgString = JsonConvert.SerializeObject (message);
			var script = string.Format ("JabbREto.addMessage({0})", msgString);
			Application.Instance.Invoke (() => {
				history.ExecuteScript (script);
			}
			);
		}

		Control MessageEntry ()
		{
			text = new TextBox ();
			text.KeyDown += (sender, e) => {
				if (e.KeyData == Key.Enter) {
					ProcessCommand (text.Text);
					text.Text = string.Empty;
					e.Handled = true;
				}
			};
			return text;
		}
		
		void ProcessCommand (string command)
		{
			if (string.IsNullOrWhiteSpace (command))
				return;
			
			if (command.StartsWith ("/")) {
				OnCommand (new CommandEventArgs (command));
			} else if (RoomName != null && info != null) {
				var message = new ClientMessage{
					Id = Guid.NewGuid ().ToString (),
					Room = RoomName,
					Content = command
				};
				AddMessage(new ChannelMessage(message.Id, DateTimeOffset.Now, info.CurrentUser.Name, command));
				info.Client.Send (message).ContinueWith(task => {
					Application.Instance.Invoke(() => {
						MessageBox.Show (this, string.Format ("Error sending message: {0}", task.Exception));
					});
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
/*#if DEBUG
			else {
				BaseMessage message;
				if (command.StartsWith("notify"))
					message = new NotificationMessage();
				else
					message = new ChannelMessage();
				message.Time = DateTimeOffset.Now.ToString("h:MM:ss tt");
				message.User = "me";
				message.Content = command;
				
				AddMessage (message);
			}
#endif
*/
		}
	}
}

