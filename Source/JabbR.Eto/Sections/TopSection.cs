using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;

namespace JabbR.Eto.Sections
{
	public class TopSection : Panel
	{
		Splitter splitter;
		Channels channels;
		
		public TopSection ()
		{
			channels = new Channels ();
			channels.ChannelChanged += HandleChannelChanged;
			
			splitter = new Splitter{
				Panel1 = channels,
				Panel2 = channels.CreateChannelPanel (),
				Position = 200
			};
			
			this.AddDockedControl (splitter);
		}
		
		public void Initialize(JabbRClient client, LogOnInfo info)
		{
			channels.Initialize (client, info);
		}
		
		void HandleChannelChanged (object sender, EventArgs e)
		{
			splitter.Panel2 = channels.CreateChannelPanel ();
		}
		
	}
}

