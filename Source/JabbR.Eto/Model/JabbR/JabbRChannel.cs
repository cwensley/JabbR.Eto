using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Linq;
using jab = JabbR.Client;
using System.Collections.Generic;
using System.Diagnostics;

namespace JabbR.Eto.Model.JabbR
{
	public class JabbRChannel : Channel
	{
		List<User> users = new List<User> ();
		List<string> owners = new List<string> ();
		IEnumerable<ChannelMessage> recentMessages;
		
		public override IEnumerable<User> Users { get { return users; } }
		
		public override IEnumerable<string> Owners { get { return owners; } }
		
		public new JabbRServer Server {
			get { return base.Server as JabbRServer; }
		}
		
		public JabbRChannel (JabbRServer server, jab.Models.Room room = null)
			: base(server)
		{
			if (room != null) {
				Set (room);
			}
		}

		void Set (jab.Models.Room room)
		{
			this.Name = room.Name;
			this.Id = room.Name;
			this.Topic = room.Topic;
			this.Private = room.Private;
		}
		
		public override IEnumerable<ChannelMessage> GetHistory (string fromId)
		{
			if (string.IsNullOrEmpty (fromId)) {
				return recentMessages ?? Enumerable.Empty<ChannelMessage> ();
			}
			return Enumerable.Empty<ChannelMessage> ();
		}
		
		public override void SendMessage (string command)
		{
			var message = new global::JabbR.Client.Models.ClientMessage{
				Id = Guid.NewGuid ().ToString (),
				Room = this.Name,
				Content = command
			};
			if (!command.TrimStart ().StartsWith ("/")) {
				OnMessageReceived (new MessageEventArgs (new ChannelMessage (message.Id, DateTimeOffset.Now, Server.CurrentUser.Name, command)));
			}
			Server.Client.Send (message).ContinueWith (task => {
				Application.Instance.Invoke (() => {
					MessageBox.Show (
						Application.Instance.MainForm,
						string.Format ("Error sending message: {0}", task.Exception)
					);
				}
				);
			}, TaskContinuationOptions.OnlyOnFaulted);
		}
		
		internal void TriggerClosed (EventArgs e)
		{
			OnClosed (e);
		}
		
		internal void TriggerUserLeft (UserEventArgs e)
		{
			OnUserLeft (e);
			this.users.Remove (e.User);
		}
		
		internal void TriggerUserJoined (UserEventArgs e)
		{
			OnUserJoined (e);
			this.users.Add (e.User);
		}
		
		public override Task<Channel> GetChannelInfo ()
		{
			var tcs = new TaskCompletionSource<Channel> ();
			Server.Client.GetRoomInfo (this.Id).ContinueWith (task => {
				if (task.Exception != null)
					tcs.SetException (task.Exception);
				else {
					Set (task.Result);
					this.users.Clear ();
					this.users.AddRange (from r in task.Result.Users select new JabbRUser (r));
					this.owners.Clear ();
					this.owners.AddRange (task.Result.Owners);
					this.recentMessages = (from m in task.Result.RecentMessages select new ChannelMessage (m.Id, m.When, m.User.Name, m.Content)).ToArray ();
					tcs.SetResult (this);
				}
			});
			
			return tcs.Task;
		}

		internal void TriggerActivityChanged (IEnumerable<jab.Models.User> users)
		{
			var theusers = from r in users select new JabbRUser (r);
			OnUsersActivityChanged (new UsersEventArgs (theusers, DateTimeOffset.Now));
		}
		
		internal void TriggerOwnerAdded (jab.Models.User user)
		{
			if (!this.owners.Contains (user.Name)) {
				bool added = false;
				lock (owners) {
					if (!this.owners.Contains (user.Name)) {
						owners.Add (user.Name);
						added = true;
					};
				}
				if (added)
					OnOwnerAdded (new UserEventArgs (new JabbRUser (user), DateTimeOffset.Now));
			}
		}

		internal void TriggerOwnerRemoved (jab.Models.User user)
		{
			if (this.owners.Contains (user.Name)) {
				bool removed = false;
				lock (owners) {
					if (this.owners.Contains (user.Name)) {
						owners.Remove (user.Name);
						removed = true;
					}
				}
				if (removed)
					OnOwnerRemoved (new UserEventArgs (new JabbRUser (user), DateTimeOffset.Now));
			}
		}
		
		internal void TriggerMeMessage (string user, string content)
		{
			OnMeMessageReceived (new MessageEventArgs (new ChannelMessage (null, DateTimeOffset.Now, user, content)));
		}
		
		internal void TriggerMessage (jab.Models.Message message)
		{
			UnreadCount ++;
			OnMessageReceived (new MessageEventArgs (new ChannelMessage (message.Id, message.When, message.User.Name, message.Content)));
		}
		
		internal void TriggerMessageContent (string messageId, string content)
		{
			OnMessageContent (new MessageContentEventArgs (new MessageContent (messageId, content)));
		}
		
		static object typinglock = new object ();
		static string lastTypingRoom;
		static DateTime? lastTypingTime;
		
		public override void UserTyping ()
		{
			lock (typinglock) {
				if (lastTypingRoom != this.Name || (lastTypingTime != null && lastTypingTime.Value < DateTime.Now)) {
					lastTypingTime = DateTime.Now.AddSeconds (5);
					lastTypingRoom = this.Name;
					Debug.WriteLine (string.Format ("UserTyping, Room:{0}", this.Name));
					Server.Client.SetTyping (this.Name);
				}
			}
		}

		public void SetNewTopic (string topic)
		{
			this.Topic = topic;
			OnTopicChanged (EventArgs.Empty);
		}
	}
}

