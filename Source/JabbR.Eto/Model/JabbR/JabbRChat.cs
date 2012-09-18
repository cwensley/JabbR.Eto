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
			UnreadCount = 1;
		}

		public override IEnumerable<ChannelMessage> GetHistory (string fromId)
		{
			if (string.IsNullOrEmpty (fromId) && !string.IsNullOrEmpty (initialMessage)) {
				yield return new ChannelMessage(Guid.NewGuid ().ToString (), DateTimeOffset.Now, user.Name, initialMessage);
			}
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
		
		internal void TriggerActivityChanged (IEnumerable<jab.Models.User> users)
		{
			var theusers = from r in users select new JabbRUser (r);
			OnUsersActivityChanged (new UsersEventArgs (theusers, DateTimeOffset.Now));
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
		
		
		public override void UserTyping ()
		{
		}

		public void SetNewTopic (string topic)
		{
			this.Topic = topic;
			OnTopicChanged (EventArgs.Empty);
		}
	
		public override int CompareTo (object obj)
		{
			if (obj is JabbRRoom) return 1;
			else return base.CompareTo (obj);
		}
	}
}

