using System;
using Eto.Forms;
using Eto.Drawing;
using System.IO;
using Eto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JabbR.Eto.Model;

namespace JabbR.Eto.Interface
{
	public class ChannelSection : MessageSection
	{
		string autocompleteText;
		
		public UserList UserList { get; private set; }
		
		public Channel Channel { get; private set; }
		
		public override bool SupportsAutoComplete {
			get { return true; }
		}
		
		public ChannelSection (Channel channel)
		{
			this.Channel = channel;
			this.Channel.MessageReceived += HandleMessageReceived;
			this.Channel.UserJoined += HandleUserJoined;
			this.Channel.UserLeft += HandleUserLeft;
			this.Channel.OwnerAdded += HandleOwnerAdded;
			this.Channel.OwnerRemoved += HandleOwnerRemoved;
			this.Channel.UsersActivityChanged += HandleUsersActivityChanged;
			this.Channel.MessageContent += HandleMessageContent;
		}

		void HandleMessageContent (object sender, MessageContentEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				AddMessageContent (e.Content);
			});
		}

		void HandleUsersActivityChanged (object sender, UsersEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.UsersActivityChanged (e.Users);
			});
		}

		void HandleOwnerRemoved (object sender, UserEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.OwnerRemoved(e.User);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} was removed as an owner", e.User.Name)
			));
		}

		void HandleOwnerAdded (object sender, UserEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.OwnerAdded(e.User);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} was added as an owner", e.User.Name)
			));
		}

		void HandleUserLeft (object sender, UserEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.UserLeft (e.User);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} left {1}", e.User.Name, Channel.Name)
			));
		}

		void HandleUserJoined (object sender, UserEventArgs e)
		{
			Application.Instance.AsyncInvoke(delegate {
				UserList.UserJoined (e.User);
			});
			AddNotification (new NotificationMessage (
				DateTimeOffset.Now,
				string.Format ("{0} just entered {1}", e.User.Name, Channel.Name)
			));
		}

		void HandleMessageReceived (object sender, MessageEventArgs e)
		{
			AddMessage (e.Message);
		}

		protected override void CreateLayout (Container container)
		{
			var split = new Splitter{
				Size = new Size(200, 200),
				Position = 50,
				FixedPanel = SplitterFixedPanel.Panel2
			};
			
			split.Panel1 = new Panel ();
			split.Panel2 = UserList = new UserList (this.Channel);
			
			base.CreateLayout (split.Panel1 as Panel);
			
			container.AddDockedControl (split);
		}

		protected override void HandleDocumentLoaded (object sender, WebViewLoadedEventArgs e)
		{
			base.HandleDocumentLoaded (sender, e);
			if (this.Channel != null) {
				this.Channel.GetChannelInfo ().ContinueWith(task => {
					var channel = task.Result;
					Application.Instance.AsyncInvoke (delegate {
						UserList.SetUsers (channel.Users);
					});
					AddHistory (channel.GetHistory (string.Empty));
				});
				// load up initial room history
				/*Info.Client.GetRoomInfo (this.RoomName).ContinueWith (task => {
					var room = task.Result;
					Application.Instance.AsyncInvoke (delegate {
						UserList.SetUsers (room.Users.Select (r => ), room.Owners);
					}
					);
					
				}, TaskContinuationOptions.OnlyOnRanToCompletion);*/
				
			}
		}

		public void MeMessage (string user, string content)
		{
			SendCommand("addMeMessage", new MeMessage (
				DateTimeOffset.Now, 
				user,
				content
			));
		}
		
		public override void UserTyping ()
		{
			autocompleteText = null;
			Channel.UserTyping ();
		}
		
		public override void ProcessCommand (string command)
		{
			if (string.IsNullOrWhiteSpace (command))
				return;
			
			Channel.SendMessage(command);
		}
		protected override Task<IEnumerable<string>> GetAutoCompleteNames (string search)
		{
			var task = new TaskCompletionSource<IEnumerable<string>> ();
			if (Channel.Server.IsConnected) {
				if (search.StartsWith ("#")) {
					search = search.TrimStart ('#');
					var getChannels = Channel.Server.GetCachedChannels ();
					getChannels.ContinueWith (t => {
						task.TrySetResult (t.Result.Where (r => r.Name.StartsWith (search, StringComparison.CurrentCultureIgnoreCase)).Select (r => r.Name));
					}, TaskContinuationOptions.OnlyOnRanToCompletion);
					getChannels.ContinueWith (t => {
						task.TrySetException (t.Exception);
					}, TaskContinuationOptions.OnlyOnFaulted);
				} else {
					search = search.TrimStart ('@');
					task.TrySetResult (Channel.Users.Where (r => r.Name.StartsWith (search, StringComparison.CurrentCultureIgnoreCase)).Select (r => r.Name));
				}
			}
			return task.Task;
		}
		
		public override string TranslateAutoCompleteText (string selection, string search)
		{
			if (search.StartsWith("#"))
				return '#' + base.TranslateAutoCompleteText (selection, search) + ' ';
			else
				return '@' + base.TranslateAutoCompleteText (selection, search) + ' ';
		}
	}
}

