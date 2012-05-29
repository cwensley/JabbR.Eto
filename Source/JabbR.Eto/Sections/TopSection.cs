using System;
using Eto.Forms;
using JabbR.Client;
using JabbR.Client.Models;
using Eto;

namespace JabbR.Eto.Sections
{
	public class TopSection : Panel, IXmlReadable
	{
		Splitter splitter;
		Channels channels;
		//ChannelSection channel;
		
		public TopSection ()
		{
			channels = new Channels ();
			channels.ChannelChanged += HandleChannelChanged;
			
			splitter = new Splitter{
				Panel1 = channels,
				Panel2 = channels.CreateChannel (),
				Position = 200
			};
			
			this.AddDockedControl (splitter);
		}
		
		public void Initialize(ConnectionInfo info)
		{
			channels.Initialize (info);
		}
		
		public void Connected ()
		{
			channels.Connected ();
		}
		
		void HandleChannelChanged (object sender, EventArgs e)
		{
			splitter.Panel2 = channels.CreateChannel ();
		}
		
		#region IXmlReadable implementation
		
		public void ReadXml (System.Xml.XmlElement element)
		{
			splitter.Position = element.GetIntAttribute ("split") ?? 200;
		}

		public void WriteXml (System.Xml.XmlElement element)
		{
			element.SetAttribute ("split", splitter.Position);
		}
		
		#endregion
	}
}

