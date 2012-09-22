using System;
using jab = JabbR.Client;
using System.Linq;
using Eto.Forms;
using Newtonsoft.Json;
using System.Threading.Tasks;
using JabbR.Eto.Interface.JabbR;
using Eto;
using System.Security;
using SignalR.Client.Transports;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace JabbR.Eto.Model.JabbR
{
	public class JabbRServer : Server
	{
		public const string JabbRTypeId = "jabbr";
		
		public jab.JabbRClient Client { get; private set; }
		
		public jab.Models.LogOnInfo LogOnInfo { get; private set; }
		
		public jab.Models.User CurrentJabbRUser { 
			get { return ((JabbRUser)this.CurrentUser).InnerUser; }
		}
		
		public override string TypeId { get { return JabbRTypeId; } }
		
		public string Address { get; set; }
		
		// TODO: store in keychain
		public string UserName { get; set; }

		// TODO: store in keychain
		public string Password { get; set; }
		
		public string UserId { get; set; }

		public bool UseSocialLogin { get; set; }
		
		public JabbRServer ()
		{
			this.UseSocialLogin = true;
		}
		
		public override void Connect ()
		{
			if (string.IsNullOrEmpty (Address))
				return;
			if (UseSocialLogin) {
				if (string.IsNullOrEmpty (UserId))
					return;
			} else if (string.IsNullOrEmpty (UserName) || string.IsNullOrEmpty (Password))
				return;
			
			OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Connecting...")));
			Client = new jab.JabbRClient (Address);//, new ServerSentEventsTransport());//new LongPollingTransport ());
			HookupEvents ();
			Task<jab.Models.LogOnInfo> connect;
			
			if (UseSocialLogin) {
				connect = Client.Connect (UserId);
			} else {
				connect = Client.Connect (UserName, Password);
			}
				
			connect.ContinueWith (task => {
				if (!task.IsCompleted || task.Exception != null) {
					Debug.WriteLine ("Error: {0}", task.Exception);
					Application.Instance.Invoke (delegate {
						var ex = task.Exception; //.GetBaseException();
						MessageBox.Show (Application.Instance.MainForm, string.Format ("Unable to log in. Reason: {0}", ex.Message));
					});
					return;
				}
				var logOnInfo = task.Result;

				Client.GetUserInfo ().ContinueWith (task2 => {
					if (task2.Exception != null) {
						Debug.WriteLine ("Failed to login {0}", task2.Exception);
						Application.Instance.Invoke (delegate {
							// failed!
						});
					}
					this.CurrentUser = new JabbRUser (task2.Result);
					
					InitializeChannels (logOnInfo.Rooms.Select (r => new JabbRRoom (this, r)));
					
					OnConnected (EventArgs.Empty);
				});
			});
		}
		
		public override Task<IEnumerable<ChannelInfo>> GetChannelList ()
		{
			var resultTask = new TaskCompletionSource<IEnumerable<ChannelInfo>> ();
			
			var getRooms = Client.GetRooms ();
			getRooms.ContinueWith (task => {
				var channels = task.Result.Select (r => {
					return new ChannelInfo (this) {
						Name = r.Name,
						Topic = r.Topic,
						Private = r.Private,
						UserCount = r.Count
					};
				});
				resultTask.TrySetResult (channels);
				channelListTask = resultTask.Task;
			}, TaskContinuationOptions.OnlyOnRanToCompletion);
			
			getRooms.ContinueWith (task => {
				resultTask.TrySetException (task.Exception);
			}, TaskContinuationOptions.OnlyOnFaulted);
			
			return resultTask.Task;
		}
		
		Task<IEnumerable<ChannelInfo>> channelListTask;

		public override Task<IEnumerable<ChannelInfo>> GetCachedChannels ()
		{
			return channelListTask ?? GetChannelList ();
		}
		
		public override void Disconnect ()
		{
			if (Client != null)
				Client.Disconnect ();
		}
		
		public JabbRRoom GetRoom (string roomName)
		{
			return Channels.OfType<JabbRRoom> ().FirstOrDefault (r => r.Name == roomName);
		}

		public JabbRChat GetChat (string userName)
		{
			var id = "chat_" + userName;
			return Channels.OfType<JabbRChat> ().FirstOrDefault (r => r.Id == id);
		}
		
		void HookupEvents ()
		{
			Client.MessageReceived += (message, room) => {
				Debug.WriteLine ("MessageReceived, Room: {3}, When: {0}, User: {1}, Content: {2}", message.When, message.User.Name, message.Content, room);
				var channel = GetRoom (room);
				if (channel != null) {
					channel.TriggerMessage (JabbRRoom.CreateMessage (message));
					OnChannelInfoChanged (new ChannelEventArgs (channel));
				}
			};
			Client.Disconnected += () => {
				Debug.WriteLine ("Disconnected");
				OnDisconnected (EventArgs.Empty);
			};
			Client.FlagChanged += (user, flag) => {
				Debug.WriteLine ("FlagChanged, User: {0}, Flag: {1}", user.Name, flag);
			};
			Client.GravatarChanged += (user, gravatar) => {
				Debug.WriteLine ("GravatarChanged, User: {0}, Gravatar: {1}", user.Name, gravatar);
			};
			Client.Kicked += (user) => {
				Debug.WriteLine ("Kicked: {0}", user);
			};
			Client.LoggedOut += (rooms) => {
				Debug.WriteLine ("LoggedOut, Rooms: {0}", string.Join (", ", rooms));
			};
			Client.MeMessageReceived += (user, content, room) => {
				Debug.WriteLine ("MeMessageReceived, User: {0}, Content: {1}, Room: {2}", user, content, room);
				var channel = GetRoom (room);
				if (channel != null) 
					channel.TriggerMeMessage (user, content);
			};
			Client.NoteChanged += (user, room) => {
				Debug.WriteLine ("NoteChanged, User: {0}, Room: {1}", user.Name, room);
			};
			Client.OwnerAdded += (user, room) => {
				Debug.WriteLine ("OwnerAdded, User: {0}, Room: {1}", user.Name, room);
				var channel = GetRoom (room);
				if (channel != null) 
					channel.TriggerOwnerAdded (user);
			};
			Client.OwnerRemoved += (user, room) => {
				Debug.WriteLine ("OwnerRemoved, User: {0}, Room: {1}", user.Name, room);
				var channel = GetRoom (room);
				if (channel != null) 
					channel.TriggerOwnerRemoved (user);
			};
			Client.PrivateMessage += (from, to, message) => {
				Debug.WriteLine ("PrivateMessage, From: {0}, To: {1}, Message: {2} ", from, to, message);
				
				var user = from == this.CurrentUser.Name ? to : from;
				JabbRChat chat;
				if (InternalStartChat (new JabbRUser(user) { Active = true }, false, message, out chat)) {
					chat.TriggerMessage (new ChannelMessage(Guid.NewGuid ().ToString (), DateTimeOffset.Now, from, message));
					OnChannelInfoChanged (new ChannelEventArgs (chat));
				}
			};
			Client.RoomCountChanged += (room, count) => {
				Debug.WriteLine ("RoomCountChanged, Room: {0}, Count: {1}", room.Name, count);
			};
			Client.TopicChanged += (room) => {
				Debug.WriteLine ("TopicChanged, Room: {0}, Topic: {1}", room.Name, room.Topic);
				var channel = GetRoom (room.Name);
				if (channel != null) {
					channel.SetNewTopic (room.Topic);
				}
			};
			Client.UserActivityChanged += (user) => {
				Debug.WriteLine ("UserActivityChanged, User: {0}, Activity: {1}", user.Name, user.Active);
				foreach (var channel in this.Channels.OfType<JabbRChannel>()) {
					channel.TriggerActivityChanged (new jab.Models.User[] { user });
				}
			};
			Client.JoinedRoom += (room) => {
				Debug.WriteLine ("JoinedRoom, Room: {0}", room.Name);
				var channel = GetRoom (room.Name);
				if (channel == null) {
					channel = new JabbRRoom (this, room);
				}
				OnOpenChannel (new OpenChannelEventArgs (channel, true));
			};
			Client.UserJoined += (user, room, isOwner) => {
				Debug.WriteLine ("UserJoined, User: {0}, Room: {1}", user.Name, room);
				var channel = GetRoom (room);
				if (channel != null) {
					if (isOwner)
						channel.TriggerOwnerAdded (user);
					channel.TriggerUserJoined (new UserEventArgs (new JabbRUser (user), DateTimeOffset.Now));
				}
			};
			Client.UserLeft += (user, room) => {
				Debug.WriteLine ("UserLeft, User: {0}, Room: {1}", user.Name, room);
				var channel = GetRoom (room);
				if (channel != null) {
					if (user.Name == CurrentJabbRUser.Name) {
						channel.TriggerClosed (EventArgs.Empty);
						OnCloseChannel (new ChannelEventArgs (channel));
					} else {
						channel.TriggerUserLeft (new UserEventArgs (new JabbRUser (user), DateTimeOffset.Now));
					}
				}
			};
			Client.UsernameChanged += (oldUserName, user, room) => {
				Debug.WriteLine ("UsernameChanged, OldUserName: {0}, NewUserName: {1}, Room: {2}", oldUserName, user.Name, room);
			};
			Client.UsersInactive += (users) => {
				Debug.WriteLine ("UsersInactive, Users: {0}", string.Join (", ", users.Select (r => r.Name)));
				foreach (var channel in this.Channels.OfType<JabbRChannel>()) {
					channel.TriggerActivityChanged (users);
				}
			};
			Client.UserTyping += (user, room) => {
				Debug.WriteLine ("UserTyping, User: {0}, Room: {1}", user.Name, room);	
			};
			Client.AddMessageContent += (messageId, content, room) => {
				Debug.WriteLine ("AddMessageContent, Id: {0}, Room: {1}, Content: {2}", messageId, room, content);
				var channel = GetRoom (room);
				if (channel != null)
					channel.TriggerMessageContent (messageId, content);
			};
			/*
			Client.StateChanged += (status) => {
				Debug.WriteLine ("StateChange, Old State: {0}, New State: {1}", status.OldState, status.NewState);
			};
			*/
		}
		
		public override void SendMessage (string command)
		{
			if (Client == null) {
				OnGlobalMessageReceived (new NotificationEventArgs (new NotificationMessage (DateTimeOffset.Now, "Cannot send command. You are not connected.")));
				return;
			}
			var message = new global::JabbR.Client.Models.ClientMessage{
				Id = Guid.NewGuid ().ToString (),
				Content = command
			};
			Client.Send (message).ContinueWith (task => {
				Application.Instance.Invoke (() => {
					MessageBox.Show (Application.Instance.MainForm, string.Format ("Error sending message: {0}", task.Exception));
				});
			}, TaskContinuationOptions.OnlyOnFaulted);
		}
		
		public override void JoinChannel (string name)
		{
			var room = GetRoom (name);
			if (room == null)
				Client.JoinRoom (name);
			else
				OnOpenChannel (new OpenChannelEventArgs(room, true));
		}
		
		public override void LeaveChannel (Channel channel)
		{
			var chat = channel as JabbRChat;
			if (chat != null) {
				chat.TriggerClosed (EventArgs.Empty);
				OnCloseChannel (new ChannelEventArgs (chat));
			}
			else
				Client.LeaveRoom (channel.Id);
		}
		
		public override void StartChat (User user)
		{
			JabbRChat chat;
			InternalStartChat(user, true, null, out chat);
		}
		
		bool InternalStartChat (User user, bool shouldFocus, string initialMessage, out JabbRChat chat)
		{
			if (user.Id == this.CurrentUser.Id) {
				chat = null;
				return false;
			}
			
			chat = GetChat (user.Name);
			if (chat == null) {
				chat = new JabbRChat(this, user, initialMessage);
				OnOpenChannel (new OpenChannelEventArgs (chat, shouldFocus));
				return false;
			}
			return true;
			
		}
		
		public override void GenerateEditControls (DynamicLayout layout)
		{
			base.GenerateEditControls (layout);
			new JabbRServerEdit (this, layout);
		}
		
		public override Control GenerateSection ()
		{
			var section = new Interface.ServerSection (this);
			section.Initialize ();
			return section;
		}
		
		public override void ReadXml (System.Xml.XmlElement element)
		{
			base.ReadXml (element);
			this.UseSocialLogin = element.GetBoolAttribute ("useSocialLogin") ?? false;
			this.Address = element.GetStringAttribute ("address");
			if (this.UseSocialLogin) {
				
				this.UserId = JabbRApplication.Instance.DecryptString(this.Address, this.Id, element.GetStringAttribute ("userId"));
			} else {
				this.UserName = JabbRApplication.Instance.DecryptString(this.Address, this.Id + "_user", element.GetStringAttribute ("userName"));
				this.Password = JabbRApplication.Instance.DecryptString(this.Address, this.Id + "_pass", element.GetStringAttribute ("password"));
			}
		}
		
		public override void WriteXml (System.Xml.XmlElement element)
		{
			base.WriteXml (element);
			element.SetAttribute ("useSocialLogin", this.UseSocialLogin);
			element.SetAttribute ("address", this.Address);
			if (this.UseSocialLogin)
				element.SetAttribute ("userId", JabbRApplication.Instance.EncryptString (this.Address, this.Id, this.UserId));
			else {
				element.SetAttribute ("userName", JabbRApplication.Instance.EncryptString (this.Address, this.Id + "_user", this.UserName));
				element.SetAttribute ("password", JabbRApplication.Instance.EncryptString (this.Address, this.Id + "_pass", this.Password));
			}
		}
	}
}

