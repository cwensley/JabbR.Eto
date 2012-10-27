using System;
using Eto.Forms;
using System.Threading.Tasks;
using System.Linq;
using jab = JabbR.Client;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Drawing;

namespace JabbR.Eto.Model.JabbR
{
	public class JabbRChat : JabbRChannel
	{
		User user;
		string initialMessage;
		
		static Image image = Bitmap.FromResource (typeof(JabbRChat).Assembly, "JabbR.Eto.Resources.chat.png");
		
		public override Image Image { get { return image; } }
		
		public override IEnumerable<User> Users {
			get { 
				yield return Server.CurrentUser;
				yield return user;
			}
		}
		
		public override IEnumerable<string> Owners { get { return Enumerable.Empty<string>(); } }
		
		public JabbRChat (JabbRServer server, User user, string initialMessage)
			: base(server)
		{
			this.initialMessage = initialMessage;
			this.user = user;
			this.Name = user.Name;
			this.Id = "chat_" + user.Id;
			if (!string.IsNullOrEmpty (initialMessage))
				UnreadCount = 1;
		}

		public override Task<IEnumerable<ChannelMessage>> GetHistory (string fromId)
		{
			if (string.IsNullOrEmpty (fromId) && !string.IsNullOrEmpty (initialMessage)) {
				var task = new TaskCompletionSource<IEnumerable<ChannelMessage>> ();
				task.SetResult (new ChannelMessage[] { new ChannelMessage(Guid.NewGuid ().ToString (), DateTimeOffset.Now, user.Name, initialMessage) });
				return task.Task;
			}
			return null;
		}
		
		public override void SendMessage (string command)
		{
			/*
			if (!command.TrimStart ().StartsWith ("/")) {
				OnMessageReceived (new MessageEventArgs (new ChannelMessage (Guid.NewGuid ().ToString (), DateTimeOffset.Now, Server.CurrentUser.Name, command)));
			}
			*/
			
			Server.Client.SendPrivateMessage(user.Name, command).ContinueWith (task => {
				Application.Instance.Invoke (() => {
					MessageBox.Show (
						Application.Instance.MainForm,
						string.Format ("Error sending message: {0}", task.Exception)
					);
				});
			}, TaskContinuationOptions.OnlyOnFaulted);
		}
		
		public override void UserTyping ()
		{
		}

		public override void TriggerActivityChanged (IEnumerable<jab.Models.User> users)
		{
			
		}

		internal void TriggerUsernameChanged (string oldUserName, jab.Models.User jabuser, bool isCurrentUser)
		{
			if (this.Name == oldUserName) {
				user.Name = jabuser.Name;
				user.Id = jabuser.Name;
				this.Name = user.Name;
				OnUsernameChanged (new UsernameChangedEventArgs (oldUserName, user, DateTimeOffset.Now));
				OnNameChanged (EventArgs.Empty);
				var server = Server as JabbRServer;
				server.TriggerChannelInfoChanged (new ChannelEventArgs (this));
			}
			else if (isCurrentUser) {
				OnUsernameChanged (new UsernameChangedEventArgs (oldUserName, Server.CurrentUser, DateTimeOffset.Now));
			}
		}

		public override int CompareTo (object obj)
		{
			if (obj is JabbRRoom) return 1;
			else return base.CompareTo (obj);
		}
	}
}

