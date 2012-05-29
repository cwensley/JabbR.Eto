using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;
using System.Linq;
using JabbR.Eto.Sections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace JabbR.Eto
{
	public class Channels : Panel
	{
		ListBox channelList;
		ConcurrentDictionary<string, ChannelSection> channelCache = new ConcurrentDictionary<string, ChannelSection> ();
		ConnectionInfo info;

		public event EventHandler<EventArgs> ChannelChanged;

		protected virtual void OnChannelChanged (EventArgs e)
		{
			if (ChannelChanged != null)
				ChannelChanged (this, e);
		}
		
		public Channels ()
		{
			channelList = new ListBox{ Style = "mainList" };
			channelList.SelectedIndexChanged += (sender, e) => {
				OnChannelChanged (e);
			};
			
			this.AddDockedControl (channelList);
		}

		public ChannelSection CreateChannel ()
		{
			string roomName = null;
			if (channelList.SelectedValue != null) {
				var item = channelList.SelectedValue as ListItem;
				roomName = item.Text;
			}
			ChannelSection channel = null;
			if (roomName != null && !channelCache.TryGetValue (roomName, out channel)) {
				channel = new ChannelSection (info, roomName);
				channel.Command += HandleCommand;
				while (!channelCache.TryAdd (roomName, channel)) {}
			}
			return channel;
		}

		void HandleCommand (object sender, CommandEventArgs e)
		{
			if (e.Command.StartsWith ("/join ")) {
				
				var roomName = e.Command.Substring (6);
				info.Client.JoinRoom (roomName).ContinueWith (task => {
					if (task.Exception != null) {
						Console.WriteLine ("Error joining room {0}, {1}", roomName, task.Exception);
						return;
					}
					info.Client.GetRoomInfo (roomName).ContinueWith (task2 => {
						if (task2.Exception != null) {
							Console.WriteLine ("Error getting room info {0}, {1}", roomName, task2.Exception);
							// show error
							return;
						}
						var room = task2.Result;
						Application.Instance.Invoke (() => {
							var item = new ListItem{ Text = room.Name };
							channelList.Items.Add (item);
							channelList.SelectedValue = item;
						}
						);
					}
					);
				}
				);
			}
		}
		
		public void Initialize (ConnectionInfo info)
		{
			if (this.info == info) return;
			this.info = info;
			ChannelSection channel;
			
			info.Client.MessageReceived += (message, room) => {
				Console.WriteLine ("MessageReceived, Room: {3}, When: {0}, User: {1}, Content: {2}", message.When, message.User.Name, message.Content, room);
				if (channelCache.TryGetValue (room, out channel)) {
					channel.MessageReceived (message);
				}
			};
			info.Client.Disconnected += () => {
				Console.WriteLine ("Disconnected");
			};
			info.Client.FlagChanged += (user, flag) => {
				Console.WriteLine ("FlagChanged, User: {0}, Flag: {1}", user.Name, flag);
			};
			info.Client.GravatarChanged += (user, gravatar) => {
				Console.WriteLine ("GravatarChanged, User: {0}, Gravatar: {1}", user.Name, gravatar);
			};
			info.Client.Kicked += (user) => {
				Console.WriteLine ("Kicked: {0}", user);
			};
			info.Client.LoggedOut += (rooms) => {
				Console.WriteLine ("LoggedOut, Rooms: {0}", string.Join (", ", rooms));
			};
			info.Client.MeMessageReceived += (user, content, room) => {
				Console.WriteLine ("MeMessageReceived, User: {0}, Content: {1}, Room: {2}", user, content, room);
			};
			info.Client.NoteChanged += (user, room) => {
				Console.WriteLine ("NoteChanged, User: {0}, Room: {1}", user.Name, room);
			};
			info.Client.OwnerAdded += (user, room) => {
				Console.WriteLine ("OwnerAdded, User: {0}, Room: {1}", user.Name, room);
			};
			info.Client.OwnerRemoved += (user, room) => {
				Console.WriteLine ("OwnerAdded, User: {0}, Room: {1}", user.Name, room);
			};
			info.Client.PrivateMessage += (from, to, message) => {
				Console.WriteLine ("PrivateMessage, From: {0}, To: {1}, Message: {2} ", from, to, message);
			};
			info.Client.RoomCountChanged += (room, count) => {
				Console.WriteLine ("RoomCountChanged, Room: {0}, Count: {1}", room.Name, count);
			};
			info.Client.TopicChanged += (room) => {
				Console.WriteLine ("TopicChanged, Room: {0}, Topic: {1}", room.Name, room.Topic);
			};
			info.Client.UserActivityChanged += (user) => {
				Console.WriteLine ("UserActivityChanged, User: {0}, Activity: {1}", user.Name, user.Active);
			};
			info.Client.UserJoined += (user, room) => {
				Console.WriteLine ("UserJoined, User: {0}, Room: {1}", user.Name, room);
				if (channelCache.TryGetValue (room, out channel)) {
					channel.UserJoined (user);
				}
			};
			info.Client.UserLeft += (user, room) => {
				Console.WriteLine ("UserLeft, User: {0}, Room: {1}", user.Name, room);
				if (channelCache.TryGetValue (room, out channel)) {
					channel.UserLeft (user);
				}
			};
			info.Client.UsernameChanged += (oldUserName, user, room) => {
				Console.WriteLine("UsernameChanged, OldUserName: {0}, NewUserName: {1}, Room: {2}", oldUserName, user.Name, room);
			};
			info.Client.UsersInactive += (users) => {
				Console.WriteLine("UsersInactive, Users: {0}", string.Join (", ", users.Select (r => r.Name)));
			};
			info.Client.UserTyping += (user, room) => {
				Console.WriteLine ("UserTyping, User: {0}, Room: {1}", user.Name, room);	
			};
		}
		
		public void Connected ()
		{
			if (info.LogOnInfo != null) {
				var items = new ListItemCollection ();
				foreach (var room in info.LogOnInfo.Rooms) {
					items.Add (new ListItem{ Text = room.Name });
				}
				items.Sort ((x, y) => x.Text.CompareTo(y.Text));
				channelList.DataStore = items;
				if (items.Count > 0)
					channelList.SelectedIndex = 0;
			}
		}
	}
}

