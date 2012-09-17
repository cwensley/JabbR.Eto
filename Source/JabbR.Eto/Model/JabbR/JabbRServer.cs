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
					Console.WriteLine ("Error: {0}", task.Exception);
					Application.Instance.Invoke (delegate {
						var ex = task.Exception; //.GetBaseException();
						MessageBox.Show (Application.Instance.MainForm, string.Format ("Unable to log in. Reason: {0}", ex.Message));
					});
					return;
				}
				var logOnInfo = task.Result;

				Client.GetUserInfo ().ContinueWith (task2 => {
					if (task2.Exception != null) {
						Console.WriteLine ("Failed to login {0}", task2.Exception);
						Application.Instance.Invoke (delegate {
							// failed!
						});
					}
					this.CurrentUser = new JabbRUser (task2.Result);
					
					InitializeChannels (logOnInfo.Rooms.Select (r => new JabbRChannel (this, r)));
					
					Application.Instance.Invoke (delegate {
						OnConnected (EventArgs.Empty);
					});
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
		
		public JabbRChannel GetChannel (string roomName)
		{
			return Channels.OfType<JabbRChannel> ().FirstOrDefault (r => r.Name == roomName);
		}

		void HookupEvents ()
		{
			Client.MessageReceived += (message, room) => {
				Console.WriteLine ("MessageReceived, Room: {3}, When: {0}, User: {1}, Content: {2}", message.When, message.User.Name, message.Content, room);
				var channel = GetChannel (room);
				if (channel != null) {
					channel.TriggerMessage (message);
					OnChannelInfoChanged (new ChannelEventArgs (channel));
				}
			};
			Client.Disconnected += () => {
				Console.WriteLine ("Disconnected");
				OnDisconnected (EventArgs.Empty);
			};
			Client.FlagChanged += (user, flag) => {
				Console.WriteLine ("FlagChanged, User: {0}, Flag: {1}", user.Name, flag);
			};
			Client.GravatarChanged += (user, gravatar) => {
				Console.WriteLine ("GravatarChanged, User: {0}, Gravatar: {1}", user.Name, gravatar);
			};
			Client.Kicked += (user) => {
				Console.WriteLine ("Kicked: {0}", user);
			};
			Client.LoggedOut += (rooms) => {
				Console.WriteLine ("LoggedOut, Rooms: {0}", string.Join (", ", rooms));
			};
			Client.MeMessageReceived += (user, content, room) => {
				Console.WriteLine ("MeMessageReceived, User: {0}, Content: {1}, Room: {2}", user, content, room);
				var channel = GetChannel (room);
				if (channel != null) 
					channel.TriggerMeMessage (user, content);
			};
			Client.NoteChanged += (user, room) => {
				Console.WriteLine ("NoteChanged, User: {0}, Room: {1}", user.Name, room);
			};
			Client.OwnerAdded += (user, room) => {
				Console.WriteLine ("OwnerAdded, User: {0}, Room: {1}", user.Name, room);
				var channel = GetChannel (room);
				if (channel != null) 
					channel.TriggerOwnerAdded (user);
			};
			Client.OwnerRemoved += (user, room) => {
				Console.WriteLine ("OwnerRemoved, User: {0}, Room: {1}", user.Name, room);
				var channel = GetChannel (room);
				if (channel != null) 
					channel.TriggerOwnerRemoved (user);
			};
			Client.PrivateMessage += (from, to, message) => {
				Console.WriteLine ("PrivateMessage, From: {0}, To: {1}, Message: {2} ", from, to, message);
			};
			Client.RoomCountChanged += (room, count) => {
				Console.WriteLine ("RoomCountChanged, Room: {0}, Count: {1}", room.Name, count);
			};
			Client.TopicChanged += (room) => {
				Console.WriteLine ("TopicChanged, Room: {0}, Topic: {1}", room.Name, room.Topic);
			};
			Client.UserActivityChanged += (user) => {
				Console.WriteLine ("UserActivityChanged, User: {0}, Activity: {1}", user.Name, user.Active);
				foreach (var channel in this.Channels.OfType<JabbRChannel>()) {
					channel.TriggerActivityChanged (new jab.Models.User[] { user });
				}
			};
			Client.JoinedRoom += (room) => {
				Console.WriteLine ("JoinedRoom, Room: {0}", room.Name);
				var channel = new JabbRChannel (this, room);
				OnOpenChannel (new ChannelEventArgs (channel));
			};
			Client.UserJoined += (user, room, isOwner) => {
				Console.WriteLine ("UserJoined, User: {0}, Room: {1}", user.Name, room);
				var channel = GetChannel (room);
				if (channel != null) {
					if (isOwner)
						channel.TriggerOwnerAdded (user);
					channel.TriggerUserJoined (new UserEventArgs (new JabbRUser (user), DateTimeOffset.Now));
				}
			};
			Client.UserLeft += (user, room) => {
				Console.WriteLine ("UserLeft, User: {0}, Room: {1}", user.Name, room);
				var channel = GetChannel (room);
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
				Console.WriteLine ("UsernameChanged, OldUserName: {0}, NewUserName: {1}, Room: {2}", oldUserName, user.Name, room);
			};
			Client.UsersInactive += (users) => {
				Console.WriteLine ("UsersInactive, Users: {0}", string.Join (", ", users.Select (r => r.Name)));
				foreach (var channel in this.Channels.OfType<JabbRChannel>()) {
					channel.TriggerActivityChanged (users);
				}
			};
			Client.UserTyping += (user, room) => {
				Console.WriteLine ("UserTyping, User: {0}, Room: {1}", user.Name, room);	
			};
			Client.AddMessageContent += (messageId, content, room) => {
				Console.WriteLine ("AddMessageContent, Id: {0}, Room: {1}, Content: {2}", messageId, room, content);
				var channel = GetChannel (room);
				if (channel != null)
					channel.TriggerMessageContent (messageId, content);
			};
			/*
			Client.StateChanged += (status) => {
				Console.WriteLine ("StateChange, Old State: {0}, New State: {1}", status.OldState, status.NewState);
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
			Client.JoinRoom (name);
		}
		
		public override void LeaveChannel (string name)
		{
			Client.LeaveRoom (name);
		}
		
		public override void GenerateEditControls (DynamicLayout layout)
		{
			base.GenerateEditControls (layout);
			new JabbRServerEdit (this, layout);
		}
		
		public override Control GenerateSection ()
		{
			return new Interface.ServerSection (this);
		}
		
		const string Salt = "JabbR.Eto";
		static byte[] saltBytes = Encoding.UTF8.GetBytes (Salt);
		
		public override void ReadXml (System.Xml.XmlElement element)
		{
			base.ReadXml (element);
			this.UseSocialLogin = element.GetBoolAttribute ("useSocialLogin") ?? false;
			this.Address = element.GetStringAttribute ("address");
			if (this.UseSocialLogin) {
				try {
					var token = element.GetStringAttribute ("userId");
					if (!string.IsNullOrEmpty (token))
						this.UserId = Encoding.UTF8.GetString (ProtectedData.Unprotect (Convert.FromBase64String (token), saltBytes, DataProtectionScope.CurrentUser));
				} catch {
				}
			} else {
				try {
					var userName = element.GetStringAttribute ("userName");
					if (!string.IsNullOrEmpty (userName))
						this.UserName = Encoding.UTF8.GetString (ProtectedData.Unprotect (Convert.FromBase64String (userName), saltBytes, DataProtectionScope.CurrentUser));
				} catch {
				}
				try {
					var password = element.GetStringAttribute ("password");
					if (!string.IsNullOrEmpty (password))
						this.Password = Encoding.UTF8.GetString (ProtectedData.Unprotect (Convert.FromBase64String (password), saltBytes, DataProtectionScope.CurrentUser));
				} catch {
				}
			}
		}
		
		public override void WriteXml (System.Xml.XmlElement element)
		{
			base.WriteXml (element);
			element.SetAttribute ("useSocialLogin", this.UseSocialLogin);
			element.SetAttribute ("address", this.Address);
			if (this.UseSocialLogin)
				element.SetAttribute ("userId", Convert.ToBase64String (ProtectedData.Protect (Encoding.UTF8.GetBytes (this.UserId ?? string.Empty), saltBytes, DataProtectionScope.CurrentUser)));
			else {
				element.SetAttribute ("userName", Convert.ToBase64String (ProtectedData.Protect (Encoding.UTF8.GetBytes (this.UserName ?? string.Empty), saltBytes, DataProtectionScope.CurrentUser)));
				element.SetAttribute ("password", Convert.ToBase64String (ProtectedData.Protect (Encoding.UTF8.GetBytes (this.Password ?? string.Empty), saltBytes, DataProtectionScope.CurrentUser)));
			}
		}
	}
}

