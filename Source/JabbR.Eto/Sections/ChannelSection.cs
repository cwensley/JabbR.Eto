using System;
using Eto.Forms;
using Eto.Drawing;
using JabbR.Client;
using JabbR.Client.Models;
using System.IO;
using Eto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR.Eto.Messages;
using JabbR.Eto.Controls;

namespace JabbR.Eto.Sections
{
	public class ChannelSection : MessageSection
	{
		public UserList UserList { get; private set; }
		
		public event EventHandler<CommandEventArgs> Command;

		protected virtual void OnCommand (CommandEventArgs e)
		{
			if (Command != null)
				Command (this, e);
		}
		
		public string RoomName { get; private set; }
		
		public ChannelSection (ConnectionInfo info, string roomName)
			: base(info)
		{
			this.RoomName = roomName;
		}

		protected override void CreateLayout (Container container)
		{
			var split = new Splitter{
				Size = new Size(200, 200),
				Position = 50,
				FixedPanel = SplitterFixedPanel.Panel2
			};
			
			split.Panel1 = new Panel ();
			split.Panel2 = UserList = new UserList (this.Info, this.RoomName);
			
			base.CreateLayout (split.Panel1 as Panel);
			
			container.AddDockedControl (split);
		}

		protected override void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			base.HandleDocumentLoaded (sender, e);
			if (this.RoomName != null) {
				// load up initial room history
				Info.Client.GetRoomInfo (this.RoomName).ContinueWith (task => {
					var room = task.Result;
					Application.Instance.AsyncInvoke (delegate {
						UserList.SetUsers (room.Users, room.Owners);
					}
					);
					AddHistory (from m in room.RecentMessages select new ChannelMessage (
						m.Id,
						m.When,
						m.User.Name,
						m.Content
					));
					
				}, TaskContinuationOptions.OnlyOnRanToCompletion);
				
			}
		}
		
		public void UserJoined (User user)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.UserJoined (user);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} just entered {1}", user.Name, RoomName)
			));
		}

		public void UserLeft (User user)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.UserLeft (user);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} left {1}", user.Name, RoomName)
			));
		}

		public override void ProcessCommand (string command)
		{
			if (string.IsNullOrWhiteSpace (command))
				return;
			
			if (command.StartsWith ("/")) {
				OnCommand (new CommandEventArgs (command));
			} else if (RoomName != null && Info != null) {
				var message = new ClientMessage{
					Id = Guid.NewGuid ().ToString (),
					Room = RoomName,
					Content = command
				};
				AddMessage (new ChannelMessage (
					message.Id,
					DateTimeOffset.Now,
					Info.CurrentUser.Name,
					command
				));
				Info.Client.Send (message).ContinueWith (task => {
					Application.Instance.Invoke (() => {
						MessageBox.Show (
							this,
							string.Format ("Error sending message: {0}", task.Exception)
						);
					}
					);
				}, TaskContinuationOptions.OnlyOnFaulted);
			}
#if DEBUG
			else {
				if (command.StartsWith("notify "))
					AddNotification(new NotificationMessage(DateTimeOffset.Now, command.Substring(7)));
				else
					AddMessage(new ChannelMessage(Guid.NewGuid().ToString(), DateTimeOffset.Now, "me", command));
			}
#endif
		}

		public override void Focus ()
		{
			TextEntry.Focus ();
		}
	}
}

