using System;
using Eto.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JabbR.Eto.Model;
using System.Collections.Specialized;
using System.Threading;
using Eto.Drawing;

namespace JabbR.Eto.Interface
{
	public class Channels : Panel
	{
		TreeView channelList;
		TreeItemCollection servers = new TreeItemCollection();
		ConcurrentDictionary<string, Control> sectionCache = new ConcurrentDictionary<string, Control> ();
		
		public Configuration Config { get; private set; }

		public event EventHandler<EventArgs> ChannelChanged;
		
		public Channel SelectedChannel
		{
			get {
				return channelList.SelectedItem as Channel;
			}
		}
		
		public Server SelectedServer { 
			get {
				if (SelectedChannel != null)
					return SelectedChannel.Server;
				else
					return channelList.SelectedItem as Server;
			}
		}

		protected virtual void OnChannelChanged (EventArgs e)
		{
			if (ChannelChanged != null)
				ChannelChanged (this, e);
			var selected = SelectedChannel;
			if (selected != null)
				selected.ResetUnreadCount ();
		}
		
		public Channels (Configuration config)
		{
			this.Config = config;
			channelList = new TreeView { Style = "channelList" };
			channelList.DataStore = servers;
			channelList.SelectionChanged += (sender, e) => {
				Application.Instance.Invoke (delegate {
					OnChannelChanged (e);
				});
			};
			channelList.MouseDoubleClick += HandleMouseDoubleClick;
			
			config.ServerAdded += HandleServerAdded;
			config.ServerRemoved += HandleServerRemoved;
			
			this.AddDockedControl (channelList);
		}

		void HandleMouseDoubleClick (object sender, MouseEventArgs e)
		{
			var server = this.SelectedServer;
			if (server != null) {
				var action = new Actions.EditServer (this, Config);
				action.Activate ();
			}
		}

		public void Initialize ()
		{
			servers.Clear ();
			servers.AddRange (Config.Servers);
			foreach (var server in Config.Servers) {
				server.Connected += HandleConnected;
				server.OpenChannel += HandleOpenChannel;
				server.CloseChannel += HandleCloseChannel;
				server.ChannelInfoChanged += HandleChannelInfoChanged;
			}
			Update (true);
			channelList.SelectedItem = servers.FirstOrDefault ();
		}

		void UnRegister (Server server)
		{
			server.Connected -= HandleConnected;
			server.OpenChannel -= HandleOpenChannel;
			server.CloseChannel -= HandleCloseChannel;
			server.ChannelInfoChanged -= HandleChannelInfoChanged;
		}

		void HandleCloseChannel (object sender, ChannelEventArgs e)
		{
			Application.Instance.AsyncInvoke (delegate {
				var isSelected = channelList.SelectedItem == e.Channel;
				
				Update ();
				if (isSelected)
					channelList.SelectedItem = e.Channel.Server;
			});
		}

		void HandleOpenChannel (object sender, ChannelEventArgs e)
		{
			Application.Instance.AsyncInvoke (delegate {
				
				Update ();
				
				channelList.SelectedItem = e.Channel;
			});
		}
		
		void HandleChannelInfoChanged (object sender, ChannelEventArgs e)
		{
			if (SelectedChannel == e.Channel) {
				e.Channel.ResetUnreadCount();
			}
			Update ();
		}
		
		void HandleConnected (object sender, EventArgs e)
		{
			Update ();
		}
		
		void HandleServerRemoved (object sender, ServerEventArgs e)
		{
			UnRegister (e.Server);
			servers.Remove (e.Server);
			Update (true);
		}
		
		void HandleServerAdded (object sender, ServerEventArgs e)
		{
			servers.Add (e.Server);
			Update (true);
		}
		
		public Control CreateChannel ()
		{
			if (channelList.SelectedItem == null)
				return null;
			
			var item = channelList.SelectedItem;
			var generator = item as ISectionGenerator;

			Control section = null;
			if (generator != null && !sectionCache.TryGetValue (item.Key, out section)) {
				section = generator.GenerateSection ();
				while (!sectionCache.TryAdd (item.Key, section)) {
					Thread.Sleep (0);
				}
			}

			return section;
		}
		
		IEnumerable<Channel> EnumerateChannels ()
		{
			foreach (var item in Config.Servers) {
				foreach (var channel in item.Channels) {
					yield return channel;
				}
			}
		}
		
		void NavigateChannel (bool unreadOnly, bool reverse)
		{
			var channels = EnumerateChannels ();
			var listOfItems = channels;
			var channel = channelList.SelectedItem as Channel;
			if (channel != null)
				listOfItems = channels.SkipWhile (r => r != channel).Skip (1).Union (channels.TakeWhile (r => r != channel));
			
			if (reverse)
				listOfItems = listOfItems.Reverse ();
			
			var next = listOfItems.FirstOrDefault (r => !unreadOnly || r.UnreadCount > 0);
			if (next != null)
				channelList.SelectedItem = next;
		}
		
		public void GoToNextChannel (bool unreadOnly)
		{
			NavigateChannel (unreadOnly, false);
		}
		
		public void GoToPreviousChannel (bool unreadOnly)
		{
			NavigateChannel (unreadOnly, true);
		}
		
		void Update (bool sort = false)
		{
			if (sort)
				servers.Sort ((x,y) => x.Text.CompareTo(y.Text));
			channelList.RefreshData ();
		}
		
		public void CreateActions (GenerateActionArgs args)
		{
			args.Actions.Add (new Actions.NextChannel (this));
			args.Actions.Add (new Actions.NextUnreadChannel (this));
			args.Actions.Add (new Actions.PrevChannel (this));
			args.Actions.Add (new Actions.PrevUnreadChannel (this));
			
			var channel = args.Menu.FindAddSubMenu ("&Channel", 800);
			
			channel.Actions.Add (Actions.NextChannel.ActionID);
			channel.Actions.Add (Actions.NextUnreadChannel.ActionID);
			channel.Actions.Add (Actions.PrevChannel.ActionID);
			channel.Actions.Add (Actions.PrevUnreadChannel.ActionID);
		}
	}

}

