using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Linq;
using jab = JabbR.Client;
using System.Collections.Generic;

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
				this.Name = room.Name;
				this.Id = room.Name;
			}
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
					this.users.Clear ();
					this.users.AddRange (from r in task.Result.Users select new JabbRUser (r));
					this.owners.Clear ();
					this.owners.AddRange (task.Result.Owners);
					this.recentMessages = (from m in task.Result.RecentMessages select new ChannelMessage (m.Id, m.When, m.User.Name, m.Content)).ToArray ();
					tcs.SetResult (this);
				}
			}
			);
			
			return tcs.Task;
		}

		internal void TriggerOwnerAdded (jab.Models.User user)
		{
			OnOwnerAdded(new UserEventArgs(new JabbRUser(user), DateTimeOffset.Now));
		}

		internal void TriggerOwnerRemoved (jab.Models.User user)
		{
			OnOwnerRemoved(new UserEventArgs(new JabbRUser(user), DateTimeOffset.Now));
		}		
		
		internal void TriggerMeMessage (string user, string content)
		{
			OnMeMessageReceived (new MessageEventArgs (new ChannelMessage (null, DateTimeOffset.Now, user, content)));
		}
		
		internal void TriggerMessage (jab.Models.Message message)
		{
			OnMessageReceived (new MessageEventArgs (new ChannelMessage (message.Id, message.When, message.User.Name, message.Content)));
		}

	}
}

