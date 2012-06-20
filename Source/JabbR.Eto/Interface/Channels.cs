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
		TreeItemCollection items = new TreeItemCollection();
		ConcurrentDictionary<string, Control> sectionCache = new ConcurrentDictionary<string, Control> ();
		
		public Configuration Config { get; private set; }

		public event EventHandler<EventArgs> ChannelChanged;
		
		public Server SelectedServer { 
			get {
				var item = channelList.SelectedItem as TreeItem;
				if (item != null) return item.Tag as Server;
				else return null;
			}
		}

		protected virtual void OnChannelChanged (EventArgs e)
		{
			if (ChannelChanged != null)
				ChannelChanged (this, e);
		}
		
		public Channels (Configuration config)
		{
			this.Config = config;
			channelList = new TreeView { Style = "mainList" };
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
				var action = new Actions.EditServer(this, Config);
				action.Activate ();
			}
		}

		public void Initialize ()
		{
			PopulateServers();
			foreach (var server in Config.Servers)
			{
				server.Connected += HandleConnected;
			}
		}
		
		void HandleConnected (object sender, EventArgs e)
		{
			var server = sender as Server;
			var item = items.FirstOrDefault (r => r.Key == server.Id) as TreeItem;
			if (item != null) {
				item.Children.Clear ();
				PopulateChannels(item, server);
				channelList.DataStore = items;
			}
		}
		
		void HandleServerRemoved (object sender, ServerEventArgs e)
		{
			var section = items.FirstOrDefault(r => r.Key == e.Server.Id);
			if (section != null) {
				items.Remove (section);
				channelList.DataStore = items;
			}
		}
		
		ITreeItem CreateItem (Channel channel)
		{
			return new TreeItem { Key = channel.Id, Text = channel.Name, Tag = channel };
		}
		
		void PopulateChannels (TreeItem item, Server server)
		{
			foreach (var channel in server.Channels.OrderBy (r => r.Name)) {
				item.Children.Add (CreateItem (channel));
			}
		}

		ITreeItem CreateItem (Server server)
		{
			var item = new TreeItem { Key = server.Id, Text = server.Name, Tag = server, Expanded = true, Image = Bitmap.FromResource ("JabbR.Eto.Resources.server.png") };
			PopulateChannels (item, server);
			return item;
		}
		void HandleServerAdded (object sender, ServerEventArgs e)
		{
			items.Add (CreateItem(e.Server));
			channelList.DataStore = items;
		}
		
		void PopulateServers()
		{
			foreach (var server in Config.Servers)
			{
				items.Add (CreateItem (server));
			}
			channelList.DataStore = items;
			if (channelList.SelectedItem == null && items.Count > 0) {
				channelList.SelectedItem = items[0];
			}
		}

		public Control CreateChannel ()
		{
			if (channelList.SelectedItem == null)
				return null;
			
			ISectionGenerator generator = null;
			var item = channelList.SelectedItem as TreeItem;
			generator = item.Tag as ISectionGenerator;

			Control section = null;
			if (generator != null && !sectionCache.TryGetValue (item.Key, out section)) {
				section = generator.GenerateSection ();
				while (!sectionCache.TryAdd (item.Key, section)) {
					Thread.Sleep (0);
				}
			}

			return section;
		}
	}

}

