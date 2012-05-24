using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;
using System.Linq;

namespace JabbR.Eto
{
	public class Channels : Panel
	{
		ListBox channelList;

		public event EventHandler<EventArgs> ChannelChanged;

		protected virtual void OnChannelChanged (EventArgs e)
		{
			if (ChannelChanged != null)
				ChannelChanged (this, e);
		}
		
		public Channels ()
		{
			channelList = new ListBox{ Style = "mainList" };
			
			this.AddDockedControl (channelList);
		}

		public Control CreateChannelPanel ()
		{
			return new Sections.ChannelSection ();
		}
		
		public void Initialize (JabbRClient client, LogOnInfo info)
		{
			var items = new ListItemCollection ();
			foreach (var room in info.Rooms) {
				items.Add (new ListItem{ Text = room.Name, Tag = room});
			}
			channelList.DataStore = items;
		}
	}
}

